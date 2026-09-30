using AnaliseCredito.Api.Data;
using AnaliseCredito.Api.DTOs;
using AnaliseCredito.Api.Interfaces;
using AnaliseCredito.Api.Models;
using AnaliseCredito.Api.Regras;

namespace AnaliseCredito.Api.Services;

// Avaliação de um pedido: pede a decisão às regras (PrioridadeDecisao), grava o pedido,
// os motivos e o primeiro registo de histórico, e devolve a resposta.
// É usado tanto para pedidos como para simulações (a diferença é só a marca Simulacao).
public sealed class AvaliacaoService(
    AppDbContext db,
    PrioridadeDecisao regras,
    IClienteService clientes,
    TimeProvider relogio) : IAvaliacaoService
{
    public const string UtilizadorSistema = "SISTEMA";

    public async Task<PedidoResponseDTO> AvaliarAsync(PedidoRequestDTO request, bool ehSimulacao, CancellationToken ct)
    {
        var input = request.ParaModelo();
        var resultado = regras.Avaliar(input);
        var agora = relogio.GetUtcNow().UtcDateTime;
        var cliente = await clientes.ObterOuCriarAsync(input.Nif, ct);

        var pedido = new PedidoCredito
        {
            ClienteId = cliente?.Id,
            NifSubmetido = input.Nif,
            Idade = input.Idade,
            RendimentoMensalLiquido = input.RendimentoMensalLiquido,
            PrestacoesAtuais = input.PrestacoesAtuais,
            ValorPretendido = input.ValorPretendido,
            PrazoMeses = input.PrazoMeses,
            SituacaoProfissional = input.SituacaoProfissional,
            IncidentesCredito = input.IncidentesCredito,
            EhSimulacao = ehSimulacao,
            PrestacaoEstimada = resultado.Indicadores?.PrestacaoEstimada,
            TaxaEsforco = resultado.Indicadores?.TaxaEsforco,
            IdadeFinalContrato = resultado.Indicadores?.IdadeFinalContrato,
            EstadoAutomatico = resultado.Decisao,
            EstadoAtual = resultado.Decisao,
            VersaoRegras = regras.Parametros.VersaoRegras,
            DataPedido = agora,
            Motivos = resultado.Motivos
                .Select(m => new MotivoDecisao
                {
                    CodigoRegra = m.CodigoRegra,
                    Estado = m.Decisao,
                    Descricao = m.Descricao,
                    Campo = m.Campo
                })
                .ToList(),
            Historico =
            [
                new HistoricoEstado
                {
                    EstadoAnterior = null,
                    EstadoNovo = resultado.Decisao,
                    DataAlteracao = agora,
                    Utilizador = UtilizadorSistema,
                    Observacao = ehSimulacao ? "Simulação - decisão automática" : "Decisão automática do motor de regras"
                }
            ]
        };

        db.PedidosCredito.Add(pedido);
        await db.SaveChangesAsync(ct);

        return PedidoResponseDTO.DeModelo(pedido, resultado.Indicadores?.MontanteMaximoRecomendado);
    }
}
