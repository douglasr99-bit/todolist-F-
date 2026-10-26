/// Escolhe a implementação do repositório a partir da configuração "Repositorio" ("memoria" ou "banco").
module TodoList.Config.RepositoryConfig

open System
open System.IO
open Microsoft.Extensions.Configuration
open TodoList.Repositories

let private connectionStringPadrao = "Data Source=data/tododb.db"

let criarRepositorio (configuration: IConfiguration) : TodoRepository =
    let tipo = configuration["Repositorio"] |> Option.ofObj |> Option.defaultValue "banco"

    if tipo.Equals("memoria", StringComparison.OrdinalIgnoreCase) then
        Memoria.criar ()
    else
        Directory.CreateDirectory "data" |> ignore

        configuration.GetConnectionString "TodoDb"
        |> Option.ofObj
        |> Option.defaultValue connectionStringPadrao
        |> Sqlite.criar
