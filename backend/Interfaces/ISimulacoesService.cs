using AnaliseCredito.Api.DTOs;

namespace AnaliseCredito.Api.Interfaces;

public interface ISimulacoesService
{
    //Aplica as regras e grava a simulação na tabela Simulacoes (sem estado nem histórico).
    Task<SimulacaoResponseDTO> SimularAsync(PedidoRequestDTO request, CancellationToken ct);
}
