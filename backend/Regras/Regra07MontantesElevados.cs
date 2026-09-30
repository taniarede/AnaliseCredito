using AnaliseCredito.Api.Enums;
using AnaliseCredito.Api.Interfaces;
using AnaliseCredito.Api.Models;

namespace AnaliseCredito.Api.Regras;

// Regra 7 - Montantes elevados: valor > 50.000,00 € -> ANÁLISE MANUAL.
// "Independentemente das restantes regras" = aplica-se mesmo que todas as outras aprovem.
// Se outra regra recusar, prevalece a Regra 8 (decisão mais restritiva = RECUSADO).
public sealed class Regra07MontantesElevados(ParametrosCredito parametros) : IRegraCredito
{
    public string Codigo => "R7";

    public MotivoAvaliacao? Avaliar(PedidoCreditoInput pedido, Indicadores indicadores) =>
        pedido.ValorPretendido > parametros.MontanteElevado
            ? new MotivoAvaliacao(Codigo, EstadoPedidoEnum.AnaliseManual,
                $"Montante superior a {MotivoAvaliacao.Euros(parametros.MontanteElevado)}.")
            : null;
}
