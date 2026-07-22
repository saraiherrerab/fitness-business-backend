using Microsoft.EntityFrameworkCore;
using FitwomanAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Registrar DbContext para PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Habilitar soporte para Controladores
builder.Services.AddControllers();

// 3. Configuración de Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. Configurar el pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// 5. Mapear rutas de los Controladores
app.MapControllers();

app.Run();
