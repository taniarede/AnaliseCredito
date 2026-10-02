using AnaliseCredito.Api.Data;
using AnaliseCredito.Api.DTOs;
using AnaliseCredito.Api.Enums;
using AnaliseCredito.Api.Interfaces;
using AnaliseCredito.Api.Models;
using AnaliseCredito.Api.Regras;
using Microsoft.EntityFrameworkCore;

namespace AnaliseCredito.Api.Services;

// Trabalho do analista sobre pedidos em ANÁLISE MANUAL:
// listar os pendentes e registar a decisão (Aprovado ou Recusado).
// A decisão automática original (EstadoAutomatico) nunca é alterada; a mudança fica no histórico.
public sealed class AnaliseManualService(AppDbContext db, PrioridadeDecisao regras, TimeProvider relogio)
    : IAnaliseManualService
{
    public const string CodigoAnalista = "ANALISTA";

    public async Task<IReadOnlyList<PedidoResponseDTO>> ListarPendentesAsync(CancellationToken ct)
    {
        var pedidos = await db.PedidosCompletos()
            .Where(p => p.EstadoAtual == EstadoPedidoEnum.AnaliseManual)
            .OrderBy(p => p.DataPedido)
            .ToListAsync(ct);

        return pedidos.Select(ParaResposta).ToList();
    }

    public async Task<PedidoResponseDTO?> RegistarDecisaoAsync(int id, DecisaoAnalistaRequestDTO request, CancellationToken ct)
    {
        if (request.NovoEstado is not (EstadoPedidoEnum.Aprovado or EstadoPedidoEnum.Recusado))
            throw new InvalidOperationException("O analista só pode decidir Aprovado ou Recusado.");

        var utilizador = request.Utilizador.Trim();
        var observacao = request.Observacao.Trim();
        if (utilizador.Length == 0 || observacao.Length == 0)
            throw new InvalidOperationException("O nome do analista e a observação são obrigatórios.");

        var pedido = await db.PedidosCompletos().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (pedido is null)
            return null;

        if (pedido.EstadoAtual != EstadoPedidoEnum.AnaliseManual)
            throw new InvalidOperationException(
                $"Só é possível decidir pedidos em ANÁLISE MANUAL (estado atual: {pedido.EstadoAtual.Descricao()}).");

        var novoEstado = request.NovoEstado.Value;
        var agora = relogio.GetUtcNow().UtcDateTime;

        pedido.Historico.Add(new HistoricoEstado
        {
            EstadoAnterior = pedido.EstadoAtual,
            EstadoNovo = novoEstado,
            DataAlteracao = agora,
            Utilizador = utilizador,
            Observacao = observacao
        });
        pedido.Motivos.Add(new MotivoDecisao
        {
            CodigoRegra = CodigoAnalista,
            Estado = novoEstado,
            Descricao = observacao
        });
        pedido.EstadoAtual = novoEstado;
        pedido.DataAtualizacao = agora;

        await db.SaveChangesAsync(ct);
        return ParaResposta(pedido);
    }

    private PedidoResponseDTO ParaResposta(PedidoCredito p) =>
        PedidoResponseDTO.DeModelo(p, p.RendimentoMensalLiquido is { } r ? (decimal?)regras.MontanteMaximo(r) : null);
}
