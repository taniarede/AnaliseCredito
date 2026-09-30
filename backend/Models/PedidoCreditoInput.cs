namespace AnaliseCredito.Api.Models;

// Em memória (não é tabela): os dados de entrada de um pedido, tal como descritos no enunciado.
// Os campos numéricos podem vir vazios (null); a Regra 1 assinala-os como obrigatórios.
public sealed record PedidoCreditoInput(
    string Nif,
    int? Idade,
    decimal? RendimentoMensalLiquido,
    decimal? PrestacoesAtuais,
    decimal? ValorPretendido,
    int? PrazoMeses,
    string SituacaoProfissional,
    bool IncidentesCredito);
