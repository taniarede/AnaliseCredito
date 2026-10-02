using AnaliseCredito.Api.Enums;

namespace AnaliseCredito.Api.Models;

// Tabela dbo.Simulacoes - uma simulação, só para contagem.
// Guarda tudo o que foi submetido e calculado, mas não tem estado atual nem histórico:
// uma simulação não é um pedido, não pode ser decidida por um analista e nunca muda.
// Não cria nem referencia um Cliente; o NIF fica guardado tal como foi escrito.
public class Simulacao
{
    public int Id { get; set; }

    // Dados de entrada (NULL = campo não preenchido)
    public string NifSubmetido { get; set; } = string.Empty;
    public int? Idade { get; set; }
    public decimal? RendimentoMensalLiquido { get; set; }
    public decimal? PrestacoesAtuais { get; set; }
    public decimal? ValorPretendido { get; set; }
    public int? PrazoMeses { get; set; }
    public string SituacaoProfissional { get; set; } = string.Empty;
    public bool IncidentesCredito { get; set; }

    // Indicadores (NULL quando a simulação é inválida)
    public decimal? PrestacaoEstimada { get; set; }
    public decimal? TaxaEsforco { get; set; }
    public decimal? IdadeFinalContrato { get; set; }

    // Decisão do motor de regras e regras que dispararam (ex.: "R4,R6"; NULL se nenhuma)
    public EstadoPedidoEnum Decisao { get; set; }
    public string? CodigosRegras { get; set; }

    public string VersaoRegras { get; set; } = string.Empty;
    public DateTime DataSimulacao { get; set; }
}
