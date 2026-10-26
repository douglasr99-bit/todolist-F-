# todolist

API de tarefas (To-Do List) em **F# / ASP.NET Core**, seguindo o paradigma **funcional**.
É a transcrição da versão em C# (orientada a objetos): mesmas rotas, mesmo JSON e o mesmo front-end (`wwwroot/`).

## Como executar

Requer o [.NET 8 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run
```

Acesse http://localhost:8080 (a porta pode ser alterada com a variável `PORT`).

### Escolha do repositório

Definido pela chave `Repositorio` no `appsettings.json`:

- `banco` (padrão): persiste em SQLite no arquivo `data/tododb.db`
- `memoria`: guarda as tarefas apenas em memória

Também pode ser definido na execução:

```bash
Repositorio=memoria dotnet run
```

## Endpoints

| Método | Rota                 | Descrição                  |
|--------|----------------------|----------------------------|
| GET    | `/todos`             | Lista as tarefas           |
| POST   | `/todos`             | Cria uma tarefa            |
| PATCH  | `/todos/{id}/toggle` | Alterna concluída/pendente |
| DELETE | `/todos/{id}`        | Remove uma tarefa          |

## Estrutura (funcional)

A ordem dos arquivos no `TodoList.fsproj` é a ordem de dependência: cada camada só conhece as anteriores.

- `Domain/Todo.fs` — records imutáveis (`Todo`, `NovaTarefa`), erros como união discriminada (`TodoErro`) e funções puras (`validar`, `alternarStatus`)
- `Repositories/TodoRepository.fs` — o repositório é um **record de funções**, não uma interface
- `Repositories/Memoria.fs` — estado imutável (`Map`) com transições puras; a única mutação é trocar a referência ao estado atual
- `Repositories/Sqlite.fs` — acesso ao banco com uma função de alta ordem (`executar`) que concentra conexão e comando
- `Services/TodoService.fs` — regras de negócio que devolvem `Result` e são compostas com `|>` e `Result.map`
- `Http/Rotas.fs` — Minimal API; `Result` vira resposta HTTP via pattern matching (400 / 404)
- `Config/RepositoryConfig.fs` — escolhe a implementação do repositório
- `Program.fs` — monta a aplicação e passa o repositório explicitamente (sem contêiner de DI)

## C# (POO) → F# (funcional)

| C#                                                     | F#                                                   |
|--------------------------------------------------------|------------------------------------------------------|
| Classe `Todo` com campos privados e setters            | Record imutável; `{ todo with Completed = ... }` gera uma cópia |
| `todo.AlternarStatus()` muda o objeto                  | `Todo.alternarStatus todo` devolve uma tarefa nova   |
| Interface `ITodoRepository` + 2 classes                | Record de funções `TodoRepository` + 2 funções `criar` |
| `List<Todo>` mutável + `lock`                          | `Map<int, Todo>` imutável + transições puras          |
| `Todo?` (pode ser null)                                | `Todo option`                                        |
| Exceções `TituloInvalidoException` / `TarefaNaoEncontradaException` | Casos `TituloInvalido` / `TarefaNaoEncontrada` em `Result` |
| `GlobalExceptionHandler` (filtro de exceções)          | Função `responder` com pattern matching              |
| Injeção de dependência no construtor                   | Repositório passado como parâmetro das funções       |
