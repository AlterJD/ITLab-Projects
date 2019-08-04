namespace ITLab.Projects

open ITLab.Projects.Database

module HttpHandlers =

    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Giraffe
    open ITLab.Projects.Models

    let allprojects =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let db = ctx.GetService<ProjectsContext>()
            task {
                let projects = query {
                    for project in db.Projects do
                        select project
                }
                return! json projects next ctx
            }