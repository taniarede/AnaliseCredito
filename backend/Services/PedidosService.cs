using AnaliseCredito.Api.Data;
using AnaliseCredito.Api.DTOs;
using AnaliseCredito.Api.Interfaces;
using AnaliseCredito.Api.Models;
using AnaliseCredito.Api.Regras;
using Microsoft.EntityFrameworkCore;

namespace AnaliseCredito.Api.Services;

//Consulta de pedidos (lista e detalhe).
public sealed class PedidosService(AppDbContext db, PrioridadeDecisao regras) : IPedidosService
{
    public async Task<IReadOnlyList<PedidoResponseDTO>> ListarAsync(int top, CancellationToken ct)
    {
        var pedidos = await db.PedidosCompletos()
            .OrderByDescending(p => p.Id)
            .Take(Math.Clamp(top, 1, 200))
            .ToListAsync(ct);

        return pedidos.Select(ParaResposta).ToList();
    }

    public async Task<PedidoResponseDTO?> ObterAsync(int id, CancellationToken ct)
    {
        var pedido = await db.PedidosCompletos().FirstOrDefaultAsync(p => p.Id == id, ct);
        return pedido is null ? null : ParaResposta(pedido);
    }

    private PedidoResponseDTO ParaResposta(PedidoCredito p) =>
        PedidoResponseDTO.DeModelo(p, p.RendimentoMensalLiquido is { } r ? (decimal?)regras.MontanteMaximo(r) : null);
}
