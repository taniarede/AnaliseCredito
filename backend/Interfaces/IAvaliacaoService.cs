using AnaliseCredito.Api.DTOs;

namespace AnaliseCredito.Api.Interfaces;

public interface IAvaliacaoService
{
    //Aplica as regras ao pedido e grava-o (como pedido ou como simulação).
    Task<PedidoResponseDTO> AvaliarAsync(PedidoRequestDTO request, bool ehSimulacao, CancellationToken ct);
}
