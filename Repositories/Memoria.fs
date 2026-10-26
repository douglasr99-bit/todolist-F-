module TodoList.Repositories.Memoria

open TodoList.Domain

/// Estado imutável do repositório: um Map de tarefas e o próximo id.
type private Estado =
    { Tarefas: Map<int, Todo>
      ProximoId: int }

/// Transições puras: recebem um estado e devolvem um estado novo (e um resultado, quando há).
module private Estado =

    let vazio = { Tarefas = Map.empty; ProximoId = 1 }

    let salvar (tarefa: NovaTarefa) (estado: Estado) =
        let todo = Todo.criar estado.ProximoId tarefa
        todo,
        { Tarefas = Map.add todo.Id todo estado.Tarefas
          ProximoId = estado.ProximoId + 1 }

    let atualizar (todo: Todo) (estado: Estado) =
        if Map.containsKey todo.Id estado.Tarefas then
            { estado with Tarefas = Map.add todo.Id todo estado.Tarefas }
        else
            estado

    let remover (id: int) (estado: Estado) =
        { estado with Tarefas = Map.remove id estado.Tarefas }

let criar () : TodoRepository =
    // A única mutação fica isolada aqui: uma referência que aponta para o estado imutável atual,
    // trocada de forma atômica (lock) a cada transição.
    let atual = ref Estado.vazio
    let trava = obj ()

    let ler consulta = lock trava (fun () -> consulta atual.Value)

    let transicionar transicao =
        lock trava (fun () ->
            let resultado, novoEstado = transicao atual.Value
            atual.Value <- novoEstado
            resultado)

    { Salvar = fun tarefa -> transicionar (Estado.salvar tarefa)
      ListarTodos = fun () -> ler (fun estado -> estado.Tarefas |> Map.values |> List.ofSeq)
      Atualizar = fun todo -> transicionar (fun estado -> (), Estado.atualizar todo estado)
      Remover = fun id -> transicionar (fun estado -> (), Estado.remover id estado)
      BuscarPorId = fun id -> ler (fun estado -> Map.tryFind id estado.Tarefas) }
