/* =============================================================================
   AnaliseCredito - 03 - Dados de exemplo (OPCIONAL)
   -----------------------------------------------------------------------------
   Gera 12 pedidos com datas relativas a "hoje" (do mais antigo, 20260001,
   ao mais recente, 20260012), para que as queries da Tarefa 4 devolvam resultados.
   Os pedidos 1 a 4 (20260001 a 20260004) são exatamente os Cenários A, B, C e D
   do enunciado, com a decisão automática (o B fica em ANÁLISE MANUAL, por decidir).
   Os indicadores e motivos foram calculados com as mesmas regras da aplicação.

   ATENÇÃO: apaga os pedidos/clientes existentes antes de inserir.
   ============================================================================= */

USE AnaliseCredito;
GO

SET NOCOUNT ON;

DELETE FROM dbo.HistoricoEstados;
DELETE FROM dbo.MotivosDecisao;
DELETE FROM dbo.PedidosCredito;
DELETE FROM dbo.Clientes;

DECLARE @agora DATETIME2(0) = SYSUTCDATETIME();

SET IDENTITY_INSERT dbo.Clientes ON;
INSERT INTO dbo.Clientes (Id, Nif, DataCriacao) VALUES
    (1, '123456789', DATEADD(DAY, -29, @agora)),
    (2, '198765436', DATEADD(DAY, -18, @agora)),
    (3, '245678905', DATEADD(DAY, -15, @agora)),
    (4, '266778895', DATEADD(DAY, -7, @agora)),
    (5, '123123127', DATEADD(DAY, -5, @agora)),
    (6, '135792460', DATEADD(DAY, -2, @agora));
SET IDENTITY_INSERT dbo.Clientes OFF;

SET IDENTITY_INSERT dbo.PedidosCredito ON;
INSERT INTO dbo.PedidosCredito
    (Id, ClienteId, NifSubmetido, Idade, RendimentoMensalLiquido, PrestacoesAtuais, ValorPretendido, PrazoMeses,
     SituacaoProfissional, IncidentesCredito, EhSimulacao, PrestacaoEstimada, TaxaEsforco, IdadeFinalContrato,
     EstadoAutomaticoId, EstadoAtualId, VersaoRegras, DataPedido, DataAtualizacao)
VALUES
    (1, 1, N'123456789', 35, 2500, 200, 10000, 60, N'Efetivo', 0, 0, 166.67, 14.67, 40.00, 1, 1, '1.0', DATEADD(DAY, -29, @agora), NULL),
    (2, 1, N'123456789', 42, 2000, 500, 20000, 48, N'ContratoPrazo', 0, 0, 416.67, 45.83, 46.00, 2, 2, '1.0', DATEADD(DAY, -28, @agora), NULL),
    (3, 1, N'123456789', 40, 3000, 300, 15000, 72, N'Efetivo', 1, 0, 208.33, 16.94, 46.00, 3, 3, '1.0', DATEADD(DAY, -27, @agora), NULL),
    (4, 1, N'123456789', 30, 1200, 300, 25000, 36, N'Efetivo', 0, 0, 694.44, 82.87, 33.00, 3, 3, '1.0', DATEADD(DAY, -26, @agora), NULL),
    (5, 1, N'123456789', 35, 2500, 200, 12000, 48, N'Efetivo', 0, 1, 250.00, 18.00, 39.00, 1, 1, '1.0', DATEADD(DAY, -25, @agora), NULL),
    (6, NULL, N'12345', 25, 1500, 0, 5000, 24, N'Efetivo', 0, 0, NULL, NULL, NULL, 4, 4, '1.0', DATEADD(DAY, -22, @agora), NULL),
    (7, 2, N'198765436', 50, 900, 100, 8000, 48, N'Desempregado', 0, 0, 166.67, 29.63, 54.00, 3, 3, '1.0', DATEADD(DAY, -18, @agora), NULL),
    (8, 3, N'245678905', 42, 2000, 500, 12000, 36, N'ContratoPrazo', 0, 0, 333.33, 41.67, 45.00, 2, 3, '1.0', DATEADD(DAY, -15, @agora), DATEADD(DAY, -14, @agora)),
    (9, 3, N'245678905', 45, 6000, 800, 60000, 120, N'Efetivo', 0, 0, 500.00, 21.67, 55.00, 2, 2, '1.0', DATEADD(DAY, -10, @agora), NULL),
    (10, 4, N'266778895', 38, 1800, 250, 9000, 60, N'Efetivo', 1, 0, 150.00, 22.22, 43.00, 3, 3, '1.0', DATEADD(DAY, -7, @agora), NULL),
    (11, 5, N'123123127', 70, 3000, 0, 20000, 72, N'Efetivo', 0, 0, 277.78, 9.26, 76.00, 2, 1, '1.0', DATEADD(DAY, -5, @agora), DATEADD(DAY, -4, @agora)),
    (12, 6, N'135792460', 33, 1500, 400, 12000, 36, N'Efetivo', 0, 0, 333.33, 48.89, 36.00, 2, 2, '1.0', DATEADD(DAY, -2, @agora), NULL);
SET IDENTITY_INSERT dbo.PedidosCredito OFF;

INSERT INTO dbo.MotivosDecisao (PedidoId, CodigoRegra, EstadoId, Descricao, Campo) VALUES
    (2, 'R4', 2, N'Cliente com contrato a prazo.', NULL),
    (2, 'R6', 2, N'Taxa de esforço de 45,83% superior a 35% e até 50%.', NULL),
    (3, 'R3', 3, N'Cliente com incidentes de crédito registados.', NULL),
    (4, 'R5', 2, N'Montante superior ao limite recomendado de 24.000,00 € (20 × rendimento).', NULL),
    (4, 'R6', 3, N'Taxa de esforço de 82,87% superior a 50%.', NULL),
    (6, 'R1', 4, N'NIF deve ter exatamente 9 dígitos.', 'nif'),
    (7, 'R4', 3, N'Cliente desempregado.', NULL),
    (8, 'R4', 2, N'Cliente com contrato a prazo.', NULL),
    (8, 'R6', 2, N'Taxa de esforço de 41,67% superior a 35% e até 50%.', NULL),
    (8, 'ANALISTA', 3, N'Rendimento não comprovado.', NULL),
    (9, 'R7', 2, N'Montante superior a 50.000,00 €.', NULL),
    (10, 'R3', 3, N'Cliente com incidentes de crédito registados.', NULL),
    (11, 'R2', 2, N'Idade no final do contrato (76,00 anos) superior a 75 anos.', NULL),
    (11, 'ANALISTA', 1, N'Fiador apresentado.', NULL),
    (12, 'R6', 2, N'Taxa de esforço de 48,89% superior a 35% e até 50%.', NULL);

INSERT INTO dbo.HistoricoEstados (PedidoId, EstadoAnteriorId, EstadoNovoId, DataAlteracao, Utilizador, Observacao) VALUES
    (1, NULL, 1, DATEADD(DAY, -29, @agora), N'SISTEMA', N'Decisão automática do motor de regras'),
    (2, NULL, 2, DATEADD(DAY, -28, @agora), N'SISTEMA', N'Decisão automática do motor de regras'),
    (3, NULL, 3, DATEADD(DAY, -27, @agora), N'SISTEMA', N'Decisão automática do motor de regras'),
    (4, NULL, 3, DATEADD(DAY, -26, @agora), N'SISTEMA', N'Decisão automática do motor de regras'),
    (5, NULL, 1, DATEADD(DAY, -25, @agora), N'SISTEMA', N'Decisão automática do motor de regras'),
    (6, NULL, 4, DATEADD(DAY, -22, @agora), N'SISTEMA', N'Decisão automática do motor de regras'),
    (7, NULL, 3, DATEADD(DAY, -18, @agora), N'SISTEMA', N'Decisão automática do motor de regras'),
    (8, NULL, 2, DATEADD(DAY, -15, @agora), N'SISTEMA', N'Decisão automática do motor de regras'),
    (8, 2, 3, DATEADD(DAY, -14, @agora), N'analista.costa', N'Rendimento não comprovado.'),
    (9, NULL, 2, DATEADD(DAY, -10, @agora), N'SISTEMA', N'Decisão automática do motor de regras'),
    (10, NULL, 3, DATEADD(DAY, -7, @agora), N'SISTEMA', N'Decisão automática do motor de regras'),
    (11, NULL, 2, DATEADD(DAY, -5, @agora), N'SISTEMA', N'Decisão automática do motor de regras'),
    (11, 2, 1, DATEADD(DAY, -4, @agora), N'analista.silva', N'Fiador apresentado.'),
    (12, NULL, 2, DATEADD(DAY, -2, @agora), N'SISTEMA', N'Decisão automática do motor de regras');
GO
