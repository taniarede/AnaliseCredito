using System.ComponentModel.DataAnnotations;
using AnaliseCredito.Api.Enums;

namespace AnaliseCredito.Api.DTOs;

// POST /api/analise-manual/{id}/decisao
public sealed class DecisaoAnalistaRequestDTO
{
    // Aprovado ou Recusado.
    [Required]
    public EstadoPedidoEnum? NovoEstado { get; set; }

    [Required(ErrorMessage = "O nome do analista é obrigatório.")]
    [StringLength(100)]
    public string Utilizador { get; set; } = string.Empty;

    [Required(ErrorMessage = "A observação é obrigatória.")]
    [StringLength(500)]
    public string Observacao { get; set; } = string.Empty;
}
