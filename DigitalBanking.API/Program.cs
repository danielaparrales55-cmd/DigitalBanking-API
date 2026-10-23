using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURACIÓN DE SERVICIOS ---

// Swagger y OpenAPI (Tu configuración)
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen(); // Necesario para generar el swagger.json

// Configuración de SQLite[cite: 1]
builder.Services.AddDbContext<BancoDbContext>(options =>
    options.UseSqlite("Data Source=banco.db"));

// Inyección de dependencias[cite: 2]
builder.Services.AddScoped<IBancoService, BancoService>();

var app = builder.Build();

// Habilitar archivos estáticos para que Swagger cargue el custom.css desde wwwroot
app.UseStaticFiles();

// --- 2. CONFIGURACIÓN DEL PIPELINE HTTP ---

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Digital Banking API V1");
        c.RoutePrefix = string.Empty; // Carga en la ruta raíz http://localhost:XXXX
        c.InjectStylesheet("/swagger/custom.css"); // <-- LÍNEA AGREGADA PARA CARGAR TU CSS PERSONALIZADO
    });
}

// Comentado para evitar el warning 'Failed to determine the https port for redirect'

// Manejo Global de Excepciones[cite: 3]
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var Excepcion = exceptionHandlerPathFeature?.Error;

        if (Excepcion is ExcepcionNegocio ne)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { Error = ne.Message });
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { Error = "Error interno del servidor." });
        }
    });
});

// --- 3. ENDPOINTS ---

// Endpoints de la Prueba Técnica[cite: 4]
app.MapPost("/clientes", async (ClienteDto dto, IBancoService service) => 
    Results.Ok(await service.CrearClienteAsync(dto)));

app.MapPost("/cuentas", async (CuentaDto dto, IBancoService service) => 
    Results.Ok(await service.CrearCuentaAsync(dto.ClienteId, dto.SaldoInicial)));

app.MapGet("/cuentas/{numero}/saldo", async (string numero, IBancoService service) => 
    Results.Ok(new { Saldo = await service.ConsultarSaldoAsync(numero) }));

app.MapPost("/cuentas/{numero}/transaccion", async (string numero, TransaccionDto dto, IBancoService service) => 
{
    await service.RegistrarTransaccionAsync(numero, dto.Monto, dto.EsDeposito);
    return Results.Ok();
});

app.MapGet("/cuentas/{numero}/historial", async (string numero, IBancoService service) => 
    Results.Ok(await service.ObtenerHistorialAsync(numero)));

// Endpoint original WeatherForecast
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Inicializar Base de Datos SQLite
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BancoDbContext>();
    db.Database.EnsureCreated();
}

app.Run();


// LÓGICA DE NEGOCIO, MODELOS Y PERSISTENCIA

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public class ExcepcionNegocio : Exception { public ExcepcionNegocio(string msg) : base(msg) { } }

public interface IBancoService 
{
    Task<Cliente> CrearClienteAsync(ClienteDto dto);
    Task<Cuenta> CrearCuentaAsync(int clienteId, decimal saldoInicial);
    Task<decimal> ConsultarSaldoAsync(string numeroCuenta);
    Task RegistrarTransaccionAsync(string numeroCuenta, decimal monto, bool esDeposito);
    Task<List<Transaccion>> ObtenerHistorialAsync(string numeroCuenta);
}

public class BancoService : IBancoService
{
    private readonly BancoDbContext _db;
    public BancoService(BancoDbContext db) { _db = db; }

    public async Task<Cliente> CrearClienteAsync(ClienteDto dto)
    {
        var cliente = new Cliente { Nombre = dto.Nombre, FechaNacimiento = dto.FechaNacimiento, Genero = dto.Genero, IngresosMensuales = dto.IngresosMensuales };
        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();
        return cliente;
    }

    public async Task<Cuenta> CrearCuentaAsync(int clienteId, decimal saldoInicial)
    {
        string numeroCuenta = $"ACC-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}"; // Generador ACC-YYYYMMDD-[XXXX][cite: 5]
        var cuenta = new Cuenta { ClienteId = clienteId, NumeroCuenta = numeroCuenta, Saldo = saldoInicial };
        _db.Cuentas.Add(cuenta);
        await _db.SaveChangesAsync();
        return cuenta;
    }

    public async Task<decimal> ConsultarSaldoAsync(string numeroCuenta)
    {
        var cuenta = await _db.Cuentas.FirstOrDefaultAsync(c => c.NumeroCuenta == numeroCuenta) 
            ?? throw new ExcepcionNegocio ("Cuenta no encontrada.");
        return cuenta.Saldo;
    }

    public async Task RegistrarTransaccionAsync(string numeroCuenta, decimal monto, bool esDeposito)
    {
        var cuenta = await _db.Cuentas.FirstOrDefaultAsync(c => c.NumeroCuenta == numeroCuenta) 
            ?? throw new ExcepcionNegocio("Cuenta no encontrada.");

        if (!esDeposito && cuenta.Saldo < monto)
            throw new ExcepcionNegocio("Fondos insuficientes."); // Rechazo limpio[cite: 6]

        cuenta.Saldo += esDeposito ? monto : -monto;
        _db.Transacciones.Add(new Transaccion { CuentaId = cuenta.Id, Monto = monto, EsDeposito = esDeposito, SaldoResultante = cuenta.Saldo, Fecha = DateTime.UtcNow });
        await _db.SaveChangesAsync();
    }

    public async Task<List<Transaccion>> ObtenerHistorialAsync(string numeroCuenta)
    {
        var cuenta = await _db.Cuentas.Include(c => c.Transacciones).FirstOrDefaultAsync(c => c.NumeroCuenta == numeroCuenta) 
            ?? throw new ExcepcionNegocio("Cuenta no encontrada.");
        return cuenta.Transacciones.OrderBy(t => t.Fecha).ToList(); // Historial ordenado[cite: 7]
    }
}

// DTOs
public class ClienteDto { public string Nombre { get; set; } = string.Empty; public DateTime FechaNacimiento { get; set; } public string Genero { get; set; } = string.Empty; public decimal IngresosMensuales { get; set; } }
public class CuentaDto { public int ClienteId { get; set; } public decimal SaldoInicial { get; set; } }
public class TransaccionDto { public decimal Monto { get; set; } public bool EsDeposito { get; set; } }

// Entidades
public class Cliente { public int Id { get; set; } public string Nombre { get; set; } = string.Empty; public DateTime FechaNacimiento { get; set; } public string Genero { get; set; } = string.Empty; public decimal IngresosMensuales { get; set; } }
public class Cuenta { public int Id { get; set; } public int ClienteId { get; set; } public string NumeroCuenta { get; set; } = string.Empty; public decimal Saldo { get; set; } public List<Transaccion> Transacciones { get; set; } = new(); }
public class Transaccion { public int Id { get; set; } public int CuentaId { get; set; } public decimal Monto { get; set; } public bool EsDeposito { get; set; } public decimal SaldoResultante { get; set; } public DateTime Fecha { get; set; } }

// DbContext
public class BancoDbContext : DbContext
{
    public BancoDbContext(DbContextOptions<BancoDbContext> options) : base(options) { }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Cuenta> Cuentas { get; set; }
    public DbSet<Transaccion> Transacciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cuenta>().HasIndex(c => c.NumeroCuenta).IsUnique(); // Restricción única[cite: 8]
    }
}