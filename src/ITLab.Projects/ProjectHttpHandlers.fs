namespace ITLab.Projects

open ITLab.Projects.Database
open ProjectResponses
open System
open System.Linq

module ProjectHttpHandlers =

    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Giraffe
    open ITLab.Projects.Models



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

    let allprojects =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let limit = getIntQueryValue ctx "limit" 10
            let start = getIntQueryValue ctx "start" 0

            let db = ctx.GetService<ProjectsContext>()

            query {
                for project in db.Projects do
                select project
                count
            }
            |> ctx.SetHttpHeader "x-total-count" |> ignore

            task {
                let projects = query {
                    for project in db.Projects do
                        sortBy project.Name
                        skip start
                        take limit
                        select {
                            Id = project.Id
                            Name = project.Name
                            ShortDescription = project.ShortDescription
                            CreateTime = project.CreateTime
                            GitRepoLink = project.GitRepoLink
                            TasksLink = project.TasksLink
                            LogoLink = project.LogoLink
                            ProjectTags = project.ProjectTags.Select(fun pt -> pt.Tag.Value).ToList()
                        }
                }
                return! json projects next ctx
            }
    
    let oneProject (id: Guid) =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            let db = ctx.GetService<ProjectsContext>()
            task {
                let finded = query {
                   for project in db.Projects do
                      where (id = project.Id)
                      select {
                         Id = project.Id
                         Name = project.Name
                         ShortDescription = project.ShortDescription
                         Description = project.Description
                         CreateTime = project.CreateTime
                         GitRepoLink = project.GitRepoLink
                         TasksLink = project.TasksLink
                         LogoLink = project.LogoLink
                         Tags = project.ProjectTags.Select(fun pt -> pt.Tag.Value).ToList()
                         Participations = project.Participations.Select(fun p -> {
                            UserId = p.UserId
                            RoleId = p.ProjectRoleId
                            RoleName = p.ProjectRole.Name
                         }).ToList()
                      }
                      exactlyOneOrDefault
                }

                match wrapOption finded with
                | None ->
                    return! RequestErrors.NOT_FOUND "no project" next ctx
                | Some full ->
                    return! json full next ctx
            }

    let addProject = 
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let db = ctx.GetService<ProjectsContext>()
            task {
                let! model = ctx.BindJsonAsync<ProjectRequests.Create>()

                let existing = query {
                    for project in db.Projects do
                        exists (project.Name = model.Name)
                }

                match existing with
                | true ->
                    return! RequestErrors.BAD_REQUEST "project exists" next ctx
                | false ->
                    let project = {
                        Id = Guid.NewGuid()
                        Name = model.Name
                        ShortDescription = model.ShortDescription
                        Description = model.Description
                        CreateTime = DateTime.UtcNow
                        GitRepoLink = model.GitRepoLink
                        TasksLink = model.TasksLink
                        LogoLink = model.LogoLink
                        ProjectTags = new ResizeArray<ProjectTag>()
                        Participations = new ResizeArray<Participation>()
                    }

                    db.Projects.Add(project) |> ignore
                    let! save = db.SaveChangesAsync()
                    return! json project next ctx
            }
    
    let removeProject (id: Guid) = 
        fun (next: HttpFunc) (ctx: HttpContext)->
            let db = ctx.GetService<ProjectsContext>()
            task {
                let existing = query {
                    for project in db.Projects do
                        where (project.Id = id)
                        select project
                        exactlyOneOrDefault
                }
                match wrapOption existing with
                | None ->
                    return! RequestErrors.NOT_FOUND "no project" next ctx
                | Some finded ->
                    db.Projects.Remove(finded) |> ignore
                    let! save = db.SaveChangesAsync()
                    return! json id next ctx
            }

            

      