using AnaliseCredito.Api.DTOs;
using AnaliseCredito.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AnaliseCredito.Api.Controllers;

[ApiController]
[Route("api/analise-manual")]
[Produces("application/json")]
public sealed class AnaliseManualController(IAnaliseManualService analiseManual) : ControllerBase
{
    [HttpGet("pendentes")]
    public async Task<ActionResult<IReadOnlyList<PedidoResponseDTO>>> Pendentes(CancellationToken ct) =>
        Ok(await analiseManual.ListarPendentesAsync(ct));

    [HttpPost("{id:int}/decisao")]
    public async Task<ActionResult<PedidoResponseDTO>> Decidir(int id, DecisaoAnalistaRequestDTO request, CancellationToken ct)
    {
        try
        {
            var pedido = await analiseManual.RegistarDecisaoAsync(id, request, ct);
            return pedido is null ? NotFound() : Ok(pedido);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status409Conflict, title: "Operação não permitida");
        }
    }
}
