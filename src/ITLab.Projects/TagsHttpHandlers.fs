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
    open Microsoft.EntityFrameworkCore

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
                        select {
                            TagResponses.Full.Id = tag.Id
                            TagResponses.Full.Value = tag.Value
                            TagResponses.Full.Color = tag.Color
                        }
                }
                return! json tags next ctx
            }

    let addTag (request: TagRequests.CreateEdit) =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let correct = match request.Color with
                            | Regex @"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$" _ -> true
                            | _  -> false
            if not correct then
                RequestErrors.BAD_REQUEST "incorrect color format" next ctx
            else
                let db = ctx.GetService<ProjectsContext>()
                task {
                    let! another = db.Tags.FirstOrDefaultAsync(fun t -> t.Value = request.Value)
                    match wrapOption another with 
                    | Some tag
                        -> return! RequestErrors.CONFLICT "tag exists" next ctx
                    | None -> 
                        let tag = {
                            Id = Guid.NewGuid()
                            Value = request.Value
                            Color = request.Color
                            ProjectTags = new ResizeArray<ProjectTag>()
                        }
    
                        db.Tags.Add(tag) |> ignore
                        let! saved = db.SaveChangesAsync()
                        return! json tag next ctx

            }

    let addTagToProject (projectId: Guid) (tagId: Guid) = 
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let db = ctx.GetService<ProjectsContext>()
            task {
                let! targetProject = db.Projects
                                        .Include(fun p -> p.ProjectTags)
                                        .Where(fun p -> p.Id = projectId)
                                        .SingleOrDefaultAsync()
                match wrapOption targetProject with
                | None -> 
                    return! RequestErrors.NOT_FOUND "no project" next ctx
                | Some project ->
                    match project.ProjectTags.Any(fun pt -> pt.TagId = tagId) with
                    | true ->
                        return! RequestErrors.BAD_REQUEST "project already contains tag" next ctx
                    | false ->
                        let! tag = db.Tags.FindAsync(tagId)
                        match wrapOption tag with
                        | None -> 
                            return! RequestErrors.NOT_FOUND "no tag" next ctx
                        | Some tag ->
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
            

      