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
open ITLab.Projects.ProjectRolesHttpHandlers

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
        
let webAppLogic =
    subRoute "/api/projects" 
        (choose [

            subRoute "/roles"(choose [
                subRoute "" (choose [
                    GET >=> allRoles
                ])
            ]
            )

            subRoute "/tags" (choose [
                subRoutef "/%O" (fun (tagId:Guid) -> 
                    choose [
                        PUT    >=> allowSynchronousIO
                               >=> bindJson<TagRequests.CreateEdit> (editTag tagId)
                        DELETE >=> removeTag tagId])
                subRoute ""
                    (choose [
                        GET  >=> allTags
                        POST >=> allowSynchronousIO 
                             >=> bindJson<TagRequests.CreateEdit> addTag])
            ])

            subRoutef "/%O" (fun (projectId:Guid) -> 
                choose [
                    subRoutef "/tags/%O" (fun (tagId:Guid) ->
                        choose [
                            POST   >=> (addTagToProject      projectId tagId)
                            DELETE >=> (removeTagFromProject projectId tagId) ] )

                    subRoute "" 
                        (choose [
                            GET    >=> (oneProject projectId)
                            PUT    >=> allowSynchronousIO 
                                   >=> (editProject projectId)
                            DELETE >=> (removeProject projectId) ]) ])
            subRoute "" 
                (choose [
                    GET  >=> allprojects
                    POST >=> allowSynchronousIO 
                         >=> addProject ]) ])

let webApp (config: IConfiguration) = mustBeLoggedIn >=> webAppLogic
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

let configureApp (config: IConfiguration) (app : IApplicationBuilder)  =
    app.UseGiraffeErrorHandler(errorHandler)
       .UseCors(configureCors)
       .UseAuthentication()
       .UseGiraffe(config |> webApp)

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
    options.TokenValidationParameters.ValidateLifetime <- not (configuration.GetValue<bool>("TESTS"))
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

    let webAppConfiguration = services.AddWebAppConfigure()
    webAppConfiguration.AddTransientConfigure<PredefinedDatabaseValues.Filler>() |> ignore
    if (configuration.GetValue("FILL_DEBUG_DB")) then
        services.AddTransient<MigrationApplyWork>().AddTransient<DebugDataBaseCreate.FillDatabaseWork>() |> ignore
        webAppConfiguration.AddTransientConfigure<SequenceConfigureWork<MigrationApplyWork, DebugDataBaseCreate.FillDatabaseWork>>() |> ignore
    else
        webAppConfiguration.AddTransientConfigure<MigrationApplyWork>() |> ignore

    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore

    let customSettings = JsonSerializerSettings(
                            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                            ContractResolver = CamelCasePropertyNamesContractResolver(),
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
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


let buildWebHost config = 
    WebHostBuilder()
           .UseKestrel()
           .UseConfiguration(config)
           .Configure(Action<IApplicationBuilder> (configureApp config))
           .ConfigureServices(configureServices config)    
           .ConfigureLogging(configureLogging)
           .Build()


let fillDebugBD (webHost :IWebHost) = 
    let scope = webHost.Services.CreateScope()
    let db = scope.ServiceProvider.GetRequiredService<ProjectsContext>()
    DebugDataBaseCreate.fillDb db |> ignore
    scope.Dispose()

[<EntryPoint>]
let main args =
    let config = configuration args;
    let webHost = buildWebHost config;
    webHost.Run()
    0