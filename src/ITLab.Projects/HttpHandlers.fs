namespace ITLab.Projects

open ITLab.Projects.Database
open System

module HttpHandlers =

    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Giraffe
    open ITLab.Projects.Models

    let tryParseInt s = 
        try 
            s |> int |> Some
        with :? FormatException -> 
            None
    
    let getIntQueryValue (ctx : HttpContext) name defaultVal =
        ctx.TryGetQueryStringValue name
            |> Option.defaultValue "incorrect"
            |> tryParseInt
            |> Option.defaultValue defaultVal

    let allprojects =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let limit = getIntQueryValue ctx "limit" 0
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
                        select project
                }
                return! json projects next ctx
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

            

      