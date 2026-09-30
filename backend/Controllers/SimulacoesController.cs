using AnaliseCredito.Api.DTOs;
using AnaliseCredito.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AnaliseCredito.Api.Controllers;

[ApiController]
[Route("api/simulacoes")]
[Produces("application/json")]
public sealed class SimulacoesController(IAvaliacaoService avaliacao) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PedidoResponseDTO>> Simular(PedidoRequestDTO request, CancellationToken ct) =>
        Ok(await avaliacao.AvaliarAsync(request, ehSimulacao: true, ct));
}
