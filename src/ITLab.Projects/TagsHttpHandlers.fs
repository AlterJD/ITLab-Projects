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


    let checkTagColor color = match color with
                                | Regex @"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$" _ -> true
                                | _  -> false

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
                            TagResponses.Full.UseCount = tag.ProjectTags.Count
                        }
                }
                return! json tags next ctx
            }

    let addTag (request: TagRequests.CreateEdit) =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            if not (checkTagColor request.Color) then
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
    let editTag (tagId: Guid) (request: TagRequests.CreateEdit) = 
        fun (next: HttpFunc) (ctx: HttpContext) ->
            if not (String.IsNullOrEmpty request.Color) && not (checkTagColor request.Color) then
                RequestErrors.BAD_REQUEST "incorrect color format" next ctx
            else
                let db = ctx.GetService<ProjectsContext>()
                task {
                    let! targetTag = db.Tags.FindAsync(tagId)
                    match wrapOption targetTag with
                    | None -> return! RequestErrors.NOT_FOUND "not found tag" next ctx
                    | Some _ ->
                        let newTag = { targetTag with
                                        Value = firstOrSecond request.Value targetTag.Value
                                        Color = firstOrSecond request.Color targetTag.Color }
                        db.Entry(targetTag).State <- EntityState.Detached
                        db.Tags.Update(newTag) |> ignore
                        let saved = try
                                        Some(db.SaveChanges())
                                    with
                                    | :? DbUpdateException as ex -> None
                        match saved with
                        | None -> return! RequestErrors.BAD_REQUEST "tag value exists" next ctx
                        | Some _ ->
                            return! json newTag next ctx
                }

    let removeTag (tagId: Guid) =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            let db = ctx.GetService<ProjectsContext>()
            task {
                let targetTag = query {
                    for tag in db.Tags.Include(fun t -> t.ProjectTags) do
                        where (tag.Id = tagId)
                        select tag
                        exactlyOneOrDefault
                }
                match wrapOption targetTag with
                | None -> return! RequestErrors.NOT_FOUND "no tag" next ctx
                | Some tag ->
                    match targetTag.ProjectTags.Count with
                    | count when count > 0 -> 
                        return! RequestErrors.BAD_REQUEST "tag uses in project" next ctx
                    | _ ->
                        db.Tags.Remove(tag) |> ignore
                        let! saved = db.SaveChangesAsync()
                        return! json tagId next ctx
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
                            let projectTag = {
                                ProjectId = projectId
                                TagId = tagId
                                Project = project
                                Tag = tag
                            }
                            db.ProjectTags.Add projectTag |> ignore
                            let! saved = db.SaveChangesAsync()
                            return! Successful.OK "tag added" next ctx
            }
    let removeTagFromProject (id: Guid) (tagId: Guid) = 
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let db = ctx.GetService<ProjectsContext>()
            task {

                let targetLink = query {
                    for pt in db.ProjectTags do
                    where (pt.ProjectId = id && pt.TagId = tagId)
                    exactlyOneOrDefault
                }

                match wrapOption targetLink with
                | None -> return! RequestErrors.NOT_FOUND "no link between tag and project" next ctx
                | Some link ->
                    db.ProjectTags.Remove(link) |> ignore
                    let! saved = db.SaveChangesAsync()
                    return! Successful.OK "tagremoved " next ctx
            }
            

      