namespace AnaliseCredito.Api.Enums;

// O valor numérico segue a ordem de severidade da Regra 8 (quanto maior, mais restritivo)
// e coincide com o Id da tabela dbo.Estados.
// Assim, "a decisão mais restritiva" é simplesmente o valor máximo.
public enum EstadoPedidoEnum : byte
{
    Aprovado = 1,
    AnaliseManual = 2,
    Recusado = 3,
    PedidoInvalido = 4
}

public static class EstadoPedidoEnumExtensions
{
    public static string Descricao(this EstadoPedidoEnum estado) => estado switch
    {
        EstadoPedidoEnum.Aprovado => "APROVADO",
        EstadoPedidoEnum.AnaliseManual => "ANÁLISE MANUAL",
        EstadoPedidoEnum.Recusado => "RECUSADO",
        EstadoPedidoEnum.PedidoInvalido => "PEDIDO INVÁLIDO",
        _ => estado.ToString()
    };
}
