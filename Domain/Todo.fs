namespace TodoList.Domain

open System

// Dados imutáveis: um record não muda depois de criado; "alterar" gera uma cópia nova.

/// Tarefa já persistida (possui id).
type Todo =
    { Id: int
      Title: string
      Weekday: string
      Priority: string option
      Completed: bool }

/// Tarefa ainda não persistida (sem id).
type NovaTarefa =
    { Title: string
      Weekday: string
      Priority: string option
      Completed: bool }

/// Erros de negócio como valores (união discriminada) em vez de exceções.
type TodoErro =
    | TituloInvalido
    | TarefaNaoEncontrada of id: int

// Funções puras sobre o domínio: só dependem da entrada e não têm efeitos colaterais.

module Todo =

    let possuiTituloValido (titulo: string) = not (String.IsNullOrWhiteSpace titulo)

    let validar (tarefa: NovaTarefa) : Result<NovaTarefa, TodoErro> =
        if possuiTituloValido tarefa.Title then Ok tarefa else Error TituloInvalido

    let criar (id: int) (tarefa: NovaTarefa) : Todo =
        { Id = id
          Title = tarefa.Title
          Weekday = tarefa.Weekday
          Priority = tarefa.Priority
          Completed = tarefa.Completed }

    let alternarStatus (todo: Todo) : Todo = { todo with Completed = not todo.Completed }

module TodoErro =

    let mensagem erro =
        match erro with
        | TituloInvalido -> "título inválido"
        | TarefaNaoEncontrada _ -> "tarefa não encontrada"
