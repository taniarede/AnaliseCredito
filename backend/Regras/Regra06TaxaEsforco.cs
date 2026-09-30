using AnaliseCredito.Api.Enums;
using AnaliseCredito.Api.Interfaces;
using AnaliseCredito.Api.Models;

namespace AnaliseCredito.Api.Regras;

// Regra 6 - Taxa de esforço.
//   até 35% (inclusive)       -> mantém decisão
//   35% e até 50% (inclusive) -> ANÁLISE MANUAL
//   > 50%                     -> RECUSADO
// A comparação é feita com a taxa arredondada a 2 casas (a mesma que o cliente vê).
public sealed class Regra06TaxaEsforco(ParametrosCredito parametros) : IRegraCredito
{
    public string Codigo => "R6";

    public MotivoAvaliacao? Avaliar(PedidoCreditoInput pedido, Indicadores indicadores)
    {
        var taxa = indicadores.TaxaEsforco;

        if (taxa > parametros.TaxaEsforcoLimiteRecusa)
            return new MotivoAvaliacao(Codigo, EstadoPedidoEnum.Recusado,
                $"Taxa de esforço de {MotivoAvaliacao.Percentagem(taxa)} superior a {parametros.TaxaEsforcoLimiteRecusa:0}%.");

        if (taxa > parametros.TaxaEsforcoLimiteAnaliseManual)
            return new MotivoAvaliacao(Codigo, EstadoPedidoEnum.AnaliseManual,
                $"Taxa de esforço de {MotivoAvaliacao.Percentagem(taxa)} superior a {parametros.TaxaEsforcoLimiteAnaliseManual:0}% " +
                $"e até {parametros.TaxaEsforcoLimiteRecusa:0}%.");

        return null;
    }
}
