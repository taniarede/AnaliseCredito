using AnaliseCredito.Api.Enums;
using AnaliseCredito.Api.Interfaces;
using AnaliseCredito.Api.Models;

namespace AnaliseCredito.Api.Regras;

/// Aplica as regras do enunciado pela ordem e decide:
///
///   1. Regra 1 (validação). Se falhar -> PEDIDO INVÁLIDO e termina aqui.
///   2. Cálculo dos indicadores (prestação, taxa de esforço, idade final, limite de montante).
///   3. Regras 2 a 7: são TODAS avaliadas (não para na primeira), para que o resultado
///      apresente todos os motivos relevantes.
///   4. Regra 8 - prioridade: a decisão final é a mais restritiva entre os motivos recolhidos.
///      Sem motivos -> APROVADO.
///
/// Não depende de base de dados nem de HTTP ----> fácil de testar!
public sealed class PrioridadeDecisao
{
    private readonly IReadOnlyList<IRegraCredito> _regras;

    public PrioridadeDecisao(ParametrosCredito parametros)
    {
        Parametros = parametros;
        _regras =
        [
            new Regra02IdadeMaxima(parametros),
            new Regra03IncidentesCredito(),
            new Regra04SituacaoProfissional(),
            new Regra05LimiteMontante(parametros),
            new Regra06TaxaEsforco(parametros),
            new Regra07MontantesElevados(parametros)
        ];
    }

    public ParametrosCredito Parametros { get; }

    public ResultadoAvaliacao Avaliar(PedidoCreditoInput pedido)
    {
        // 1. Regra 1 - barreira
        var erros = Regra01ValidacaoInicial.Validar(pedido, Parametros);
        if (erros.Count > 0)
            return new ResultadoAvaliacao(EstadoPedidoEnum.PedidoInvalido, erros, null);

        // 2. Indicadores
        var indicadores = CalcularIndicadores(pedido);

        // 3. Regras 2 a 7
        var motivos = _regras
            .Select(regra => regra.Avaliar(pedido, indicadores))
            .OfType<MotivoAvaliacao>()
            .ToList();

        // 4. Regra 8 - a mais restritiva (o enum está ordenado por severidade)
        var decisao = motivos.Count == 0
            ? EstadoPedidoEnum.Aprovado
            : motivos.Max(m => m.Decisao);

        return new ResultadoAvaliacao(decisao, motivos, indicadores);
    }

    // PrestaçãoNova = ValorPretendido / PrazoMeses (simplificação do enunciado, sem juros).
    // TaxaEsforço   = (PrestaçõesAtuais + PrestaçãoNova) / Rendimento × 100.
    // IdadeFinal    = Idade + PrazoMeses / 12 (com decimais; o frontend decide como mostrar).
    // A taxa é calculada com a prestação sem arredondamento e só no fim é arredondada.
    // Só é chamado depois da Regra 1, por isso todos os campos estão preenchidos e são válidos.
    public Indicadores CalcularIndicadores(PedidoCreditoInput pedido)
    {
        var valor = pedido.ValorPretendido!.Value;
        var prazo = pedido.PrazoMeses!.Value;
        var rendimento = pedido.RendimentoMensalLiquido!.Value;
        var prestacoes = pedido.PrestacoesAtuais!.Value;
        var idade = pedido.Idade!.Value;

        var prestacaoNova = valor / prazo;
        var taxaEsforco = (prestacoes + prestacaoNova) / rendimento * 100m;
        var idadeFinal = idade + prazo / 12m;

        return new Indicadores(
            Arredondar(prestacaoNova),
            Arredondar(taxaEsforco),
            Arredondar(idadeFinal),
            MontanteMaximo(rendimento));
    }

    //Montante máximo recomendado = rendimento × 20 (Regra 5).
    public decimal MontanteMaximo(decimal rendimento) =>
        Arredondar(rendimento * Parametros.MultiploMaximoRendimento);

    // Arredondamento comercial a 2 casas (0,005 -> 0,01).
    // Faz parte das regras: os limites (35,00% e 50,00%) são comparados com o valor arredondado.
    public static decimal Arredondar(decimal valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);
}
