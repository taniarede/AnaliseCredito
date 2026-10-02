using AnaliseCredito.Api.Enums;
using AnaliseCredito.Api.Models;
using AnaliseCredito.Api.Regras;

namespace AnaliseCredito.Tests;

/// <summary>
/// Testes das regras de negócio e da PrioridadeDecisao (sem base de dados).
/// Executar com:  dotnet test
/// </summary>
public class PrioridadeDecisaoTests
{
    private readonly PrioridadeDecisao _motor = new(new ParametrosCredito());

    /// <summary>Pedido "bom" de base: aprovado. Cada teste altera só o que precisa.</summary>
    private static PedidoCreditoInput PedidoBase(
        string nif = "123456789",
        int? idade = 35,
        decimal? rendimento = 2500m,
        decimal? prestacoes = 200m,
        decimal? valor = 10000m,
        int? prazo = 60,
        string situacao = "Efetivo",
        bool incidentes = false) =>
        new(nif, idade, rendimento, prestacoes, valor, prazo, situacao, incidentes);

    private static string[] Regras(ResultadoAvaliacao r) => r.Motivos.Select(m => m.CodigoRegra).ToArray();

    // ---------------------------------------------------------------------
    // Cenários do enunciado (Tarefa 2)
    // ---------------------------------------------------------------------

    [Fact]
    public void CenarioA_Aprovado()
    {
        var r = _motor.Avaliar(PedidoBase());

        Assert.Equal(EstadoPedidoEnum.Aprovado, r.Decisao);
        Assert.Empty(r.Motivos);
        Assert.Equal(166.67m, r.Indicadores!.PrestacaoEstimada);
        Assert.Equal(14.67m, r.Indicadores.TaxaEsforco);
        Assert.Equal(40.00m, r.Indicadores.IdadeFinalContrato);
    }

    [Fact]
    public void CenarioB_AnaliseManual_ContratoPrazoETaxaEntre35e50()
    {
        var r = _motor.Avaliar(PedidoBase(idade: 42, rendimento: 2000, prestacoes: 500, valor: 20000, prazo: 48, situacao: "ContratoPrazo"));

        Assert.Equal(EstadoPedidoEnum.AnaliseManual, r.Decisao);
        Assert.Equal(new[] { "R4", "R6" }, Regras(r));
        Assert.Equal(416.67m, r.Indicadores!.PrestacaoEstimada);
        Assert.Equal(45.83m, r.Indicadores.TaxaEsforco);
    }

    [Fact]
    public void CenarioC_Recusado_Incidentes()
    {
        var r = _motor.Avaliar(PedidoBase(idade: 40, rendimento: 3000, prestacoes: 300, valor: 15000, prazo: 72, incidentes: true));

        Assert.Equal(EstadoPedidoEnum.Recusado, r.Decisao);
        Assert.Equal(new[] { "R3" }, Regras(r));
        Assert.Equal(208.33m, r.Indicadores!.PrestacaoEstimada);
        Assert.Equal(16.94m, r.Indicadores.TaxaEsforco);
    }

    [Fact]
    public void CenarioD_Recusado_TaxaAcima50_PrevaleceSobreManual()
    {
        var r = _motor.Avaliar(PedidoBase(idade: 30, rendimento: 1200, prestacoes: 300, valor: 25000, prazo: 36));

        Assert.Equal(EstadoPedidoEnum.Recusado, r.Decisao);
        Assert.Equal(new[] { "R5", "R6" }, Regras(r));
        Assert.Equal(694.44m, r.Indicadores!.PrestacaoEstimada);
        Assert.Equal(82.87m, r.Indicadores.TaxaEsforco);
        Assert.Equal(24000m, r.Indicadores.MontanteMaximoRecomendado);
    }

    // ---------------------------------------------------------------------
    // Regra 1 - validação
    // ---------------------------------------------------------------------

    [Theory]
    [InlineData("12345678")]    // 8 dígitos
    [InlineData("1234567890")]  // 10 dígitos
    [InlineData("12345678A")]   // letra
    [InlineData("")]
    [InlineData("123456788")]   // dígito de controlo errado
    public void Regra1_NifInvalido(string nif)
    {
        var r = _motor.Avaliar(PedidoBase(nif: nif));

        Assert.Equal(EstadoPedidoEnum.PedidoInvalido, r.Decisao);
        Assert.Null(r.Indicadores);
    }

    [Fact]
    public void Regra1_DigitoControloPodeSerDesligado()
    {
        var motor = new PrioridadeDecisao(new ParametrosCredito { ValidarDigitoControloNif = false });

        Assert.Equal(EstadoPedidoEnum.Aprovado, motor.Avaliar(PedidoBase(nif: "123456788")).Decisao);
    }

    [Fact]
    public void Regra1_Idade18EAceite_17EInvalido()
    {
        Assert.NotEqual(EstadoPedidoEnum.PedidoInvalido, _motor.Avaliar(PedidoBase(idade: 18)).Decisao);
        Assert.Equal(EstadoPedidoEnum.PedidoInvalido, _motor.Avaliar(PedidoBase(idade: 17)).Decisao);
    }

    [Fact]
    public void Regra1_RecolheTodosOsErrosEParaAntesDasOutrasRegras()
    {
        // prazo 0 não pode rebentar (divisão por zero) e incidentes não devem aparecer
        var r = _motor.Avaliar(PedidoBase(nif: "123", idade: 16, rendimento: 0, valor: 0, prazo: 0, incidentes: true));

        Assert.Equal(EstadoPedidoEnum.PedidoInvalido, r.Decisao);
        Assert.Equal(5, r.Motivos.Count);
        Assert.All(r.Motivos, m => Assert.Equal("R1", m.CodigoRegra));
    }

    [Theory]
    [InlineData("Reformado")]
    [InlineData("1")]
    [InlineData("")]
    public void Regra1_SituacaoProfissionalDesconhecida(string situacao) =>
        Assert.Equal(EstadoPedidoEnum.PedidoInvalido, _motor.Avaliar(PedidoBase(situacao: situacao)).Decisao);

    [Fact]
    public void Regra1_PrestacoesNegativas() =>
        Assert.Equal(EstadoPedidoEnum.PedidoInvalido, _motor.Avaliar(PedidoBase(prestacoes: -1)).Decisao);

    [Fact]
    public void Regra1_CamposVazios_SaoObrigatoriosEIndicamOCampo()
    {
        var r = _motor.Avaliar(PedidoBase(nif: "", idade: null, rendimento: null, prestacoes: null,
            valor: null, prazo: null, situacao: ""));

        Assert.Equal(EstadoPedidoEnum.PedidoInvalido, r.Decisao);
        Assert.Equal(
            new[] { "nif", "idade", "rendimentoMensalLiquido", "prestacoesAtuais", "valorPretendido", "prazoMeses", "situacaoProfissional" },
            r.Motivos.Select(m => m.Campo).ToArray());
    }

    [Fact]
    public void Regra1_PrestacoesZero_SaoAceites() =>
        Assert.Equal(EstadoPedidoEnum.Aprovado, _motor.Avaliar(PedidoBase(prestacoes: 0)).Decisao);

    [Fact]
    public void Regra1_SoOsCamposErradosSaoAssinalados()
    {
        var r = _motor.Avaliar(PedidoBase(nif: "12345", idade: 16, rendimento: null, prazo: 0));

        Assert.Equal(new[] { "nif", "idade", "rendimentoMensalLiquido", "prazoMeses" }, r.Motivos.Select(m => m.Campo).ToArray());
    }

    // ---------------------------------------------------------------------
    // Regra 2 - idade no final do contrato = Idade + PrazoMeses / 12
    // ---------------------------------------------------------------------

    [Fact]
    public void Regra2_Exatamente75_Aprovado()
    {
        var r = _motor.Avaliar(PedidoBase(idade: 70, prazo: 60));   // 70 + 60/12 = 75

        Assert.Equal(75.00m, r.Indicadores!.IdadeFinalContrato);
        Assert.Equal(EstadoPedidoEnum.Aprovado, r.Decisao);
    }

    [Fact]
    public void Regra2_UmMesAcimaDe75_AnaliseManual()
    {
        var r = _motor.Avaliar(PedidoBase(idade: 70, prazo: 61));   // 75,08

        Assert.Equal(EstadoPedidoEnum.AnaliseManual, r.Decisao);
        Assert.Equal(new[] { "R2" }, Regras(r));
    }

    // ---------------------------------------------------------------------
    // Regra 4 - situação profissional
    // ---------------------------------------------------------------------

    [Fact]
    public void Regra4_Desempregado_Recusado() =>
        Assert.Equal(EstadoPedidoEnum.Recusado, _motor.Avaliar(PedidoBase(situacao: "Desempregado")).Decisao);

    [Fact]
    public void Regra4_AceitaMaiusculasMinusculas() =>
        Assert.Equal(EstadoPedidoEnum.AnaliseManual, _motor.Avaliar(PedidoBase(situacao: "contratoprazo")).Decisao);

    // ---------------------------------------------------------------------
    // Regra 5 - limite de 20x o rendimento
    // ---------------------------------------------------------------------

    [Fact]
    public void Regra5_Exatamente20x_Aprovado() =>
        Assert.Equal(EstadoPedidoEnum.Aprovado,
            _motor.Avaliar(PedidoBase(rendimento: 1000, prestacoes: 0, valor: 20000, prazo: 120)).Decisao);

    [Fact]
    public void Regra5_AcimaDe20x_AnaliseManual()
    {
        var r = _motor.Avaliar(PedidoBase(rendimento: 1000, prestacoes: 0, valor: 20000.01m, prazo: 120));

        Assert.Equal(EstadoPedidoEnum.AnaliseManual, r.Decisao);
        Assert.Equal(new[] { "R5" }, Regras(r));
    }

    // ---------------------------------------------------------------------
    // Regra 6 - taxa de esforço (limites inclusivos)
    // rendimento 1000, sem prestações, prazo 12 -> taxa = valor / 120
    // ---------------------------------------------------------------------

    [Theory]
    [InlineData(4200.00, 35.00, EstadoPedidoEnum.Aprovado)]
    [InlineData(4201.20, 35.01, EstadoPedidoEnum.AnaliseManual)]
    [InlineData(6000.00, 50.00, EstadoPedidoEnum.AnaliseManual)]
    [InlineData(6001.20, 50.01, EstadoPedidoEnum.Recusado)]
    // Fronteira do arredondamento: compara-se a taxa já arredondada a 2 casas (AwayFromZero)
    [InlineData(4200.48, 35.00, EstadoPedidoEnum.Aprovado)]       // 35,004% -> 35,00%
    [InlineData(4200.60, 35.01, EstadoPedidoEnum.AnaliseManual)]  // 35,005% -> 35,01%
    [InlineData(6000.48, 50.00, EstadoPedidoEnum.AnaliseManual)]  // 50,004% -> 50,00%
    [InlineData(6000.60, 50.01, EstadoPedidoEnum.Recusado)]       // 50,005% -> 50,01%
    public void Regra6_Limites(double valor, double taxaEsperada, EstadoPedidoEnum esperado)
    {
        var r = _motor.Avaliar(PedidoBase(rendimento: 1000, prestacoes: 0, valor: (decimal)valor, prazo: 12));

        Assert.Equal((decimal)taxaEsperada, r.Indicadores!.TaxaEsforco);
        Assert.Equal(esperado, r.Decisao);
    }

    // ---------------------------------------------------------------------
    // Regra 7 - montantes elevados e Regra 8 - prioridade
    // ---------------------------------------------------------------------

    [Fact]
    public void Regra7_Exatamente50000_Aprovado() =>
        Assert.Equal(EstadoPedidoEnum.Aprovado,
            _motor.Avaliar(PedidoBase(rendimento: 5000, prestacoes: 0, valor: 50000, prazo: 120)).Decisao);

    [Fact]
    public void Regra7_AcimaDe50000_AnaliseManual()
    {
        var r = _motor.Avaliar(PedidoBase(rendimento: 5000, prestacoes: 0, valor: 50000.01m, prazo: 120));

        Assert.Equal(EstadoPedidoEnum.AnaliseManual, r.Decisao);
        Assert.Equal(new[] { "R7" }, Regras(r));
    }

    [Fact]
    public void Regra8_Regra7ComIncidentes_PrevaleceRecusado()
    {
        var r = _motor.Avaliar(PedidoBase(rendimento: 5000, prestacoes: 0, valor: 60000, prazo: 120, incidentes: true));

        Assert.Equal(EstadoPedidoEnum.Recusado, r.Decisao);
        Assert.Equal(new[] { "R3", "R7" }, Regras(r));
    }

    [Fact]
    public void Regra8_DoisMotivosManual_ContinuaAnaliseManual()
    {
        // T22 - idade final 77 (R2) + contrato a prazo (R4): dois Manual não fazem um Recusado
        var r = _motor.Avaliar(PedidoBase(idade: 72, situacao: "ContratoPrazo"));

        Assert.Equal(EstadoPedidoEnum.AnaliseManual, r.Decisao);
        Assert.Equal(new[] { "R2", "R4" }, Regras(r));
    }

    [Fact]
    public void Regra8_TresMotivosRecusa_Recusado()
    {
        // T23 - incidentes (R3) + desempregado (R4) + taxa 61,11% (R6)
        var r = _motor.Avaliar(PedidoBase(rendimento: 600, situacao: "Desempregado", incidentes: true));

        Assert.Equal(EstadoPedidoEnum.Recusado, r.Decisao);
        Assert.Equal(new[] { "R3", "R4", "R6" }, Regras(r));
        Assert.Equal(61.11m, r.Indicadores!.TaxaEsforco);
        Assert.All(r.Motivos, m => Assert.Equal(EstadoPedidoEnum.Recusado, m.Decisao));
    }

    [Fact]
    public void Regra8_DoisManualEUmRecusado_Recusado()
    {
        // T25 - incidentes (R3 Recusado) + contrato a prazo (R4 Manual) + 60.000 € (R7 Manual).
        // O limite de 20x (80.000 €) não dispara.
        var r = _motor.Avaliar(PedidoBase(idade: 40, rendimento: 4000, prestacoes: 0, valor: 60000, prazo: 120,
            situacao: "ContratoPrazo", incidentes: true));

        Assert.Equal(EstadoPedidoEnum.Recusado, r.Decisao);
        Assert.Equal(new[] { "R3", "R4", "R7" }, Regras(r));
        Assert.Equal(12.50m, r.Indicadores!.TaxaEsforco);
    }
}
