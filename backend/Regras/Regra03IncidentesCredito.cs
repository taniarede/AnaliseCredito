using AnaliseCredito.Api.Enums;
using AnaliseCredito.Api.Interfaces;
using AnaliseCredito.Api.Models;

namespace AnaliseCredito.Api.Regras;

//Regra 3 - Incidentes de crédito registados -> RECUSADO.
public sealed class Regra03IncidentesCredito : IRegraCredito
{
    public string Codigo => "R3";

    public MotivoAvaliacao? Avaliar(PedidoCreditoInput pedido, Indicadores indicadores) =>
        pedido.IncidentesCredito
            ? new MotivoAvaliacao(Codigo, EstadoPedidoEnum.Recusado, "Cliente com incidentes de crédito registados.")
            : null;
}
