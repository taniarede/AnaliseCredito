namespace AnaliseCredito.Api.Enums;

public enum SituacaoProfissionalEnum
{
    Efetivo,
    ContratoPrazo,
    Desempregado
}

public static class SituacaoProfissionalEnumParser
{
    // Converte o texto recebido. Aceita apenas os três nomes do enunciado (sem distinguir maiúsculas).
    // Não utilizei Enum.TryParse porque este aceita números ("1") como válidos.
    public static bool TryParse(string? texto, out SituacaoProfissionalEnum situacao)
    {
        switch (texto?.Trim().ToLowerInvariant())
        {
            case "efetivo":
                situacao = SituacaoProfissionalEnum.Efetivo;
                return true;
            case "contratoprazo":
                situacao = SituacaoProfissionalEnum.ContratoPrazo;
                return true;
            case "desempregado":
                situacao = SituacaoProfissionalEnum.Desempregado;
                return true;
            default:
                situacao = default;
                return false;
        }
    }
}
