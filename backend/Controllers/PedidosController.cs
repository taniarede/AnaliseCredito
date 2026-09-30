using AnaliseCredito.Api.DTOs;
using AnaliseCredito.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AnaliseCredito.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
[Produces("application/json")]
public sealed class PedidosController(IAvaliacaoService avaliacao, IPedidosService pedidos) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PedidoResponseDTO>> Submeter(PedidoRequestDTO request, CancellationToken ct)
    {
        var resposta = await avaliacao.AvaliarAsync(request, ehSimulacao: false, ct);
        return CreatedAtAction(nameof(Obter), new { id = resposta.Id }, resposta);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PedidoResponseDTO>>> Listar([FromQuery] int top = 50, CancellationToken ct = default) =>
        Ok(await pedidos.ListarAsync(top, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PedidoResponseDTO>> Obter(int id, CancellationToken ct)
    {
        var pedido = await pedidos.ObterAsync(id, ct);
        return pedido is null ? NotFound() : Ok(pedido);
    }
}
