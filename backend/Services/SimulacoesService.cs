using AnaliseCredito.Api.Data;
using AnaliseCredito.Api.DTOs;
using AnaliseCredito.Api.Interfaces;
using AnaliseCredito.Api.Models;
using AnaliseCredito.Api.Regras;

namespace AnaliseCredito.Api.Services;

// Simulação: usa exatamente as mesmas regras de um pedido (PrioridadeDecisao),
// mas grava uma única linha em dbo.Simulacoes - sem cliente, sem motivos em tabela própria,
// sem estado atual e sem histórico. Serve apenas para contagem.
public sealed class SimulacoesService(AppDbContext db, PrioridadeDecisao regras, TimeProvider relogio)
    : ISimulacoesService
{
    public async Task<SimulacaoResponseDTO> SimularAsync(PedidoRequestDTO request, CancellationToken ct)
    {
        var input = request.ParaModelo();
        var resultado = regras.Avaliar(input);

        // Códigos distintos pela ordem em que aparecem (a Regra 1 pode dar vários erros "R1")
        var codigos = resultado.Motivos.Select(m => m.CodigoRegra).Distinct().ToList();

        var simulacao = new Simulacao
        {
            NifSubmetido = input.Nif,
            Idade = input.Idade,
            RendimentoMensalLiquido = input.RendimentoMensalLiquido,
            PrestacoesAtuais = input.PrestacoesAtuais,
            ValorPretendido = input.ValorPretendido,
            PrazoMeses = input.PrazoMeses,
            SituacaoProfissional = input.SituacaoProfissional,
            IncidentesCredito = input.IncidentesCredito,
            PrestacaoEstimada = resultado.Indicadores?.PrestacaoEstimada,
            TaxaEsforco = resultado.Indicadores?.TaxaEsforco,
            IdadeFinalContrato = resultado.Indicadores?.IdadeFinalContrato,
            Decisao = resultado.Decisao,
            CodigosRegras = codigos.Count == 0 ? null : string.Join(',', codigos),
            VersaoRegras = regras.Parametros.VersaoRegras,
            DataSimulacao = relogio.GetUtcNow().UtcDateTime
        };

        db.Simulacoes.Add(simulacao);
        await db.SaveChangesAsync(ct);

        return SimulacaoResponseDTO.DeModelo(simulacao, resultado);
    }
}
