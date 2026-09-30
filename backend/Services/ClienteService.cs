using AnaliseCredito.Api.Data;
using AnaliseCredito.Api.Interfaces;
using AnaliseCredito.Api.Models;
using AnaliseCredito.Api.Regras;
using Microsoft.EntityFrameworkCore;

namespace AnaliseCredito.Api.Services;

//Gestão de clientes: um cliente por NIF.
public sealed class ClienteService(AppDbContext db, TimeProvider relogio) : IClienteService
{
    public async Task<Cliente?> ObterOuCriarAsync(string nif, CancellationToken ct)
    {
        if (!ValidadorNif.TemNoveDigitos(nif))
            return null;

        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Nif == nif, ct);
        if (cliente is not null)
            return cliente;

        cliente = new Cliente { Nif = nif, DataCriacao = relogio.GetUtcNow().UtcDateTime };
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync(ct);
        return cliente;
    }
}
