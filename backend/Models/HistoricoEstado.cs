using AnaliseCredito.Api.Enums;

namespace AnaliseCredito.Api.Models;

// Tabela dbo.HistoricoEstados - cada mudança de estado de um pedido - ALTERAR PARA MESMO ID SER SUBSTITUIDO!! 
public class HistoricoEstado
{
    public int Id { get; set; }
    public int PedidoId { get; set; }

    // NULL no primeiro registo (decisão automática do sistema)
    public EstadoPedidoEnum? EstadoAnterior { get; set; }
    public EstadoPedidoEnum EstadoNovo { get; set; }

    public DateTime DataAlteracao { get; set; }
    public string Utilizador { get; set; } = string.Empty;
    public string? Observacao { get; set; }
}
