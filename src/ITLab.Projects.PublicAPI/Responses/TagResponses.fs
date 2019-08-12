module TagResponses

open System

[<CLIMutable>]
type Compact = {
    Value: string
    /// <summary>
    /// Tag color in HEX format
    ///</summary>
    Color: string
}

[<CLIMutable>]
type Full = {
    Id: Guid
    Value: string
    /// <summary>
    /// Tag color in HEX format
    ///</summary>
    Color: string
}
