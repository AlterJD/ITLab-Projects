module ITLab.Projects.App

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.EntityFrameworkCore
open Giraffe
open ITLab.Projects.HttpHandlers
open Microsoft.Extensions.Configuration
open ITLab.Projects.Database
open WebApp.Configure.Models;
open Microsoft.AspNetCore.Http.Features
open Microsoft.AspNetCore.Http
open Giraffe.Serialization
open Newtonsoft.Json

// ---------------------------------
// Web app
// ---------------------------------


let allowSynchronousIO  : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        ctx.Features.Get<IHttpBodyControlFeature>().AllowSynchronousIO <- true
        next ctx


let webApp =
    subRoute "/api" (
        choose [
            subRoute "/projects" (
                choose [
                    GET >=> allprojects
                    POST >=> allowSynchronousIO >=> addProject
                ]
            )
        ]
    )

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder.WithOrigins("http://localhost:8080")
           .AllowAnyMethod()
           .AllowAnyHeader()
           |> ignore

let configureApp (app : IApplicationBuilder) =
    app.UseGiraffeErrorHandler(errorHandler)
       .UseCors(configureCors)
       .UseGiraffe(webApp)

type CustomNegotiationConfig (baseConfig : INegotiationConfig) =
    let plainText x = text (x.ToString())

    interface INegotiationConfig with

        member __.UnacceptableHandler =
            baseConfig.UnacceptableHandler

        member __.Rules =
                dict [
                    "*/*"             , json
                    "text/plain"      , plainText
                ]

exception InvalidDbType of string


let configureServices (configuration: IConfiguration) (services : IServiceCollection) =
    match configuration.GetValue<string>("DB_TYPE") with
    | "POSTGRES" -> 
        let connectionString = configuration.GetConnectionString("Postgres")
        services.AddDbContext<ProjectsContext>(fun o -> o.UseNpgsql connectionString |> ignore) |> ignore
    | "IN_MEMORY" -> 
        services.AddDbContext<ProjectsContext>(fun o -> o.UseInMemoryDatabase "In memory" |> ignore) |> ignore
    | _ -> raise (InvalidDbType "invalid db type")
    services.AddGiraffe() |> ignore
    services.AddSingleton<INegotiationConfig>(
           CustomNegotiationConfig(
               DefaultNegotiationConfig())
       ) |> ignore
    services.AddWebAppConfigure().AddTransientConfigure<MigrationApplyWork>() |> ignore
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore

    let customSettings = JsonSerializerSettings(DateTimeZoneHandling = DateTimeZoneHandling.Utc)

    services.AddSingleton<IJsonSerializer>(
        NewtonsoftJsonSerializer(customSettings)) |> ignore


let configureLogging (builder : ILoggingBuilder) =
    builder.AddFilter(fun l -> l.Equals LogLevel.Debug)
           .AddConsole()
           .AddDebug() |> ignore


let configuration(args: string[]) =
    (new ConfigurationBuilder())
        .AddEnvironmentVariables()
        .AddCommandLine(args)
        .AddJsonFile("appsettings.Secret.json", false)
        .Build()

[<EntryPoint>]
let main args =
    let config = configuration args;
    WebHostBuilder()
        .UseKestrel()
        .UseConfiguration(config)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices config)    
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0