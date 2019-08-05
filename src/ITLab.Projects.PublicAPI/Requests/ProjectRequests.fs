module ProjectRequests

open System

[<CLIMutable>]
type Create = {
    Name: string
    Description: string
    CreateTime: DateTime
    GitRepoLink: string
    TasksLink: string
    LogoLink: string
}