using AnaliseCredito.Api.Models;

namespace AnaliseCredito.Api.Interfaces;

// Contrato comum às Regras 2 a 7.
// Cada regra olha para o pedido e para os indicadores já calculados e devolve:
//   - null, se não tem objecções (o pedido "prossegue");
//   - um MotivoAvaliacao, com a decisão que exige e a explicação.
public interface IRegraCredito
{
    string Codigo { get; }

    MotivoAvaliacao? Avaliar(PedidoCreditoInput pedido, Indicadores indicadores);
}



// Para acrescentar uma regra nova basta criar uma classe e registá-la na PrioridadeDecisao.
