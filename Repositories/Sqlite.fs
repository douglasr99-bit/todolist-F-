module TodoList.Repositories.Sqlite

open System
open Microsoft.Data.Sqlite
open TodoList.Domain

let private schema =
    """
    CREATE TABLE IF NOT EXISTS todos (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        title VARCHAR(255) NOT NULL,
        weekday VARCHAR(20) NOT NULL,
        priority VARCHAR(10),
        completed BOOLEAN
    );"""

/// Converte valores do F# (incluindo null e option) para parâmetros do banco.
let private paraDb (valor: obj) =
    match valor with
    | null -> box DBNull.Value
    | :? option<string> as opcao -> opcao |> Option.map box |> Option.defaultValue (box DBNull.Value)
    | v -> v

/// Função de alta ordem: abre a conexão, monta o comando e entrega para `acao`.
/// Toda a parte de recursos (conexão, comando) fica concentrada em um único lugar.
let private executar (connectionString: string) (sql: string) (parametros: (string * obj) list) (acao: SqliteCommand -> 'T) : 'T =
    use conexao = new SqliteConnection(connectionString)
    conexao.Open()
    use comando = conexao.CreateCommand()
    comando.CommandText <- sql
    parametros |> List.iter (fun (nome, valor) -> comando.Parameters.AddWithValue(nome, paraDb valor) |> ignore)
    acao comando

let private mapearTarefa (leitor: SqliteDataReader) : Todo =
    let coluna = leitor.GetOrdinal

    { Id = leitor.GetInt32(coluna "id")
      Title = leitor.GetString(coluna "title")
      Weekday = leitor.GetString(coluna "weekday")
      Priority =
        if leitor.IsDBNull(coluna "priority") then None
        else Some(leitor.GetString(coluna "priority"))
      Completed = not (leitor.IsDBNull(coluna "completed")) && leitor.GetBoolean(coluna "completed") }

let private lerTarefas (comando: SqliteCommand) : Todo list =
    use leitor = comando.ExecuteReader()
    [ while leitor.Read() do
          mapearTarefa leitor ]

let private naoConsulta (comando: SqliteCommand) = comando.ExecuteNonQuery() |> ignore

let criar (connectionString: string) : TodoRepository =
    // Fixa a connection string uma vez só (com os parâmetros explícitos para continuar genérica).
    let executar sql parametros acao = executar connectionString sql parametros acao

    executar schema [] naoConsulta

    { Salvar =
        fun tarefa ->
            let id =
                executar
                    "INSERT INTO todos (title, weekday, priority, completed)
                     VALUES ($title, $weekday, $priority, $completed)
                     RETURNING id"
                    [ "$title", box tarefa.Title
                      "$weekday", box tarefa.Weekday
                      "$priority", box tarefa.Priority
                      "$completed", box tarefa.Completed ]
                    (fun comando -> comando.ExecuteScalar() |> Convert.ToInt32)

            Todo.criar id tarefa

      ListarTodos = fun () -> executar "SELECT * FROM todos" [] lerTarefas

      Atualizar =
        fun todo ->
            executar
                "UPDATE todos SET completed = $completed WHERE id = $id"
                [ "$completed", box todo.Completed; "$id", box todo.Id ]
                naoConsulta

      Remover = fun id -> executar "DELETE FROM todos WHERE id = $id" [ "$id", box id ] naoConsulta

      BuscarPorId =
        fun id -> executar "SELECT * FROM todos WHERE id = $id" [ "$id", box id ] lerTarefas |> List.tryHead }
