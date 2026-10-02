using AnaliseCredito.Api.Enums;
using AnaliseCredito.Api.Models;

namespace AnaliseCredito.Api.DTOs;

// Resposta de POST /api/simulacoes: decisão, motivos e indicadores, como num pedido,
// mas sem número de pedido, estado atual nem histórico.
public sealed record SimulacaoResponseDTO(
    int Id,
    DateTime DataSimulacao,
    EstadoPedidoEnum Decisao,
    string DecisaoDescricao,
    IReadOnlyList<MotivoDTO> Motivos,
    IndicadoresDTO? Indicadores,
    DadosPedidoDTO Dados)
{
    // Os motivos vêm do resultado em memória (texto completo); na tabela só ficam os códigos das regras.
    public static SimulacaoResponseDTO DeModelo(Simulacao s, ResultadoAvaliacao resultado)
    {
        var i = resultado.Indicadores;

        return new SimulacaoResponseDTO(
            s.Id,
            DateTime.SpecifyKind(s.DataSimulacao, DateTimeKind.Utc),
            s.Decisao,
            s.Decisao.Descricao(),
            resultado.Motivos
                .Select(m => new MotivoDTO(m.CodigoRegra, m.Decisao, m.Decisao.Descricao(), m.Descricao, m.Campo))
                .ToList(),
            i is null ? null : new IndicadoresDTO(i.PrestacaoEstimada, i.TaxaEsforco, i.IdadeFinalContrato, i.MontanteMaximoRecomendado),
            new DadosPedidoDTO(s.NifSubmetido, s.Idade, s.RendimentoMensalLiquido, s.PrestacoesAtuais,
                s.ValorPretendido, s.PrazoMeses, s.SituacaoProfissional, s.IncidentesCredito));
    }
}
