/// Camada HTTP: traduz requisições para chamadas do serviço e Result para respostas.
module TodoList.Http.Rotas

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open TodoList.Domain
open TodoList.Repositories
open TodoList.Services

/// Corpo do POST. Vem do JSON e pode ter campos nulos; é convertido para o domínio logo na borda.
[<CLIMutable>]
type NovaTarefaRequest =
    { Title: string
      Weekday: string
      Priority: string
      Completed: bool }

let private paraNovaTarefa (request: NovaTarefaRequest) : NovaTarefa =
    { Title = request.Title
      Weekday = request.Weekday
      Priority = Option.ofObj request.Priority
      Completed = request.Completed }

/// Pattern matching substitui a hierarquia de exceções com StatusCode sobrescrito.
let private statusCode erro =
    match erro with
    | TituloInvalido -> StatusCodes.Status400BadRequest
    | TarefaNaoEncontrada _ -> StatusCodes.Status404NotFound

let private responder (sucesso: 'T -> IResult) (resultado: Result<'T, TodoErro>) : IResult =
    match resultado with
    | Ok valor -> sucesso valor
    | Error erro -> Results.Json({| erro = TodoErro.mensagem erro |}, statusCode = statusCode erro)

let private semCorpo () = Results.Ok()

let mapear (repository: TodoRepository) (app: WebApplication) =
    app.MapGet("/todos", Func<IResult>(fun () -> TodoService.listarTarefas repository |> Results.Ok))
    |> ignore

    app.MapPost(
        "/todos",
        Func<NovaTarefaRequest, IResult>(fun request ->
            request
            |> paraNovaTarefa
            |> TodoService.criarTarefa repository
            |> responder Results.Ok)
    )
    |> ignore

    app.MapPatch(
        "/todos/{id}/toggle",
        Func<int, IResult>(fun id -> TodoService.alternarStatus repository id |> responder semCorpo)
    )
    |> ignore

    app.MapDelete("/todos/{id}", Func<int, IResult>(fun id -> TodoService.removerTarefa repository id |> responder semCorpo))
    |> ignore
