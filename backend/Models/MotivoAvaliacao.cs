using AnaliseCredito.Api.Enums;

namespace AnaliseCredito.Api.Models;

// a regra, a decisão que ela pede, a explicação e (só na Regra 1) o campo com erro.
//
// Inclui também a formatação usada para escrever o texto dos motivos
// ("24.000,00 €", "82,87%"), porque só é usada para esse fim.
public sealed record MotivoAvaliacao(
    string CodigoRegra,
    EstadoPedidoEnum Decisao,
    string Descricao,
    string? Campo = null)
{
    private static readonly System.Globalization.NumberFormatInfo FormatoPt = new()
    {
        NumberDecimalSeparator = ",",
        NumberGroupSeparator = ".",
        NumberGroupSizes = [3]
    };

    //24000 -> "24.000,00 €" (formato fixo, independente das definições do computador)
    public static string Euros(decimal valor) => valor.ToString("#,##0.00", FormatoPt) + " €";

    //76 -> "76,00"
    public static string Numero(decimal valor) => valor.ToString("#,##0.00", FormatoPt);

    // 82.87 -> "82,87%"
    public static string Percentagem(decimal valor) => valor.ToString("#,##0.00", FormatoPt) + "%";
}
