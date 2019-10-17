namespace ITLab.Projects.Stuff

module Database =
    open Microsoft.EntityFrameworkCore.Storage.ValueConversion
    open Microsoft.FSharp.Linq.RuntimeHelpers
    open System
    open System.Linq.Expressions
    
    let public fromOption<'T> =
        <@ Func<'T option, 'T>(fun (x: 'T option) -> match x with Some y -> y | None -> Unchecked.defaultof<'T>) @>
        |> LeafExpressionConverter.QuotationToExpression
        |> unbox<Expression<Func<'T option, 'T>>>

    let public toOption<'T> =
        <@ Func<'T, 'T option>(fun (x: 'T) -> match box x with null -> None | _ -> Some x) @>
        |>LeafExpressionConverter.QuotationToExpression
        |> unbox<Expression<Func<'T, 'T option>>>

