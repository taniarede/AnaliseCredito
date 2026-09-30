namespace AnaliseCredito.Api.Models;

// um registo por NIF
public class Cliente
{
    public int Id { get; set; }
    public string Nif { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }

    public List<PedidoCredito> Pedidos { get; set; } = [];
}
