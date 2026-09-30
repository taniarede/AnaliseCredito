using AnaliseCredito.Api.Enums;

namespace AnaliseCredito.Api.DTOs;

// Mudança de estado do pedido
public sealed record HistoricoDTO(
    EstadoPedidoEnum? EstadoAnterior,
    EstadoPedidoEnum EstadoNovo,
    string EstadoNovoDescricao,
    DateTime DataAlteracao,
    string Utilizador,
    string? Observacao);
