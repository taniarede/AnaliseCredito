/* =============================================================================
   AnaliseCredito - 02 - Tabelas, índices e dados de referência
   -----------------------------------------------------------------------------
   Pode ser executado várias vezes (cada objeto só é criado se não existir).

   Tabelas:
     Estados           -> lista fixa de estados/decisões (APROVADO, ANÁLISE MANUAL...)
     Regras            -> catálogo das regras de negócio (R1..R7 + decisão do analista)
     Clientes          -> um registo por NIF
     PedidosCredito    -> dados submetidos + indicadores calculados + estado
     MotivosDecisao    -> um registo por motivo (permite estatísticas por regra)
     HistoricoEstados  -> todas as mudanças de estado (auditoria e query "Manual -> Aprovado")
     Simulacoes        -> simulações (só para contagem: sem estado atual nem histórico)
   ============================================================================= */

USE AnaliseCredito;
GO

/* ---------------------------------------------------------------------------
   Estados - o Id segue a ordem de severidade da Regra 8
   (quanto maior o Id, mais restritiva é a decisão).
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Estados', N'U') IS NULL
CREATE TABLE dbo.Estados
(
    Id        TINYINT       NOT NULL CONSTRAINT PK_Estados PRIMARY KEY,
    Codigo    VARCHAR(20)   NOT NULL CONSTRAINT UQ_Estados_Codigo UNIQUE,
    Descricao NVARCHAR(50)  NOT NULL
);
GO

/* ---------------------------------------------------------------------------
   Regras - catálogo das regras (usado nos relatórios)
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Regras', N'U') IS NULL
CREATE TABLE dbo.Regras
(
    Codigo    VARCHAR(10)   NOT NULL CONSTRAINT PK_Regras PRIMARY KEY,
    Nome      NVARCHAR(100) NOT NULL,
    Descricao NVARCHAR(300) NOT NULL
);
GO

/* ---------------------------------------------------------------------------
   Clientes
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Clientes', N'U') IS NULL
CREATE TABLE dbo.Clientes
(
    Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Clientes PRIMARY KEY,
    Nif         CHAR(9)           NOT NULL CONSTRAINT UQ_Clientes_Nif UNIQUE,
    DataCriacao DATETIME2(0)      NOT NULL CONSTRAINT DF_Clientes_DataCriacao DEFAULT SYSUTCDATETIME()
);
GO

/* ---------------------------------------------------------------------------
   PedidosCredito
   - Os dados de entrada ficam guardados tal como foram submetidos
     (NULL = campo deixado vazio no formulário -> pedido inválido).
   - Os indicadores ficam NULL quando o pedido é inválido (Regra 1).
   - EstadoAutomaticoId = decisão do motor de regras (nunca muda).
   - EstadoAtualId      = estado atual (pode mudar pela decisão do analista).
   - ClienteId é NULL quando o NIF não tem 9 dígitos (pedido inválido).
   - NumeroPedido é calculado: ano + número sequencial (ex.: 20260001).
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.PedidosCredito', N'U') IS NULL
CREATE TABLE dbo.PedidosCredito
(
    Id                      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PedidosCredito PRIMARY KEY,
    NumeroPedido            AS (CAST(YEAR(DataPedido) AS VARCHAR(4))
                                + CASE WHEN Id < 10000 THEN RIGHT('0000' + CAST(Id AS VARCHAR(10)), 4)
                                       ELSE CAST(Id AS VARCHAR(10)) END),
    ClienteId               INT            NULL CONSTRAINT FK_Pedidos_Clientes REFERENCES dbo.Clientes(Id),
    NifSubmetido            NVARCHAR(20)   NOT NULL,
    Idade                   INT            NULL,
    RendimentoMensalLiquido DECIMAL(18,2)  NULL,
    PrestacoesAtuais        DECIMAL(18,2)  NULL,
    ValorPretendido         DECIMAL(18,2)  NULL,
    PrazoMeses              INT            NULL,
    SituacaoProfissional    NVARCHAR(30)   NOT NULL,
    IncidentesCredito       BIT            NOT NULL,
    PrestacaoEstimada       DECIMAL(18,2)  NULL,
    TaxaEsforco             DECIMAL(18,2)  NULL,
    IdadeFinalContrato      DECIMAL(18,2)  NULL,
    EstadoAutomaticoId      TINYINT        NOT NULL CONSTRAINT FK_Pedidos_EstadoAutomatico REFERENCES dbo.Estados(Id),
    EstadoAtualId           TINYINT        NOT NULL CONSTRAINT FK_Pedidos_EstadoAtual REFERENCES dbo.Estados(Id),
    VersaoRegras            VARCHAR(10)    NOT NULL,
    DataPedido              DATETIME2(0)   NOT NULL CONSTRAINT DF_Pedidos_DataPedido DEFAULT SYSUTCDATETIME(),
    DataAtualizacao         DATETIME2(0)   NULL
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Pedidos_ClienteId_DataPedido')
    CREATE INDEX IX_Pedidos_ClienteId_DataPedido ON dbo.PedidosCredito (ClienteId, DataPedido);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Pedidos_EstadoAtualId')
    CREATE INDEX IX_Pedidos_EstadoAtualId ON dbo.PedidosCredito (EstadoAtualId);
GO

/* ---------------------------------------------------------------------------
   MotivosDecisao - um registo por motivo
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.MotivosDecisao', N'U') IS NULL
CREATE TABLE dbo.MotivosDecisao
(
    Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MotivosDecisao PRIMARY KEY,
    PedidoId    INT            NOT NULL CONSTRAINT FK_Motivos_Pedidos REFERENCES dbo.PedidosCredito(Id) ON DELETE CASCADE,
    CodigoRegra VARCHAR(10)    NOT NULL CONSTRAINT FK_Motivos_Regras REFERENCES dbo.Regras(Codigo),
    EstadoId    TINYINT        NOT NULL CONSTRAINT FK_Motivos_Estados REFERENCES dbo.Estados(Id),
    Descricao   NVARCHAR(300)  NOT NULL,
    Campo       VARCHAR(30)    NULL        -- só nos erros da Regra 1 (ex.: 'nif', 'idade')
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Motivos_PedidoId')
    CREATE INDEX IX_Motivos_PedidoId ON dbo.MotivosDecisao (PedidoId);
GO

/* ---------------------------------------------------------------------------
   HistoricoEstados - cada mudança de estado de um pedido
   (EstadoAnteriorId é NULL no primeiro registo, feito pelo sistema)
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.HistoricoEstados', N'U') IS NULL
CREATE TABLE dbo.HistoricoEstados
(
    Id               INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HistoricoEstados PRIMARY KEY,
    PedidoId         INT            NOT NULL CONSTRAINT FK_Historico_Pedidos REFERENCES dbo.PedidosCredito(Id) ON DELETE CASCADE,
    EstadoAnteriorId TINYINT        NULL     CONSTRAINT FK_Historico_EstadoAnterior REFERENCES dbo.Estados(Id),
    EstadoNovoId     TINYINT        NOT NULL CONSTRAINT FK_Historico_EstadoNovo REFERENCES dbo.Estados(Id),
    DataAlteracao    DATETIME2(0)   NOT NULL CONSTRAINT DF_Historico_Data DEFAULT SYSUTCDATETIME(),
    Utilizador       NVARCHAR(100)  NOT NULL,
    Observacao       NVARCHAR(500)  NULL
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Historico_PedidoId')
    CREATE INDEX IX_Historico_PedidoId ON dbo.HistoricoEstados (PedidoId);
GO

/* ---------------------------------------------------------------------------
   Simulacoes - uma linha por simulação, só para contagem
   - Mesmos dados de entrada e indicadores de um pedido, e a decisão do motor.
   - Sem estado atual, sem histórico e sem motivos em tabela própria:
     uma simulação nunca muda e não pode ser decidida por um analista.
   - Sem ClienteId: uma simulação não cria cliente; o NIF fica como foi escrito.
   - CodigosRegras = regras que dispararam, separadas por vírgula (ex.: 'R4,R6').
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Simulacoes', N'U') IS NULL
CREATE TABLE dbo.Simulacoes
(
    Id                      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Simulacoes PRIMARY KEY,
    NifSubmetido            NVARCHAR(20)   NOT NULL,
    Idade                   INT            NULL,
    RendimentoMensalLiquido DECIMAL(18,2)  NULL,
    PrestacoesAtuais        DECIMAL(18,2)  NULL,
    ValorPretendido         DECIMAL(18,2)  NULL,
    PrazoMeses              INT            NULL,
    SituacaoProfissional    NVARCHAR(30)   NOT NULL,
    IncidentesCredito       BIT            NOT NULL,
    PrestacaoEstimada       DECIMAL(18,2)  NULL,
    TaxaEsforco             DECIMAL(18,2)  NULL,
    IdadeFinalContrato      DECIMAL(18,2)  NULL,
    DecisaoId               TINYINT        NOT NULL CONSTRAINT FK_Simulacoes_Estados REFERENCES dbo.Estados(Id),
    CodigosRegras           VARCHAR(100)   NULL,
    VersaoRegras            VARCHAR(10)    NOT NULL,
    DataSimulacao           DATETIME2(0)   NOT NULL CONSTRAINT DF_Simulacoes_Data DEFAULT SYSUTCDATETIME()
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Simulacoes_DataSimulacao')
    CREATE INDEX IX_Simulacoes_DataSimulacao ON dbo.Simulacoes (DataSimulacao) INCLUDE (NifSubmetido, DecisaoId);
GO

/* ---------------------------------------------------------------------------
   Dados de referência
   --------------------------------------------------------------------------- */
MERGE dbo.Estados AS alvo
USING (VALUES
    (1, 'APROVADO',        N'APROVADO'),
    (2, 'ANALISE_MANUAL',  N'ANÁLISE MANUAL'),
    (3, 'RECUSADO',        N'RECUSADO'),
    (4, 'PEDIDO_INVALIDO', N'PEDIDO INVÁLIDO')
) AS origem (Id, Codigo, Descricao)
ON alvo.Id = origem.Id
WHEN MATCHED THEN UPDATE SET Codigo = origem.Codigo, Descricao = origem.Descricao
WHEN NOT MATCHED THEN INSERT (Id, Codigo, Descricao) VALUES (origem.Id, origem.Codigo, origem.Descricao);
GO

MERGE dbo.Regras AS alvo
USING (VALUES
    ('R1',       N'Validação inicial',            N'NIF com 9 dígitos, idade >= 18, rendimento, valor e prazo superiores a 0.'),
    ('R2',       N'Idade no final do contrato',   N'Idade + PrazoMeses / 12 superior a 75 anos -> Análise Manual.'),
    ('R3',       N'Incidentes de crédito',        N'Cliente com incidentes registados -> Recusado.'),
    ('R4',       N'Situação profissional',        N'ContratoPrazo -> Análise Manual; Desempregado -> Recusado.'),
    ('R5',       N'Limite do montante',           N'Valor pretendido superior a 20x o rendimento -> Análise Manual.'),
    ('R6',       N'Taxa de esforço',              N'> 35% e <= 50% -> Análise Manual; > 50% -> Recusado.'),
    ('R7',       N'Montantes elevados',           N'Valor pretendido superior a 50.000,00 EUR -> Análise Manual.'),
    ('ANALISTA', N'Decisão do analista',          N'Decisão tomada por um analista sobre um pedido em Análise Manual.')
) AS origem (Codigo, Nome, Descricao)
ON alvo.Codigo = origem.Codigo
WHEN MATCHED THEN UPDATE SET Nome = origem.Nome, Descricao = origem.Descricao
WHEN NOT MATCHED THEN INSERT (Codigo, Nome, Descricao) VALUES (origem.Codigo, origem.Nome, origem.Descricao);
GO
