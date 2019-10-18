namespace ITLab.Projects

open ITLab.Projects.Database
open ProjectResponses
open System
open System.Linq
open Microsoft.EntityFrameworkCore

module ProjectHttpHandlers =

    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Giraffe
    open ITLab.Projects.Models

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
                            ProjectTags = project.ProjectTags.Select(fun pt -> {
                                TagResponses.Compact.Value = pt.Tag.Value
                                TagResponses.Compact.Color = pt.Tag.Color
                                }).ToList()
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
                         CreatorId = project.CreatorId
                         Tags = project.ProjectTags.Select(fun pt -> {
                            TagResponses.ProjectView.Id = pt.Tag.Id
                            TagResponses.ProjectView.Value = pt.Tag.Value
                            TagResponses.ProjectView.Color = pt.Tag.Color
                            }).ToList()
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
                let! model = ctx.BindJsonAsync<ProjectRequests.CreateEdit>()

                let existing = query {
                    for project in db.Projects do
                        exists (project.Name = model.Name)
                }

                match existing with
                | true ->
                    return! RequestErrors.BAD_REQUEST "project exists" next ctx
                | false ->
                    let participation = new ResizeArray<Participation>()
                    let project = {
                        Id = Guid.NewGuid()
                        Name = model.Name
                        ShortDescription = model.ShortDescription
                        Description = model.Description
                        CreateTime = DateTime.UtcNow
                        GitRepoLink = model.GitRepoLink
                        TasksLink = model.TasksLink
                        LogoLink = model.LogoLink
                        CreatorId = UserId ctx
                        ProjectTags = new ResizeArray<ProjectTag>()
                        Participations = participation
                    }
                    let! ownerRole = db.ProjectRoles.FindAsync(PredefinedDatabaseValues.OwnerRoleId)
                    participation.Add ({
                        Participation.Id = Guid.NewGuid()
                        Participation.Project = project
                        Participation.ProjectId = project.Id
                        Participation.UserId = UserId ctx
                        Participation.ProjectRoleId = PredefinedDatabaseValues.OwnerRoleId
                        Participation.ProjectRole = ownerRole
                        Participation.From = DateTime.UtcNow
                        Participation.To = None
                    })
                    db.Projects.Add(project) |> ignore
                    let! save = db.SaveChangesAsync()
                    return! json project next ctx
            }
    
    let editProject (id: Guid) =
        fun (next : HttpFunc) (ctx: HttpContext) ->
            let db = ctx.GetService<ProjectsContext>()
            task {
                let! model = ctx.BindJsonAsync<ProjectRequests.CreateEdit>()
                let! project = db.Projects.FindAsync(id);
                match wrapOption project with
                | None ->
                    return! RequestErrors.BAD_REQUEST "project not found" next ctx
                | Some project ->
                    let updated = { project with 
                                        Name = firstOrSecond model.Name project.Name
                                        ShortDescription = firstOrSecond model.ShortDescription project.ShortDescription
                                        Description = firstOrSecond model.Description project.Description
                                        GitRepoLink = firstOrSecond model.GitRepoLink project.GitRepoLink
                                        TasksLink = firstOrSecond model.TasksLink project.TasksLink
                                        LogoLink = firstOrSecond model.LogoLink project.LogoLink 
                    }
                    db.Entry(project).State <- EntityState.Detached
                    db.Update(updated) |> ignore
                    let! saved = db.SaveChangesAsync()
                    return! json updated next ctx
                
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

            

      