# AnaliseCredito — Pré-análise de crédito pessoal

- **Backend:** ASP.NET Core Web API em .NET 10 (C#), num único projeto.
- **Base de dados:** Microsoft SQL Server 2022.
- **Frontend:** React e TypeScript, com Vite.
- **Testes:** xUnit.

---

## 1. Estrutura da pasta

```
AnaliseCredito/
├── AnaliseCredito.slnx                 Solução (abre no VS Code com C# Dev Kit)
├── README.md                           Este ficheiro
├── docker-compose.yml                  SQL Server em Docker
├── .vscode/                            Extensões recomendadas, tarefas e debug
│
├── backend/                            API .NET 10 (projeto AnaliseCredito.Api)
│   ├── Program.cs                      Arranque e injeção de dependências
│   ├── appsettings.json                Connection string e limites das regras
│   ├── AnaliseCredito.Api.http         Pedidos de teste prontos a enviar
│   ├── Controllers/                    PedidosController, SimulacoesController, AnaliseManualController
│   ├── Services/                       AvaliacaoService, PedidosService, AnaliseManualService, ClienteService
│   ├── Interfaces/                     IRegraCredito + uma interface por service
│   ├── DTOs/                           Um ficheiro por DTO (…DTO.cs)
│   ├── Models/                         Tabelas (Cliente, PedidoCredito…) e modelos das regras
│   ├── Enums/                          EstadoPedidoEnum, SituacaoProfissionalEnum
│   ├── Regras/                         Regra01 … Regra07, PrioridadeDecisao (Regra 8), ParametrosCredito
│   └── Data/                           AppDbContext (EF Core) e criação automática da BD
│
├── frontend/                           React + TypeScript (Vite)
│   └── src/
│       ├── App.tsx, api.ts, types.ts, cenarios.ts, formatacao.ts, styles.css
│       └── components/                 FormularioPedido, ResultadoDecisao, ListaPedidos, PainelDetalhes
│
├── database/
│   ├── 01-create-database.sql          Cria a base de dados
│   ├── 02-create-tables.sql            Tabelas, índices, estados e regras
│   ├── 03-seed-data.sql                12 pedidos de exemplo (20260001–20260004 = Cenários A–D)
│   └── 04-queries-tarefa4.sql          As 5 queries da Tarefa 4
│
└── tests/
    └── AnaliseCredito.Tests/           Testes das regras (cenários A–D, limites, NIF)
```

---

## 2. Instalar o que falta (só uma vez)

Abra a **Linha de Comandos** ou o **PowerShell** do Windows. Para cada programa, corra o comando de verificação. Se aparecer uma versão igual ou superior à indicada, esse programa já está instalado.

| Programa | Verificar | Versão necessária | Onde instalar |
|---|---|---|---|
| .NET SDK | `dotnet --list-sdks` | uma linha começada por **10.** | https://dotnet.microsoft.com/download/dotnet/10.0 → *SDK* → *Windows x64* |
| Node.js | `node -v` | **v20.19** ou superior (recomendado v22 ou v24 LTS) | https://nodejs.org → versão *LTS* |
| Docker Desktop | `docker --version` | qualquer versão recente | já instalado |
| VS Code | — | recente | https://code.visualstudio.com |

Depois de instalar o .NET ou o Node, **feche e volte a abrir** o terminal e o VS Code, para que os novos comandos sejam reconhecidos.

Espaço em disco aproximado:

| Componente | Espaço |
|---|---|
| .NET SDK | 1 GB |
| Node.js | 100 MB |
| Imagem do SQL Server | 1,5 GB |
| `node_modules` | 150 MB |
| Pacotes NuGet | 100 MB |

---

## 3. Executar, passo a passo

### Passo 1 — Abrir o projeto no VS Code
1. Extraia o `AnaliseCredito.zip`, por exemplo para `C:\Projetos\AnaliseCredito`.
2. No VS Code, abra a pasta com **File → Open Folder…** e escolha `AnaliseCredito`.
3. O VS Code sugere instalar as extensões recomendadas. Clique em **Install All**. As extensões são C# Dev Kit, SQL Server (mssql), Docker e REST Client.
4. Abra um terminal no VS Code com **Terminal → New Terminal**. Todos os comandos seguintes são escritos aí.

### Passo 2 — Arrancar o SQL Server (Docker)
1. Abra o **Docker Desktop** e espere que diga *Engine running*.
2. No terminal do VS Code, na pasta `AnaliseCredito`, escreva:
   ```
   docker compose up -d
   ```
   Da primeira vez, o Docker descarrega a imagem do SQL Server (cerca de 1,5 GB), o que demora alguns minutos. Nas vezes seguintes arranca em segundos.
3. Confirme que o contentor está a correr:
   ```
   docker ps
   ```
   Deve aparecer `analisecredito-sql` com o estado `Up`.

Os dados de acesso ao SQL Server são os seguintes. Já estão configurados no projeto.

| Campo | Valor |
|---|---|
| Servidor | `localhost,1433` |
| Utilizador | `sa` |
| Password | `AnaliseCredito#2026` |

### Passo 3 — Arrancar a API (.NET)
No terminal, escreva:
```
cd backend
dotnet run
```

Da primeira vez, o .NET descarrega os pacotes NuGet e compila o projeto. Quando aparecer `Now listening on: http://localhost:5080`, a API está pronta.

No primeiro arranque, a API **cria sozinha a base de dados** com os scripts da pasta `database/`, incluindo os dados de exemplo. No terminal aparece `Base de dados AnaliseCredito criada com sucesso`.

Para confirmar, abra http://localhost:5080/swagger. É uma página onde pode experimentar a API.

**Deixe este terminal aberto.** Fechá-lo desliga a API.

### Passo 4 — Arrancar o frontend (React)
1. Abra um **segundo terminal** clicando no **+** do painel de terminais.
2. Escreva:
   ```
   cd frontend
   npm install
   npm run dev
   ```
   O `npm install` só é preciso da primeira vez e demora cerca de 1 minuto.
3. Abra http://localhost:5173 no browser.

### Passo 5 — Usar a aplicação
- Ao abrir, o cartão de resultado mostra o **Cenário A**. Os Cenários A–D estão na lista de pedidos (20260001 a 20260004), com a etiqueta *Cenário A/B/C/D*.
- Preencha o **Novo pedido** e clique em **Simular** (grava marcado como simulação) ou **Submeter pedido** (grava como pedido real).
- O cartão de resultado mostra a **decisão final**, os **motivos** e os **indicadores**.
- Se o pedido for **inválido**, os campos com erro ficam vazios e a vermelho; os campos corretos mantêm-se.
- Na lista, a coluna **Ação / Regras** abre um painel lateral com os detalhes de cada pedido. Nos pedidos em **ANÁLISE MANUAL**, o botão **Decidir** abre esse painel com a decisão do analista (nome e observação obrigatórios, com confirmação). A mudança fica no histórico, e é isso que a query 5 da Tarefa 4 conta.
- O frontend adapta-se a computador, tablet e telemóvel.

Resultados esperados para os cenários:

| Cenário | Decisão | Motivos | Taxa de esforço | Prestação |
|---|---|---|---|---|
| A | APROVADO | — | 14,67% | 166,67 € |
| B | ANÁLISE MANUAL | contrato a prazo; taxa > 35% | 45,83% | 416,67 € |
| C | RECUSADO | incidentes de crédito | 16,94% | 208,33 € |
| D | RECUSADO | montante > 20× rendimento; taxa > 50% | 82,87% | 694,44 € |

### Passo 6 — Correr os testes
Num terminal, na pasta `AnaliseCredito`, escreva:
```
dotnet test
```
Os testes não precisam do SQL Server. Também pode usar o painel **Testing** do VS Code (ícone do frasco).

### Passo 7 — Correr as queries da Tarefa 4
1. No VS Code, abra `database/04-queries-tarefa4.sql`.
2. Clique no ícone **SQL Server** na barra lateral. A ligação **AnaliseCredito (Docker)** já está configurada. Clique nela para ligar.
3. No ficheiro, carregue em **Ctrl+Shift+E** (ou clique em ▷ *Execute Query*) e escolha essa ligação.
4. Os resultados esperados com os dados de exemplo estão no cabeçalho do ficheiro.

---

## 4. Parar e voltar a arrancar

| Ação | Comando |
|---|---|
| Parar a API ou o frontend | `Ctrl+C` no respetivo terminal |
| Parar o SQL Server (os dados ficam guardados) | `docker compose down` |
| Voltar a arrancar | `docker compose up -d`, depois `dotnet run`, depois `npm run dev` |
| Apagar a base de dados e recomeçar do zero | `docker compose down -v` e depois repetir os passos 2 e 3 |

No VS Code também existem atalhos em **Terminal → Run Task…**:
1. *SQL Server (Docker) – arrancar*
2. *API – executar*
3. *Frontend – instalar dependências*
4. *Frontend – executar*

Para debug da API, carregue em **F5**.

---

## 5. Alternativa sem Docker: SQL Server Express instalado no Windows

1. Instale o **SQL Server 2022 Express** (https://www.microsoft.com/sql-server/sql-server-downloads → *Express* → *Basic*).
2. Em `backend/appsettings.json`, troque a connection string por:
   ```json
   "AnaliseCredito": "Server=localhost\\SQLEXPRESS;Database=AnaliseCredito;Trusted_Connection=True;TrustServerCertificate=True"
   ```
3. Continue no Passo 3. A API cria a base de dados da mesma forma.

---

## 6. Problemas comuns

| Sintoma | Causa provável e solução |
|---|---|
| `dotnet: command not found` ou `'dotnet' is not recognized` | O .NET não está instalado, ou o terminal foi aberto antes da instalação. Instale o .NET 10 e reabra o VS Code. |
| `NETSDK1045: The current .NET SDK does not support targeting .NET 10.0` | Tem uma versão antiga do .NET. Instale o SDK do .NET 10. |
| A API mostra `SQL Server ainda não disponível` e depois erro | O contentor não está a correr. Abra o Docker Desktop e corra `docker compose up -d`. |
| `docker compose up` diz *port is already allocated* | Já existe um SQL Server a usar a porta 1433. Pare-o, ou siga a secção 5 para usar esse servidor. |
| O frontend mostra *Não foi possível contactar a API* | A API não está a correr. Volte ao Passo 3. |
| Alterei os scripts SQL e nada mudou | A BD só é criada quando não existe. Apague-a com `docker compose down -v` e volte a arrancar. |

---

## 7. Decisões técnicas (resumo)

**Um projeto de API com as regras isoladas.** A pasta `Regras/` não depende da base de dados nem de HTTP. Por isso, as regras são testadas diretamente, e é fácil lê-las lado a lado com o enunciado.

**Uma classe por regra.** As Regras 2 a 7 implementam a interface `IRegraCredito`. Para acrescentar uma regra nova basta criar uma classe, sem mexer na `PrioridadeDecisao`.

**Controllers → Interfaces → Services.** Cada controller depende apenas da interface do service (`IAvaliacaoService`, `IPedidosService`, `IAnaliseManualService`). As implementações são registadas no `Program.cs`, o que permite trocá-las ou simulá-las em testes.

**Endpoints.**

| Método | Endereço | Função |
|---|---|---|
| POST | `/api/pedidos` | Avaliar e gravar um pedido |
| GET | `/api/pedidos?top=50` | Últimos pedidos |
| GET | `/api/pedidos/{id}` | Detalhe de um pedido |
| POST | `/api/simulacoes` | Avaliar e gravar como simulação |
| GET | `/api/analise-manual/pendentes` | Pedidos à espera do analista |
| POST | `/api/analise-manual/{id}/decisao` | Aprovar ou recusar um pedido em Análise Manual |

**Ordem de avaliação.**
1. Aplica-se a Regra 1. Se falhar, o resultado é PEDIDO INVÁLIDO e não se avalia mais nada, porque prazo 0 dividiria por zero.
2. Calculam-se os indicadores.
3. Avaliam-se **todas** as Regras 2 a 7, para mostrar todos os motivos.
4. A decisão final é a mais restritiva (Regra 8).

**Regra 8 com um `Max()`.** O enum `EstadoPedidoEnum` está ordenado por severidade: Aprovado=1, Manual=2, Recusado=3, Inválido=4. A decisão mais restritiva é, por isso, o valor máximo. Os mesmos números são os Ids da tabela `Estados`.

**Cálculos em `decimal` com arredondamento a 2 casas.**
- Os limites são inclusivos: 35,00% mantém a decisão e 50,00% dá Análise Manual.
- A comparação usa a taxa arredondada, que é a mesma que o cliente vê.

**Idade no fim do contrato:** `Idade + PrazoMeses / 12`, calculada com decimais no backend. Exatamente 75 anos é aceite. O frontend mostra só os anos (ex.: 46 anos), exceto perto do limite, entre 74 e 76, onde mostra anos e meses (ex.: 74 anos e 11 meses, 75 anos e 1 mês).

**Campos vazios.** São enviados como vazios (null) e a Regra 1 assinala-os como obrigatórios, incluindo as prestações atuais (0 é válido, vazio não). Cada erro indica o campo, para o frontend o pôr a vermelho.

**NIF.** Além dos 9 dígitos, a API valida o dígito de controlo (módulo 11). Esta validação pode ser desligada em `appsettings.json` (`ValidarDigitoControloNif`).

**Parâmetros em `appsettings.json`.** A área de Risco pode mudar um limite sem recompilar. A versão das regras fica gravada em cada pedido.

**Histórico de estados.** Guarda cada mudança de estado, com utilizador e data. É o que permite a auditoria e a query "Manual → Aprovado".

**Scripts SQL como fonte única do esquema.** A API executa-os no primeiro arranque, e o EF Core só faz o mapeamento das tabelas.
