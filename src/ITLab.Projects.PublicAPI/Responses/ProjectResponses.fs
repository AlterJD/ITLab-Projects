module ProjectResponses

open System
open System.Collections.Generic

[<CLIMutable>]
type Compact = {
    Id: Guid
    Name: string
    ShortDescription: string
    CreateTime: DateTime
    GitRepoLink: string
    TasksLink: string
    LogoLink: string
    ProjectTags: ICollection<TagResponses.Compact>
}

[<CLIMutable>]
type Participation = {
    UserId: Guid
    RoleId: Guid
    RoleName: string
}


[<CLIMutable>]
type Full = {
    Id: Guid
    Name: string
    ShortDescription: string
    Description: string
    CreateTime: DateTime
    GitRepoLink: string
    TasksLink: string
    LogoLink: string
    Tags: ICollection<TagResponses.ProjectView>
    Participations: ICollection<Participation>
}