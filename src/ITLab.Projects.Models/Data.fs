namespace ITLab.Projects.Models

open System
open System.Collections.Generic

[<CLIMutable>]
type Project = {
    Id: Guid
    Name: string
    Description: string
    CreateTime: DateTime
    GitRepoLink: string
    TasksLink: string
    LogoLink: string
    ProjectTags: ICollection<ProjectTag>
    Participations: ICollection<Participation>
}

and [<CLIMutable>] Tag = {
    Id: Guid
    Value: string
    ProjectTags: ICollection<ProjectTag>
}

and [<CLIMutable>] ProjectTag = {
    ProjectId: Guid
    Project: Project
    TagId: Guid
    Tag: Tag
}

and [<CLIMutable>] ProjectRole = {
    Id: Guid
    Name: string
    Description: string
    Participations: ICollection<Participation>
}

and [<CLIMutable>] Participation = {
    Id: Guid

    UserId: Guid

    ProjectId: Guid
    Project: Project

    ProjectRoleId: Guid
    ProjectRole: ProjectRole

    From: DateTime
    To: DateTime
}
