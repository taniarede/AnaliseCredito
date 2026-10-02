using AnaliseCredito.Api.Enums;
using AnaliseCredito.Api.Models;

namespace AnaliseCredito.Api.DTOs;

// Resposta devolvida ao frontend: decisão final, motivos e indicadores (como pede o enunciado),
// mais os dados submetidos e o histórico de estados.
public sealed record PedidoResponseDTO(
    int Id,
    string NumeroPedido,
    DateTime DataPedido,
    EstadoPedidoEnum DecisaoAutomatica,
    EstadoPedidoEnum EstadoAtual,
    string EstadoAtualDescricao,
    IReadOnlyList<MotivoDTO> Motivos,
    IndicadoresDTO? Indicadores,
    DadosPedidoDTO Dados,
    IReadOnlyList<HistoricoDTO> Historico)
{
    // Converte o pedido guardado na base de dados na resposta da API.
    // <param name="montanteMaximo">Montante máximo recomendado (20× rendimento), calculado pelas regras.</param>
    public static PedidoResponseDTO DeModelo(PedidoCredito p, decimal? montanteMaximo)
    {
        IndicadoresDTO? indicadores =
            p is { PrestacaoEstimada: not null, TaxaEsforco: not null, IdadeFinalContrato: not null } && montanteMaximo is not null
                ? new IndicadoresDTO(p.PrestacaoEstimada.Value, p.TaxaEsforco.Value, p.IdadeFinalContrato.Value, montanteMaximo.Value)
                : null;

        return new PedidoResponseDTO(
            p.Id,
            p.NumeroPedido,
            DateTime.SpecifyKind(p.DataPedido, DateTimeKind.Utc),
            p.EstadoAutomatico,
            p.EstadoAtual,
            p.EstadoAtual.Descricao(),
            p.Motivos
                .OrderBy(m => m.Id)
                .Select(m => new MotivoDTO(m.CodigoRegra, m.Estado, m.Estado.Descricao(), m.Descricao, m.Campo))
                .ToList(),
            indicadores,
            new DadosPedidoDTO(p.NifSubmetido, p.Idade, p.RendimentoMensalLiquido, p.PrestacoesAtuais,
                p.ValorPretendido, p.PrazoMeses, p.SituacaoProfissional, p.IncidentesCredito),
            p.Historico
                .OrderBy(h => h.DataAlteracao).ThenBy(h => h.Id)
                .Select(h => new HistoricoDTO(h.EstadoAnterior, h.EstadoNovo, h.EstadoNovo.Descricao(),
                    DateTime.SpecifyKind(h.DataAlteracao, DateTimeKind.Utc), h.Utilizador, h.Observacao))
                .ToList());
    }
}
