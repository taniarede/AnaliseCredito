using AnaliseCredito.Api.DTOs;

namespace AnaliseCredito.Api.Interfaces;

public interface IAnaliseManualService
{
    //Pedidos que estão à espera de decisão do analista.
    Task<IReadOnlyList<PedidoResponseDTO>> ListarPendentesAsync(CancellationToken ct);

    // Regista a decisão do analista. Devolve null se o pedido não existir.
    // Lança InvalidOperationException se a decisão não for permitida.
    Task<PedidoResponseDTO?> RegistarDecisaoAsync(int id, DecisaoAnalistaRequestDTO request, CancellationToken ct);
}
