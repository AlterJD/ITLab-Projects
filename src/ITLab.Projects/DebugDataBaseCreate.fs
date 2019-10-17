namespace ITLab.Projects

open ITLab.Projects.Database
open ITLab.Projects.Models
open System
open System.Threading.Tasks
open WebApp.Configure.Models.Configure.Interfaces;


module DebugDataBaseCreate =
    let projectId = Guid.Parse("b34b7186-1735-45d5-9756-857fd89ffe02")
    let tagId = Guid.Parse("b34b7186-1735-45d5-9756-857fd89ffe03")

    
    let fillProjects (dbContext: ProjectsContext) = 
        let project = dbContext.Projects.Find(projectId)
        match wrapOption project with
        | Some _ -> 0
        | None ->
            let project = {
                Id = projectId
                Name = "Test project"
                ShortDescription = "Test short description"
                Description = "Full size description"
                CreateTime = DateTime.UtcNow
                GitRepoLink = "https://github.com/ITLabRTUMIREA/ITLab-Projects"
                TasksLink = ""
                LogoLink = ""
                CreatorId = Guid.Empty
                ProjectTags = new ResizeArray<ProjectTag>()
                Participations = new ResizeArray<Participation>()
            }
            dbContext.Projects.Add project |> ignore
            dbContext.SaveChanges()
        
    let fillTags (dbContext: ProjectsContext) =
        let tag = dbContext.Tags.Find(tagId)
        match wrapOption tag with
        | Some _ -> 0
        | None ->
            let tag = {
                Id = tagId
                Color = "#ffffff"
                Value = "C#"
                ProjectTags = new ResizeArray<ProjectTag>()
            }
            dbContext.Tags.Add tag |> ignore
            dbContext.SaveChanges()
        
    let fillProjectTagLinks (dbContext: ProjectsContext) =
        let ptLink = dbContext.ProjectTags.Find(projectId, tagId)
        match wrapOption ptLink with
        | Some _ -> 0
        | None ->
            let project = dbContext.Projects.Find(projectId)
            let tag = dbContext.Tags.Find(tagId)
            let ptLink = {
                ProjectId = projectId
                Project = project
                TagId = tagId
                Tag = tag
            }
            dbContext.ProjectTags.Add ptLink |> ignore
            dbContext.SaveChanges()

    let fillDb (dbContext: ProjectsContext) =
        fillProjects dbContext  +
        fillTags dbContext +
        fillProjectTagLinks dbContext

    type FillDatabaseWork(context : ProjectsContext) =
        
        interface IConfigureWork with
            member this.Configure() =
                fillDb context |> ignore
                Task.CompletedTask

