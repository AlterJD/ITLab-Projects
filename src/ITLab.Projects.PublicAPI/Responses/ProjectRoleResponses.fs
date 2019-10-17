module ProjectRoleResponses

open System

[<CLIMutable>]
type Full = {
    Id: Guid
    Name: string
    Description: string
}
