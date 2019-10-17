namespace ITLab.Projects.Database
module PredefinedDatabaseValues =
    open System
    open System.Threading.Tasks
    open WebApp.Configure.Models.Configure.Interfaces
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open ITLab.Projects.Models

    let OwnerRoleId = Guid.Parse("25D69881-CB1A-42D4-8C44-FF1DA83D9CE1")
    let mutable OwnerRole = {
        ProjectRole.Id = OwnerRoleId
        ProjectRole.Name = "Ответственный"
        ProjectRole.Description = "Отвечает за работу над проектом"
        ProjectRole.Participations = new ResizeArray<Participation>()
    }

    type Filler(dbContext: ProjectsContext) = 
        interface IConfigureWork with
            member this.Configure() =
                task {
                    let! role = dbContext.ProjectRoles.FindAsync(OwnerRoleId)
                    match wrapOption role with
                    | Some _ -> 
                        OwnerRole = role |> ignore
                        ()
                    | None ->
                        let role = OwnerRole
                        dbContext.ProjectRoles.Add(role) |> ignore
                        let! saved = dbContext.SaveChangesAsync()
                        ()
                } :> Task
