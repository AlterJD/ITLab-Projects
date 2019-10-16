namespace ITLab.Projects

open ITLab.Projects.Database
open ITLab.Projects.Models
open System

module DebugDataBaseCreate =
    
    let fillDb (dbContext: ProjectsContext) =
        let project = {
                        Id = Guid.Parse("b34b7186-1735-45d5-9756-857fd89ffe02")
                        Name = "Test project"
                        ShortDescription = "Test short description"
                        Description = "Full size description"
                        CreateTime = DateTime.UtcNow
                        GitRepoLink = "https://github.com/ITLabRTUMIREA/ITLab-Projects"
                        TasksLink = ""
                        LogoLink = ""
                        ProjectTags = new ResizeArray<ProjectTag>()
                        Participations = new ResizeArray<Participation>()
        }
        dbContext.Projects.Add project |> ignore
        dbContext.SaveChanges()

