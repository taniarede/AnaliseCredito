using AnaliseCredito.Api.Enums;

namespace AnaliseCredito.Api.Models;

// Tabela dbo.PedidosCredito - um pedido (as simulações estão na tabela dbo.Simulacoes)
// Os dados de entrada ficam guardados tal como foram submetidos (NULL = campo não preenchido).
public class PedidoCredito
{
    public int Id { get; set; }

    //Coluna calculada pelo SQL Server (ano + sequencial, ex.: 20260001)
    public string NumeroPedido { get; set; } = string.Empty;

    //NULL quando o NIF não tem 9 dígitos (pedido inválido)
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    // Dados de entrada
    public string NifSubmetido { get; set; } = string.Empty;
    public int? Idade { get; set; }
    public decimal? RendimentoMensalLiquido { get; set; }
    public decimal? PrestacoesAtuais { get; set; }
    public decimal? ValorPretendido { get; set; }
    public int? PrazoMeses { get; set; }
    public string SituacaoProfissional { get; set; } = string.Empty;
    public bool IncidentesCredito { get; set; }

    // Indicadores (NULL em pedidos inválidos)
    public decimal? PrestacaoEstimada { get; set; }
    public decimal? TaxaEsforco { get; set; }
    public decimal? IdadeFinalContrato { get; set; }

    // Estados
    //Decisão do motor de regras. Nunca muda.
    public EstadoPedidoEnum EstadoAutomatico { get; set; }
    //Estado atual (muda quando o analista decide um pedido em Análise Manual).
    public EstadoPedidoEnum EstadoAtual { get; set; }

    public string VersaoRegras { get; set; } = string.Empty;
    public DateTime DataPedido { get; set; }
    public DateTime? DataAtualizacao { get; set; }

    public List<MotivoDecisao> Motivos { get; set; } = [];
    public List<HistoricoEstado> Historico { get; set; } = [];
}
