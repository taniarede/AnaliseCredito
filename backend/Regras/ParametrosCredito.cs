namespace AnaliseCredito.Api.Regras;

// Limites das regras de negócio. Os valores vêm de appsettings.json (secção "ParametrosCredito"),
// para que a área de Risco possa alterar um limite sem alterar código.
// Os valores por omissão são os do enunciado.
public sealed class ParametrosCredito
{
    public const string Seccao = "ParametrosCredito";

    //Versão das regras gravada em cada pedido (auditoria)
    public string VersaoRegras { get; set; } = "1.0";

    //Regra 1 - idade mínima para o pedido ser analisado.
    public int IdadeMinima { get; set; } = 18;

    // Regra 2 - idade máxima no final do contrato.
    public decimal IdadeMaximaFimContrato { get; set; } = 75m;

    // Regra 5 - o valor pretendido não pode exceder N vezes o rendimento.
    public decimal MultiploMaximoRendimento { get; set; } = 20m;

    //Regra 6 - acima deste valor (%) vai para Análise Manual. O limite é inclusivo: 35,00% mantém a decisão.
    public decimal TaxaEsforcoLimiteAnaliseManual { get; set; } = 35m;

    //Regra 6 - acima deste valor (%) é Recusado. O limite é inclusivo: 50,00% é Análise Manual.
    public decimal TaxaEsforcoLimiteRecusa { get; set; } = 50m;

    //Regra 7 - montante a partir do qual (exclusive) o pedido vai sempre para Análise Manual.
    public decimal MontanteElevado { get; set; } = 50_000m;

    //Regra 1 - para além dos 9 dígitos, valida também o dígito de controlo do NIF (módulo 11).
    public bool ValidarDigitoControloNif { get; set; } = true;
}
