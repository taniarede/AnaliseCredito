using AnaliseCredito.Api.Enums;
using AnaliseCredito.Api.Models;

namespace AnaliseCredito.Api.Regras;

// Regra 1 - Validação inicial.
// É diferente das outras: é uma "barreira". Se falhar, o pedido é PEDIDO INVÁLIDO e as
// restantes regras não são avaliadas (ex.: com prazo 0 a Regra 6 dividiria por zero).
// Recolhe TODOS os erros de uma vez e indica o campo de cada um,
// para o frontend assinalar a vermelho os campos a corrigir.
public static class Regra01ValidacaoInicial
{
    public const string Codigo = "R1";

    // Nomes dos campos iguais no frontend! (JSON)
    public const string CampoNif = "nif";
    public const string CampoIdade = "idade";
    public const string CampoRendimento = "rendimentoMensalLiquido";
    public const string CampoPrestacoes = "prestacoesAtuais";
    public const string CampoValor = "valorPretendido";
    public const string CampoPrazo = "prazoMeses";
    public const string CampoSituacao = "situacaoProfissional";

    public static IReadOnlyList<MotivoAvaliacao> Validar(PedidoCreditoInput pedido, ParametrosCredito parametros)
    {
        var erros = new List<MotivoAvaliacao>();
        void Erro(string campo, string texto) =>
            erros.Add(new MotivoAvaliacao(Codigo, EstadoPedidoEnum.PedidoInvalido, texto, campo));

        // --- Regras do enunciado ---
        var nif = pedido.Nif?.Trim();
        if (string.IsNullOrEmpty(nif))
            Erro(CampoNif, "NIF é obrigatório.");
        else if (!ValidadorNif.TemNoveDigitos(nif))
            Erro(CampoNif, "NIF deve ter exatamente 9 dígitos.");
        else if (parametros.ValidarDigitoControloNif && !ValidadorNif.DigitoControloValido(nif))
            Erro(CampoNif, "NIF com dígito de controlo inválido.");

        if (pedido.Idade is null)
            Erro(CampoIdade, "Idade é obrigatória.");
        else if (pedido.Idade < parametros.IdadeMinima)
            Erro(CampoIdade, $"Idade deve ser igual ou superior a {parametros.IdadeMinima} anos.");

        if (pedido.RendimentoMensalLiquido is null)
            Erro(CampoRendimento, "Rendimento mensal líquido é obrigatório.");
        else if (pedido.RendimentoMensalLiquido <= 0)
            Erro(CampoRendimento, "Rendimento mensal líquido deve ser superior a 0.");

        // Pressuposto documentado: as prestações têm de ser preenchidas (0 é válido, vazio não),
        // para o utilizador confirmar que o cliente não tem outros créditos.
        if (pedido.PrestacoesAtuais is null)
            Erro(CampoPrestacoes, "Prestações atuais são obrigatórias (indique 0 se não existirem).");
        else if (pedido.PrestacoesAtuais < 0)
            Erro(CampoPrestacoes, "Prestações atuais não podem ser negativas.");

        if (pedido.ValorPretendido is null)
            Erro(CampoValor, "Valor pretendido é obrigatório.");
        else if (pedido.ValorPretendido <= 0)
            Erro(CampoValor, "Valor pretendido deve ser superior a 0.");

        if (pedido.PrazoMeses is null)
            Erro(CampoPrazo, "Prazo é obrigatório.");
        else if (pedido.PrazoMeses <= 0)
            Erro(CampoPrazo, "Prazo deve ser superior a 0 meses.");

        // Só são aceites as três situações do enunciado.
        if (!SituacaoProfissionalEnumParser.TryParse(pedido.SituacaoProfissional, out _))
            Erro(CampoSituacao, "Situação profissional inválida (valores aceites: Efetivo, ContratoPrazo, Desempregado).");

        return erros;
    }
}
