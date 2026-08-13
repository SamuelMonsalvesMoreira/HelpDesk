using System.Text;
using System.Text.Json.Serialization;
using HelpDesk.Api.Authentication;
using HelpDesk.Api.Data;
using HelpDesk.Api.Models;
using HelpDesk.Api.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var jwtSettings = builder.Configuration
    .GetSection(JwtSettings.SectionName)
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException("As configurações JWT não foram definidas.");

if (Encoding.UTF8.GetByteCount(jwtSettings.Key) < 32)
{
    throw new InvalidOperationException("A chave JWT deve possuir pelo menos 32 bytes.");
}

if (!builder.Environment.IsDevelopment()
    && jwtSettings.Key.StartsWith("CHANGE_THIS_KEY", StringComparison.Ordinal))
{
    throw new InvalidOperationException(
        "Defina uma chave JWT segura por variável de ambiente antes de executar em produção.");
}

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services
    .AddAuthentication("Bearer")
    .AddScheme<AuthenticationSchemeOptions, JwtAuthenticationHandler>("Bearer", _ => { });
builder.Services.AddAuthorization();

var storageProvider = builder.Configuration["StorageProvider"] ?? "Json";
var useSqlServer = storageProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase);

if (storageProvider.Equals("Json", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<ITicketRepository, JsonTicketRepository>();
    builder.Services.AddSingleton<IUserRepository, JsonUserRepository>();
}
else if (useSqlServer)
{
    var connectionString = builder.Configuration.GetConnectionString("SqlServer")
        ?? throw new InvalidOperationException("A conexão 'SqlServer' não foi configurada.");

    builder.Services.AddDbContext<HelpDeskDbContext>(options =>
        options.UseSqlServer(connectionString));
    builder.Services.AddScoped<ITicketRepository, EfTicketRepository>();
    builder.Services.AddScoped<IUserRepository, EfUserRepository>();
}
else
{
    throw new InvalidOperationException(
        $"O armazenamento '{storageProvider}' não é suportado. Use 'Json' ou 'SqlServer'.");
}

builder.Services.AddScoped<DemoDataSeeder>();

var app = builder.Build();

if (useSqlServer)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<HelpDeskDbContext>();
    await dbContext.Database.MigrateAsync();
}

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
    await seeder.SeedAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .AllowAnonymous();

app.Run();

