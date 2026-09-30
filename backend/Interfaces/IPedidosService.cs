using AnaliseCredito.Api.DTOs;

namespace AnaliseCredito.Api.Interfaces;

public interface IPedidosService
{
    //Últimos pedidos e simulações, do mais recente para o mais antigo.
    Task<IReadOnlyList<PedidoResponseDTO>> ListarAsync(int top, CancellationToken ct);

    //Um pedido pelo Id, ou null se não existir.
    Task<PedidoResponseDTO?> ObterAsync(int id, CancellationToken ct);
}
