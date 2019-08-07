module ProjectRequests

open System

[<CLIMutable>]
type CreateEdit = {
    Name: string
    ShortDescription: string
    Description: string
    CreateTime: DateTime
    GitRepoLink: string
    TasksLink: string
    LogoLink: string
}
