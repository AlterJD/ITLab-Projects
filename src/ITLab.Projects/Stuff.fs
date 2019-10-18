[<AutoOpen>]
module Stuff

open Microsoft.AspNetCore.Http
open System
open System.Text.RegularExpressions
open Giraffe
open System.Security.Claims

let wrapOption value =
    if (box value = null) then None else Some(value)


let tryParseInt s = 
    try 
        s |> int |> Some
    with :? FormatException -> 
        None

let getIntQueryValue (ctx : HttpContext) name defaultVal =
    ctx.TryGetQueryStringValue name
        |> Option.defaultValue "incorrect"
        |> tryParseInt
        |> Option.defaultValue defaultVal

let firstOrSecond first second =
    if (box first = null) then second else first

let (|Regex|_|) pattern input =
    let m = Regex.Match(input, pattern)
    if m.Success then Some(List.tail [ for g in m.Groups -> g.Value ])
    else None

let UserId (ctx: HttpContext) =
    Guid.Parse(ctx.User.FindFirst(ClaimTypes.NameIdentifier).Value)
