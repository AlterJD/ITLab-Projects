module Tests

open System
open System.IO
open System.Net
open System.Net.Http
open Xunit
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration

// ---------------------------------
// Helper functions (extend as you need)
// ---------------------------------

let config = dict[
    ("DB_TYPE","IN_MEMORY")
    
    ]

let createHost() =
    let configuration = ConfigurationBuilder().AddInMemoryCollection(config).Build()
    WebHostBuilder()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .Configure(Action<IApplicationBuilder> ITLab.Projects.App.configureApp)
        .ConfigureServices(Action<IServiceCollection> (ITLab.Projects.App.configureServices configuration))

let runTask task =
    task
    |> Async.AwaitTask
    |> Async.RunSynchronously

let httpGet (path : string) (client : HttpClient) =
    path
    |> client.GetAsync
    |> runTask

let isStatus (code : HttpStatusCode) (response : HttpResponseMessage) =
    Assert.Equal(code, response.StatusCode)
    response

let ensureSuccess (response : HttpResponseMessage) =
    if not response.IsSuccessStatusCode
    then response.Content.ReadAsStringAsync() |> runTask |> failwithf "%A"
    else response

let readText (response : HttpResponseMessage) =
    response.Content.ReadAsStringAsync()
    |> runTask

let shouldEqual expected actual =
    Assert.Equal(expected, actual)

let shouldContain (expected : string) (actual : string) =
    Assert.True(actual.Contains expected)

// ---------------------------------
// Tests
// ---------------------------------

// EF core 3 throws exception while user In memoery database, close tests

//[<Fact>]
//let ``Route /api/projects returns empty array`` () =
//    use server = new TestServer(createHost())
//    use client = server.CreateClient()

//    client
//    |> httpGet "/api/projects"
//    |> ensureSuccess
//    |> readText
//    |> shouldContain "[]"

//[<Fact>]
//let ``Route which doesn't exist returns 404 Page not found`` () =
//    use server = new TestServer(createHost())
//    use client = server.CreateClient()

//    client
//    |> httpGet "/route/which/does/not/exist"
//    |> isStatus HttpStatusCode.NotFound