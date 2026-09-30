using AnaliseCredito.Api.Enums;

namespace AnaliseCredito.Api.DTOs;

// Um motivo da decisão. "Campo" só vem preenchido nos erros da Regra 1.
public sealed record MotivoDTO(
    string CodigoRegra,
    EstadoPedidoEnum Decisao,
    string DecisaoDescricao,
    string Descricao,
    string? Campo);
