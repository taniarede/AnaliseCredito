using AnaliseCredito.Api.Enums;

namespace AnaliseCredito.Api.Models;

// Em memória (não é tabela): resultado final da avaliação -
// decisão, motivos e indicadores (null quando o pedido é inválido).
public sealed record ResultadoAvaliacao(
    EstadoPedidoEnum Decisao,
    IReadOnlyList<MotivoAvaliacao> Motivos,
    Indicadores? Indicadores);
