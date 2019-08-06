namespace ITLab.Projects

open ITLab.Projects.Database
open ProjectResponses
open System
open System.Linq
open System.Collections.Generic

module TagsHttpHandlers =

    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Giraffe
    open ITLab.Projects.Models
    open Microsoft.FSharp.Linq.RuntimeHelpers

    let tryParseInt s = 
        try 
            s |> int |> Some
        with :? FormatException -> 
            None
    
    let wrapOption value =
        if (box value = null) then None else Some(value)

    let getIntQueryValue (ctx : HttpContext) name defaultVal =
        ctx.TryGetQueryStringValue name
            |> Option.defaultValue "incorrect"
            |> tryParseInt
            |> Option.defaultValue defaultVal

    let allTags =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let limit = getIntQueryValue ctx "limit" 10
            let start = getIntQueryValue ctx "start" 0

            let db = ctx.GetService<ProjectsContext>()

            query {
                for tag in db.Tags do
                select tag
                count
            }
            |> ctx.SetHttpHeader "x-total-count" |> ignore

            task {
                let tags = query {
                    for tag in db.Tags do
                        sortBy tag.Value
                        skip start
                        take limit
                        select tag.Value
                }
                return! json tags next ctx
            }

    let addTagToProject (id: Guid) = 
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let db = ctx.GetService<ProjectsContext>()
            task {
                let! wantedTag = ctx.BindJsonAsync<string>()
                let project = query {
                    for p in db.Projects do
                        where (p.Id = id)
                        select (p, p.ProjectTags.Select(fun pt -> pt.Tag).ToList())
                        exactlyOneOrDefault
                }
                
                match wrapOption project with
                | None -> return! RequestErrors.NOT_FOUND "not found project" next ctx
                | Some project ->
                    let project, tags = project

                    if tags.Any(fun t -> t.Value = wantedTag)
                    then return! json wantedTag next ctx
                    else
                        let tag = query {
                            for projectTag in db.ProjectTags do
                                where (projectTag.Tag.Value = wantedTag)
                                select projectTag.Tag
                                exactlyOneOrDefault
                        }

                        match wrapOption tag with
                        | Some tag ->
                            let projTag = {
                                ProjectId = id
                                TagId = tag.Id
                                Project = project
                                Tag = tag
                            }
                            db.ProjectTags.Add(projTag) |> ignore
                            let! saved = db.SaveChangesAsync()
                            return! json wantedTag next ctx
                        | None ->
                            let tag = {
                                Id = Guid.NewGuid()
                                Value = wantedTag
                                ProjectTags = new ResizeArray<ProjectTag>()
                            }
                            let projTag = {
                                ProjectId = id
                                TagId = tag.Id
                                Project = project
                                Tag = tag
                            }
                            db.Tags.Add(tag) |> ignore
                            db.ProjectTags.Add(projTag) |> ignore
                            let! saved = db.SaveChangesAsync()
                            return! json wantedTag next ctx
            }
    let removeTagFromProject (id: Guid) = 
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let db = ctx.GetService<ProjectsContext>()
            task {
                let! wantedTag = ctx.BindJsonAsync<string>()

                let targetLink = query {
                    for pt in db.ProjectTags do
                    where (pt.ProjectId = id && pt.Tag.Value = wantedTag)
                    exactlyOneOrDefault
                }

                match wrapOption targetLink with
                | None -> return! json wantedTag next ctx
                | Some link ->
                    db.ProjectTags.Remove(link) |> ignore
                    let! saved = db.SaveChangesAsync()
                    return! json wantedTag next ctx
            }
            

      