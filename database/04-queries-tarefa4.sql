/* =============================================================================
   AnaliseCredito - 04 - Queries da Tarefa 4
   -----------------------------------------------------------------------------
   Convenções usadas:
   - "Estado final" de um pedido = PedidosCredito.EstadoAtualId
     (o estado depois de uma eventual decisão do analista).
   - As simulações estão na tabela Simulacoes, por isso NÃO entram nas queries
     1, 2, 3 e 5 (que só leem PedidosCredito). Na query 4 contam, porque o
     enunciado fala em "pedido/simulação": as simulações ligam-se ao cliente pelo NIF.
   - As datas são guardadas em UTC.

   Resultados esperados com os dados de exemplo (03-seed-data.sql):
     Q1 -> 2    Q2 -> Manual 3, Recusado 5, Inválido 1
     Q3 -> R3 (Incidentes de crédito), 2 ocorrências
     Q4 -> 2 clientes (123456789: 4 pedidos e 1 simulação; 245678905: 2 pedidos)
     Q5 -> 1
     Q6 (extra) -> 1 simulação, APROVADO
   ============================================================================= */

USE AnaliseCredito;
GO

/* -----------------------------------------------------------------------------
   Q1 - Número de pedidos terminados com o estado APROVADO
   ----------------------------------------------------------------------------- */
SELECT COUNT(*) AS PedidosAprovados
FROM dbo.PedidosCredito AS p
WHERE p.EstadoAtualId = 1;          -- 1 = APROVADO
GO

/* -----------------------------------------------------------------------------
   Q2 - Número de pedidos terminados em cada um dos restantes estados
   O LEFT JOIN a partir de Estados garante que um estado sem pedidos
   aparece com 0 (em vez de desaparecer do resultado).
   ----------------------------------------------------------------------------- */
SELECT e.Descricao                  AS Estado,
       COUNT(p.Id)                  AS NumeroPedidos
FROM dbo.Estados AS e
LEFT JOIN dbo.PedidosCredito AS p
       ON p.EstadoAtualId = e.Id
WHERE e.Id <> 1                     -- todos exceto APROVADO
GROUP BY e.Id, e.Descricao
ORDER BY e.Id;
GO

/* -----------------------------------------------------------------------------
   Q3 - Nos pedidos RECUSADOS, qual o motivo mais frequente
   Conta apenas os motivos que, por si, levaram à recusa (EstadoId = 3),
   agrupados por regra (a descrição tem valores variáveis, a regra não).
   WITH TIES devolve todos os motivos em caso de empate.
   ----------------------------------------------------------------------------- */
SELECT TOP (1) WITH TIES
       r.Codigo                     AS CodigoRegra,
       r.Nome                       AS Motivo,
       COUNT(*)                     AS Ocorrencias
FROM dbo.PedidosCredito AS p
JOIN dbo.MotivosDecisao AS m ON m.PedidoId = p.Id
JOIN dbo.Regras         AS r ON r.Codigo   = m.CodigoRegra
WHERE p.EstadoAtualId = 3           -- 3 = RECUSADO
  AND m.EstadoId      = 3           -- motivo que dita recusa
GROUP BY r.Codigo, r.Nome
ORDER BY COUNT(*) DESC;
GO

/* -----------------------------------------------------------------------------
   Q4 - Clientes com mais do que um pedido/simulação no último mês
   "Último mês" = de hoje há um mês até agora (janela móvel, ex.: 15/09 a 15/10).
   Alternativa comentada em baixo para o mês civil anterior.
   ----------------------------------------------------------------------------- */
DECLARE @Desde DATETIME2(0) = DATEADD(MONTH, -1, SYSUTCDATETIME());

WITH Atividade AS
(
    -- Pedidos: só os que têm cliente (NIF válido)
    SELECT p.ClienteId, p.DataPedido AS Data, 0 AS EhSimulacao
    FROM dbo.PedidosCredito AS p
    WHERE p.ClienteId IS NOT NULL
      AND p.DataPedido >= @Desde

    UNION ALL

    -- Simulações: não têm ClienteId; ligam-se ao cliente pelo NIF.
    -- Quem só simulou (sem nenhum pedido) ainda não é cliente e fica de fora.
    SELECT c.Id, s.DataSimulacao, 1
    FROM dbo.Simulacoes AS s
    JOIN dbo.Clientes   AS c ON c.Nif = s.NifSubmetido
    WHERE s.DataSimulacao >= @Desde
)
SELECT c.Nif,
       COUNT(*)               AS Total,
       SUM(1 - a.EhSimulacao) AS Pedidos,
       SUM(a.EhSimulacao)     AS Simulacoes,
       MIN(a.Data)            AS Primeiro,
       MAX(a.Data)            AS Ultimo
FROM Atividade AS a
JOIN dbo.Clientes AS c ON c.Id = a.ClienteId
GROUP BY c.Id, c.Nif
HAVING COUNT(*) > 1
ORDER BY Total DESC, c.Nif;
GO

/*  Alternativa - mês civil anterior (ex.: se hoje é 15/10, considera 01/09 a 30/09):
    troca-se a variável e acrescenta-se um limite superior em cada parte.

    DECLARE @Desde DATETIME2(0) = DATEADD(MONTH, DATEDIFF(MONTH, 0, SYSUTCDATETIME()) - 1, 0);
    DECLARE @Ate   DATETIME2(0) = DATEADD(MONTH, DATEDIFF(MONTH, 0, SYSUTCDATETIME()), 0);
    -- em cada parte:  AND Data >= @Desde AND Data < @Ate
*/

/* -----------------------------------------------------------------------------
   Q5 - Pedidos que terminaram em ANÁLISE MANUAL e evoluíram para APROVADO
   Usa o histórico de estados: existe uma transição 2 -> 1.
   ----------------------------------------------------------------------------- */
SELECT COUNT(DISTINCT h.PedidoId)   AS ManualParaAprovado
FROM dbo.HistoricoEstados AS h
WHERE h.EstadoAnteriorId = 2        -- ANÁLISE MANUAL
  AND h.EstadoNovoId     = 1;       -- APROVADO

/*  Variante sem histórico (só olha para o estado inicial e o estado atual):

    SELECT COUNT(*) FROM dbo.PedidosCredito
    WHERE EstadoAutomaticoId = 2 AND EstadoAtualId = 1;
*/
GO

/* -----------------------------------------------------------------------------
   Q6 (extra) - Contagem de simulações, no total e por decisão
   É para isto que as simulações são gravadas: contar, sem estado nem histórico.
   ----------------------------------------------------------------------------- */
SELECT COUNT(*) AS TotalSimulacoes
FROM dbo.Simulacoes;

SELECT e.Descricao                  AS Decisao,
       COUNT(s.Id)                  AS NumeroSimulacoes
FROM dbo.Estados AS e
LEFT JOIN dbo.Simulacoes AS s ON s.DecisaoId = e.Id
GROUP BY e.Id, e.Descricao
ORDER BY e.Id;
GO
