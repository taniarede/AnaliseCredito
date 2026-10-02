using AnaliseCredito.Api.Regras;

namespace AnaliseCredito.Tests;

/// <summary>Testes da validação do NIF (9 dígitos + dígito de controlo módulo 11).</summary>
public class ValidadorNifTests
{
    [Theory]
    [InlineData("123456789")]
    [InlineData("245678905")]
    [InlineData("500123454")]
    [InlineData("198765436")]
    public void DigitoControlo_Valido(string nif) =>
        Assert.True(ValidadorNif.DigitoControloValido(nif));

    [Theory]
    [InlineData("123456788")]
    [InlineData("245678900")]
    public void DigitoControlo_Invalido(string nif) =>
        Assert.False(ValidadorNif.DigitoControloValido(nif));

    [Theory]
    [InlineData("123456789", true)]
    [InlineData("12345678", false)]
    [InlineData("1234567890", false)]
    [InlineData("12345678A", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void TemNoveDigitos(string? nif, bool esperado) =>
        Assert.Equal(esperado, ValidadorNif.TemNoveDigitos(nif));
}
