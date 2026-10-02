using AnaliseCredito.Api.DTOs;
using AnaliseCredito.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AnaliseCredito.Api.Controllers;

// Simulações: mesmas regras de um pedido, gravadas à parte (tabela Simulacoes), só para contagem.
[ApiController]
[Route("api/simulacoes")]
[Produces("application/json")]
public sealed class SimulacoesController(ISimulacoesService simulacoes) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SimulacaoResponseDTO>> Simular(PedidoRequestDTO request, CancellationToken ct) =>
        Ok(await simulacoes.SimularAsync(request, ct));
}
