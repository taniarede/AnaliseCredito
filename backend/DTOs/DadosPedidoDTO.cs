namespace AnaliseCredito.Api.DTOs;

// (null = campo não preenchido).
public sealed record DadosPedidoDTO(
    string Nif,
    int? Idade,
    decimal? RendimentoMensalLiquido,
    decimal? PrestacoesAtuais,
    decimal? ValorPretendido,
    int? PrazoMeses,
    string SituacaoProfissional,
    bool IncidentesCredito);
