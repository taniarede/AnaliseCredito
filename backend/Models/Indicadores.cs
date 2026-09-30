namespace AnaliseCredito.Api.Models;

// Em memória (não é tabela): indicadores calculados pela PrioridadeDecisao (2 casas decimais.)
public sealed record Indicadores(
    decimal PrestacaoEstimada,
    decimal TaxaEsforco,
    decimal IdadeFinalContrato,
    decimal MontanteMaximoRecomendado);
