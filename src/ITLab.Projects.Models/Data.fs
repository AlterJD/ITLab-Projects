namespace ITLab.Projects.Models



open System

[<CLIMutable>]
type Project = {
    Id: Guid
    Name: string
    Description: string
    CreateTime: DateTime
}
