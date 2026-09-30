using AnaliseCredito.Api.Enums;
using AnaliseCredito.Api.Interfaces;
using AnaliseCredito.Api.Models;

namespace AnaliseCredito.Api.Regras;

// Regra 2 - Idade máxima no final do contrato.
// IdadeFinal = Idade + PrazoMeses / 12 (calculada com decimais na PrioridadeDecisao).
// Se IdadeFinal > 75 -> ANÁLISE MANUAL. Exatamente 75 anos é aceite ("não poderá ultrapassar").
public sealed class Regra02IdadeMaxima(ParametrosCredito parametros) : IRegraCredito
{
    public string Codigo => "R2";

    public MotivoAvaliacao? Avaliar(PedidoCreditoInput pedido, Indicadores indicadores) =>
        indicadores.IdadeFinalContrato > parametros.IdadeMaximaFimContrato
            ? new MotivoAvaliacao(Codigo, EstadoPedidoEnum.AnaliseManual,
                $"Idade no final do contrato ({MotivoAvaliacao.Numero(indicadores.IdadeFinalContrato)} anos) " +
                $"superior a {parametros.IdadeMaximaFimContrato:0} anos.")
            : null;
}
