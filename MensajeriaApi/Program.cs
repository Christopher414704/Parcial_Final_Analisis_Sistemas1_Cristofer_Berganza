using MensajeriaApi.Data;
using MensajeriaApi.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Controladores API
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "API Mensajería y Paquetería",
        Description = "API REST para registrar envíos, calcular tarifas, rastrear paquetes, cambiar estados y registrar historial."
    });
});

// Servicios propios
builder.Services.AddSingleton<SqlConnectionFactory>();
builder.Services.AddScoped<IEnvioService, EnvioService>();

var app = builder.Build();

// Swagger habilitado
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Mensajería y Paquetería v1");
    options.RoutePrefix = "swagger";
});

// Redirigir la raíz hacia Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseHttpsRedirection();

app.MapControllers();

app.Run();