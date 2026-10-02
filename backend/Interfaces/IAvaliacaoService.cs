using AnaliseCredito.Api.DTOs;

namespace AnaliseCredito.Api.Interfaces;

public interface IAvaliacaoService
{
    //Aplica as regras ao pedido e grava-o (pedido, motivos e primeiro registo de histórico).
    Task<PedidoResponseDTO> AvaliarAsync(PedidoRequestDTO request, CancellationToken ct);
}
