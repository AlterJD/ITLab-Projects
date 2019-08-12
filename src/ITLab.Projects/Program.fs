module ITLab.Projects.App

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.EntityFrameworkCore
open Giraffe
open ITLab.Projects.ProjectHttpHandlers
open Microsoft.Extensions.Configuration
open ITLab.Projects.Database
open WebApp.Configure.Models;
open Microsoft.AspNetCore.Http.Features
open Microsoft.AspNetCore.Http
open Giraffe.Serialization
open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open ITLab.Projects.TagsHttpHandlers
open Microsoft.AspNetCore.Authentication.JwtBearer

// ---------------------------------
// Web app
// ---------------------------------

let allowSynchronousIO  : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let logger = ctx.GetService<ILoggerFactory>().CreateLogger("routing")
        logger.LogWarning "Allow Synchronous IO" |> ignore
        ctx.Features.Get<IHttpBodyControlFeature>().AllowSynchronousIO <- true
        next ctx

let mustBeLoggedIn = requiresAuthentication (challenge "Bearer")
        
let webApp =
    mustBeLoggedIn >=> subRoute "/api/projects" 
        (choose [
            subRoute "/tags" (choose [
                GET >=> allTags
                POST >=> allowSynchronousIO 
                     >=> bindJson<TagRequests.CreateEdit> addTag
            ])

            subRoutef "/%O" (fun (projectId:Guid) -> 
                choose [
                    subRoutef "/tags/%O" (fun (tagId:Guid) ->
                        choose [
                            POST >=> (addTagToProject projectId tagId)
                            DELETE >=> (removeTagFromProject projectId tagId) ] )

                    subRoute "" 
                        (choose [
                            GET >=> (oneProject projectId)
                            PUT >=> allowSynchronousIO >=> (editProject projectId)
                            DELETE >=> (removeProject projectId) ]) ])
            subRoute "" 
                (choose [
                    GET >=> allprojects
                    POST >=> allowSynchronousIO >=> addProject ]) ])

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
       .UseAuthentication()
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

let configureJwt (configuration: IConfiguration) (options: JwtBearerOptions) =
    options.Authority <- configuration.GetValue<string> "JWT:Authority"
    options.RequireHttpsMetadata <- false
    options.Audience <- "itlab.projects"

let configureServices (configuration: IConfiguration) (services : IServiceCollection) =
    match configuration.GetValue<string> "DB_TYPE"  with
    | "POSTGRES" -> 
        let connectionString = "Postgres" |> configuration.GetConnectionString
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

    let customSettings = JsonSerializerSettings(
                            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                            ContractResolver = CamelCasePropertyNamesContractResolver()
                            //, 
                            //NullValueHandling = NullValueHandling.Ignore
                            )

    services.AddSingleton<IJsonSerializer>(
        NewtonsoftJsonSerializer(customSettings)) |> ignore
    services.AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", Action<JwtBearerOptions> (configureJwt configuration) ) |> ignore


let configureLogging (builder : ILoggingBuilder) =
    builder.AddFilter(fun l -> l > LogLevel.Trace)
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