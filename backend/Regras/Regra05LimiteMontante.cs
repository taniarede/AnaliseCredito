using AnaliseCredito.Api.Enums;
using AnaliseCredito.Api.Interfaces;
using AnaliseCredito.Api.Models;

namespace AnaliseCredito.Api.Regras;

//Regra 5 - ValorPretendido > 20 × rendimento -> ANÁLISE MANUAL (exatamente 20× é aceite).
public sealed class Regra05LimiteMontante(ParametrosCredito parametros) : IRegraCredito
{
    public string Codigo => "R5";

    public MotivoAvaliacao? Avaliar(PedidoCreditoInput pedido, Indicadores indicadores) =>
        pedido.ValorPretendido > indicadores.MontanteMaximoRecomendado
            ? new MotivoAvaliacao(Codigo, EstadoPedidoEnum.AnaliseManual,
                $"Montante superior ao limite recomendado de {MotivoAvaliacao.Euros(indicadores.MontanteMaximoRecomendado)} " +
                $"({parametros.MultiploMaximoRendimento:0} × rendimento).")
            : null;
}
