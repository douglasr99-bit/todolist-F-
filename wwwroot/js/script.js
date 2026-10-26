const DIAS_SEMANA = ["domingo", "segunda", "terca", "quarta", "quinta", "sexta", "sabado"];

function obterDiaAtual() {
    var indice = new Date().getDay();
    return DIAS_SEMANA[indice];
}

function aplicarCorDoDiaAtual() {
    var diaAtual = obterDiaAtual();
    var corDoDia = localStorage.getItem("cor-" + diaAtual);
    var main = document.querySelector("main");
    var tab = document.querySelector(".tab");

    if (corDoDia) {
        main.style.backgroundColor = corDoDia;
        tab.style.backgroundColor = corDoDia;
    }
}

async function carregarTarefas() {
    const resposta = await fetch("/todos");
    const dados = await resposta.json();
    renderizarTarefas(dados);
}

function renderizarTarefas(todos) {
    document.querySelectorAll(".task-list").forEach(function (lista) {
        lista.innerHTML = "";
    });

    todos.forEach(function (todo) {
        var card = document.querySelector('.card[data-day="' + todo.weekday + '"]');

        if (!card) {
            console.warn("Nenhum card encontrado para o dia: " + todo.weekday);
            return;
        }

        var lista = card.querySelector(".task-list");

        var item = document.createElement("li");
        item.classList.add(todo.priority);

        var checkbox = document.createElement("input");
        checkbox.type = "checkbox";
        checkbox.id = "task-" + todo.id;
        checkbox.checked = todo.completed;

        checkbox.addEventListener("change", async function () {
            await fetch("/todos/" + todo.id + "/toggle", {
                method: "PATCH"
            });
        });

        var label = document.createElement("label");
        label.setAttribute("for", "task-" + todo.id);
        label.textContent = todo.title;

        var btnRemover = document.createElement("button");
        btnRemover.textContent = "x";
        btnRemover.classList.add("btn-remover");
        btnRemover.addEventListener("click", async function () {
            await fetch("/todos/" + todo.id, { method: "DELETE" });
            carregarTarefas();
        });

        item.appendChild(checkbox);
        item.appendChild(label);
        item.appendChild(btnRemover);
        lista.appendChild(item);
    });
}

function selecaodata() {
    document.querySelectorAll(".date").forEach(function (elementoDate) {
        elementoDate.addEventListener("click", function (event) {
            document.querySelectorAll(".date").forEach(function (outro) {
                outro.classList.remove("active");
                document.querySelector(".form-container").classList.add("hidden");
                document.querySelectorAll(".form-name").forEach(function (formName) {
                    formName.remove();
                });
            });
            event.currentTarget.classList.add("active");
            document.querySelector(".form-container.hidden").classList.remove("hidden");
            var div = document.createElement("div");
            div.classList.add("form-name");
            div.textContent = event.currentTarget.textContent;
            document.querySelector(".form-container").insertBefore(div, document.querySelector(".form-container").firstChild);
        });
    });
}

async function adicionarTarefa() {
    var week = document.querySelector(".date.active").parentElement.dataset.day;
    var inputclear = document.querySelector("#new-task-title");
    var input = document.querySelector("#new-task-title").value;

    if (input === "") {
        alert("O título da tarefa não pode ser vazio.");
        return;
    }

    var novaTarefa = {
        title: input,
        completed: false,
        weekday: week,
        priority: document.querySelector("#new-task-priority").value
    };

    await fetch("/todos", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(novaTarefa)
    });

    await carregarTarefas();

    inputclear.value = "";
}

function salvarCorCard(weekday, cor) {
    localStorage.setItem("cor-" + weekday, cor);
}

function aplicarCoresSalvas() {
    document.querySelectorAll(".card").forEach(function (card) {
        var dia = card.dataset.day;
        var corSalva = localStorage.getItem("cor-" + dia);
        if (corSalva) {
            card.style.backgroundColor = corSalva;
        }
    });
}

function colorintime() {
    var colorinput = document.querySelector("#new-task-color");

    colorinput.addEventListener("input", function (event) {
        var cardAtivo = document.querySelector(".date.active").parentElement;
        var dia = cardAtivo.dataset.day;
        cardAtivo.style.backgroundColor = event.target.value;
        salvarCorCard(dia, event.target.value);

        if (dia === obterDiaAtual()) {
            aplicarCorDoDiaAtual();
        }
    });
}

document.querySelector("#add-task-button").addEventListener("click", adicionarTarefa);

selecaodata();
colorintime();
aplicarCoresSalvas();
aplicarCorDoDiaAtual();
carregarTarefas();