namespace AnaliseCredito.Api.Regras;

/// Validação do NIF português.
///
/// Dígito de controlo (módulo 11):
///   1. Multiplicar os 8 primeiros dígitos por 9, 8, 7, 6, 5, 4, 3, 2 e somar.
///   2. Calcular o resto da divisão da soma por 11.
///   3. Se o resto for 0 ou 1, o dígito de controlo é 0; caso contrário é 11 - resto.
///   4. O 9.º dígito do NIF tem de ser igual a esse valor.
///
/// Exemplo: 123456789 -> 1×9 + 2×8 + 3×7 + 4×6 + 5×5 + 6×4 + 7×3 + 8×2 = 156;
///          156 mod 11 = 2; 11 - 2 = 9 -> o último dígito é 9, logo é válido.
public static class ValidadorNif
{
    public static bool TemNoveDigitos(string? nif) =>
        nif is { Length: 9 } && nif.All(char.IsAsciiDigit);

    /// <summary>Pressupõe que o NIF já tem 9 dígitos.</summary>
    public static bool DigitoControloValido(string nif)
    {
        var soma = 0;
        for (var i = 0; i < 8; i++)
        {
            soma += (nif[i] - '0') * (9 - i);
        }

        var resto = soma % 11;
        var esperado = resto < 2 ? 0 : 11 - resto;
        return nif[8] - '0' == esperado;
    }
}
