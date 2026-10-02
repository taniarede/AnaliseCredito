using System.Text.Json.Serialization;
using AnaliseCredito.Api.Data;
using AnaliseCredito.Api.Interfaces;
using AnaliseCredito.Api.Regras;
using AnaliseCredito.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("AnaliseCredito")
    ?? throw new InvalidOperationException("Falta a connection string 'AnaliseCredito' em appsettings.json.");

// Parâmetros das regras (appsettings.json -> ParametrosCredito)
var parametros = builder.Configuration.GetSection(ParametrosCredito.Seccao).Get<ParametrosCredito>()
    ?? new ParametrosCredito();

// ---------- Injeção de dependências ----------
// Regras (sem estado -> uma única instância para toda a aplicação)
builder.Services.AddSingleton(parametros);
builder.Services.AddSingleton<PrioridadeDecisao>();
builder.Services.AddSingleton(TimeProvider.System);

// Base de dados
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));

// Services (os controllers dependem das interfaces, não das classes)
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IAvaliacaoService, AvaliacaoService>();
builder.Services.AddScoped<IPedidosService, PedidosService>();
builder.Services.AddScoped<IAnaliseManualService, AnaliseManualService>();
builder.Services.AddScoped<ISimulacoesService, SimulacoesService>();

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// O frontend (Vite) corre em http://localhost:5173
builder.Services.AddCors(o => o.AddPolicy("Frontend", p => p
    .WithOrigins("http://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

// Cria a base de dados no primeiro arranque (se ainda não existir)
if (app.Configuration.GetValue("BaseDados:InicializarAutomaticamente", true))
{
    await DatabaseInitializer.InicializarAsync(
        connectionString,
        app.Configuration.GetValue("BaseDados:CarregarDadosExemplo", true),
        app.Logger);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Frontend");
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();

// Permite que testes de integração futuros referenciem a aplicação.
public partial class Program { }
