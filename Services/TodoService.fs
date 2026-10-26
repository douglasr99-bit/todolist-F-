/// Regras de negócio. O repositório é recebido como primeiro parâmetro
/// (injeção de dependência por aplicação parcial) e os erros voltam como Result.
module TodoList.Services.TodoService

open TodoList.Domain
open TodoList.Repositories

let private buscarTarefaExistente (repository: TodoRepository) (id: int) : Result<Todo, TodoErro> =
    match repository.BuscarPorId id with
    | Some tarefa -> Ok tarefa
    | None -> Error(TarefaNaoEncontrada id)

let criarTarefa (repository: TodoRepository) (tarefa: NovaTarefa) : Result<Todo, TodoErro> =
    tarefa |> Todo.validar |> Result.map repository.Salvar

let listarTarefas (repository: TodoRepository) : Todo list = repository.ListarTodos()

let removerTarefa (repository: TodoRepository) (id: int) : Result<unit, TodoErro> =
    id |> buscarTarefaExistente repository |> Result.map (fun tarefa -> repository.Remover tarefa.Id)

let alternarStatus (repository: TodoRepository) (id: int) : Result<unit, TodoErro> =
    id
    |> buscarTarefaExistente repository
    |> Result.map Todo.alternarStatus
    |> Result.map repository.Atualizar
