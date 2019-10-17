namespace ITLab.Projects

open Microsoft.AspNetCore.Http
open Giraffe
open ITLab.Projects.Database
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.EntityFrameworkCore


module ProjectRolesHttpHandlers =
    let allRoles =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let limit = getIntQueryValue ctx "limit" 10
            let start = getIntQueryValue ctx "start" 0

            let db = ctx.GetService<ProjectsContext>()

            query {
                for tag in db.ProjectRoles do
                select tag
                count
            }
            |> ctx.SetHttpHeader "x-total-count" |> ignore

            task {
                let roles = query {
                    for role in db.ProjectRoles do
                        skip start
                        take limit
                        select {
                            ProjectRoleResponses.Full.Id = role.Id
                            ProjectRoleResponses.Full.Name = role.Name
                            ProjectRoleResponses.Full.Description = role.Description
                        }
                }
                let! tags = roles.ToListAsync()
                return! json tags next ctx
            }
