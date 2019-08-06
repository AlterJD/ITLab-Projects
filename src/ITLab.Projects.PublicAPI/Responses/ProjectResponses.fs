module ProjectResponses

open System

[<CLIMutable>]
type Compact = {
    Id: Guid
    Name: string
    ShortDescription: string
    CreateTime: DateTime
    GitRepoLink: string
    TasksLink: string
    LogoLink: string
}