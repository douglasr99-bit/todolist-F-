module TodoList.Program

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open TodoList.Config
open TodoList.Http

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)

    // Porta configurável pela variável PORT (padrão 8080).
    let porta = Environment.GetEnvironmentVariable "PORT" |> Option.ofObj |> Option.defaultValue "8080"
    builder.WebHost.UseUrls($"http://0.0.0.0:{porta}") |> ignore

    let app = builder.Build()

    // Sem contêiner de DI: o repositório é criado aqui e passado explicitamente para as rotas.
    let repository = RepositoryConfig.criarRepositorio app.Configuration

    app.UseDefaultFiles().UseStaticFiles() |> ignore
    Rotas.mapear repository app

    app.Run()
    0
