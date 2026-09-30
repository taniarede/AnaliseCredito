using System.ComponentModel.DataAnnotations;
using AnaliseCredito.Api.Models;

namespace AnaliseCredito.Api.DTOs;

// Dados recebidos do frontend (POST /api/pedidos e POST /api/simulacoes).
// Os campos numéricos podem vir a null (campo vazio no formulário).
// As regras de negócio NÃO são validadas aqui (isso é a Regra 1): um pedido com prazo 0 ou vazio
// tem de devolver "PEDIDO INVÁLIDO" com o motivo, e não um erro técnico.

public sealed class PedidoRequestDTO
{
    [StringLength(20)]
    public string? Nif { get; set; }

    [Range(-1000, 1000)]
    public int? Idade { get; set; }

    // Os [Range] só protegem contra valores absurdos que fariam rebentar os cálculos ou a base de dados.
    [Range(typeof(decimal), "-1000000000", "1000000000")]
    public decimal? RendimentoMensalLiquido { get; set; }

    [Range(typeof(decimal), "-1000000000", "1000000000")]
    public decimal? PrestacoesAtuais { get; set; }

    [Range(typeof(decimal), "-1000000000", "1000000000")]
    public decimal? ValorPretendido { get; set; }

    [Range(-100000, 100000)]
    public int? PrazoMeses { get; set; }

    [StringLength(30)]
    public string? SituacaoProfissional { get; set; }

    public bool IncidentesCredito { get; set; }

    public PedidoCreditoInput ParaModelo() => new(
        Nif?.Trim() ?? string.Empty,
        Idade,
        RendimentoMensalLiquido,
        PrestacoesAtuais,
        ValorPretendido,
        PrazoMeses,
        SituacaoProfissional?.Trim() ?? string.Empty,
        IncidentesCredito);
}
