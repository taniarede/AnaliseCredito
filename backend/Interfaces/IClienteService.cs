using AnaliseCredito.Api.Models;

namespace AnaliseCredito.Api.Interfaces;

public interface IClienteService
{
    // Devolve o cliente com este NIF, criando-o se ainda não existir.
    // Devolve null se o NIF não tiver 9 dígitos (o pedido fica sem cliente associado).
    Task<Cliente?> ObterOuCriarAsync(string nif, CancellationToken ct);
}
