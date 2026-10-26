namespace TodoList.Repositories

open TodoList.Domain

/// Em vez de uma interface, o repositório é um record de funções.
/// Cada implementação (memória, SQLite) é só uma função que devolve esse record.
type TodoRepository =
    { Salvar: NovaTarefa -> Todo
      ListarTodos: unit -> Todo list
      Atualizar: Todo -> unit
      Remover: int -> unit
      BuscarPorId: int -> Todo option }
