using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace AnaliseCredito.Api.Data;

// Cria a base de dados no primeiro arranque, executando os scripts da pasta /database.
// (Só actua se a base de dados "AnaliseCredito" ainda não existir)
// Pode ser desligado em appsettings.json (BaseDados:InicializarAutomaticamente = false),
// por exemplo se quisermos correr os scripts à mão no VS Code ou no SSMS.
public static partial class DatabaseInitializer
{
    private static readonly string[] ScriptsBase = ["01-create-database.sql", "02-create-tables.sql"];
    private const string ScriptDadosExemplo = "03-seed-data.sql";

    public static async Task InicializarAsync(string connectionString, bool carregarDadosExemplo, ILogger logger)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var nomeBaseDados = builder.InitialCatalog;
        builder.InitialCatalog = "master";

        await using var ligacao = new SqlConnection(builder.ConnectionString);

        // O SQL Server em Docker demora alguns segundos a arrancar: tentamos várias vezes.
        const int maxTentativas = 10;
        for (var tentativa = 1; ; tentativa++)
        {
            try
            {
                await ligacao.OpenAsync();
                break;
            }
            catch (SqlException ex) when (tentativa < maxTentativas)
            {
                logger.LogWarning("SQL Server ainda não disponível (tentativa {T}/{Max}): {Erro}",
                    tentativa, maxTentativas, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
            catch (SqlException ex)
            {
                logger.LogError(ex,
                    "Não foi possível ligar ao SQL Server. Confirme que o SQL Server está a correr " +
                    "e que a connection string em appsettings.json está correta.");
                throw;
            }
        }

        await using (var verificar = new SqlCommand("SELECT DB_ID(@nome)", ligacao))
        {
            verificar.Parameters.AddWithValue("@nome", nomeBaseDados);
            var id = await verificar.ExecuteScalarAsync();
            if (id is not null && id is not DBNull)
            {
                logger.LogInformation("Base de dados {Nome} já existe. Nada a fazer.", nomeBaseDados);
                return;
            }
        }

        logger.LogInformation("A criar a base de dados {Nome}...", nomeBaseDados);

        string[] scripts = carregarDadosExemplo ? [.. ScriptsBase, ScriptDadosExemplo] : ScriptsBase;
        var pasta = Path.Combine(AppContext.BaseDirectory, "Database");

        foreach (var nomeScript in scripts)
        {
            var sql = await File.ReadAllTextAsync(Path.Combine(pasta, nomeScript));
            foreach (var batch in DividirEmBatches(sql))
            {
                await using var comando = new SqlCommand(batch, ligacao) { CommandTimeout = 120 };
                await comando.ExecuteNonQueryAsync();
            }
            logger.LogInformation("Script {Script} executado.", nomeScript);
        }

        logger.LogInformation("Base de dados {Nome} criada com sucesso.", nomeBaseDados);
    }

    /// <summary>"GO" não é SQL: é um separador usado pelas ferramentas. Aqui dividimos o script por ele.</summary>
    private static IEnumerable<string> DividirEmBatches(string sql) =>
        SeparadorGo().Split(sql).Where(b => !string.IsNullOrWhiteSpace(b));

    [GeneratedRegex(@"^\s*GO\s*;?\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex SeparadorGo();
}
