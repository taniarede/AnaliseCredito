namespace AnaliseCredito.Api.DTOs;

//Indicadores calculados (com 2 casas decimais; à partida, apenas a idade será número inteiro. 74 e 75 com meses incluídos).
public sealed record IndicadoresDTO(
    decimal PrestacaoEstimada,
    decimal TaxaEsforco,
    decimal IdadeFinalContrato,
    decimal MontanteMaximoRecomendado);
