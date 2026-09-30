using AnaliseCredito.Api.Enums;

namespace AnaliseCredito.Api.Models;

//Tabela dbo.MotivosDecisao - um registo por motivo  - TRATAR DISTO!!
public class MotivoDecisao
{
    public int Id { get; set; }
    public int PedidoId { get; set; }

    //R1 … R7 ou ANALISTA
    public string CodigoRegra { get; set; } = string.Empty;

    //Decisão que este motivo pede
    public EstadoPedidoEnum Estado { get; set; }

    public string Descricao { get; set; } = string.Empty;

    //Só nos erros da Regra 1: o campo do formulário com problema (ex.: "nif")
    public string? Campo { get; set; }
}
