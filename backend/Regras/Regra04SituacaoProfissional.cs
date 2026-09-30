using AnaliseCredito.Api.Enums;
using AnaliseCredito.Api.Interfaces;
using AnaliseCredito.Api.Models;

namespace AnaliseCredito.Api.Regras;

//Regra 4 - Situação profissional: Efetivo prossegue; ContratoPrazo -> MANUAL; Desempregado -> RECUSADO.
public sealed class Regra04SituacaoProfissional : IRegraCredito
{
    public string Codigo => "R4";

    public MotivoAvaliacao? Avaliar(PedidoCreditoInput pedido, Indicadores indicadores)
    {
        // A Regra 1 já garantiu que a situação é válida.
        SituacaoProfissionalEnumParser.TryParse(pedido.SituacaoProfissional, out var situacao);

        return situacao switch
        {
            SituacaoProfissionalEnum.ContratoPrazo =>
                new MotivoAvaliacao(Codigo, EstadoPedidoEnum.AnaliseManual, "Cliente com contrato a prazo."),
            SituacaoProfissionalEnum.Desempregado =>
                new MotivoAvaliacao(Codigo, EstadoPedidoEnum.Recusado, "Cliente desempregado."),
            _ => null
        };
    }
}
