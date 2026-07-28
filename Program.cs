using System.Text;
using FitwomanAPI.Data;
using FitwomanAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Registrar DbContext para PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Registrar Servicios personalizados
builder.Services.AddScoped<IJwtService, JwtService>();

// 3. Configurar Autenticación con JWT Bearer
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] 
    ?? throw new InvalidOperationException("Jwt:SecretKey no está configurada.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Cambiar a true en producción con HTTPS obligatorio
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "FitwomanAPI",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "FitwomanApps",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.Zero // Elimina el tiempo de tolerancia por defecto (5 min)
    };

    // Extraer Token desde Cookie si no viene en el Header Authorization
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.TryGetValue("access_token", out var token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

// 4. Configurar política de CORS para portales (Admin y Cliente)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontends", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173", // Vite Admin por defecto
                "http://localhost:5174", // Vite Cliente por defecto
                "http://localhost:3000", // Next.js por defecto
                "http://localhost:4200"  // Angular por defecto
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Importante para enviar/recibir Cookies HttpOnly
    });
});

// 5. Configurar Enrutamiento en minúsculas (Estándar REST)
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// 6. Habilitar soporte para Controladores
builder.Services.AddControllers();

// 6. Configuración de Swagger / OpenAPI con soporte para JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fitwoman API", Version = "v1" });

    // Agregar definición de seguridad JWT en Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Ingrese el token JWT en el formato: Bearer {tu_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// 7. Configurar el pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Habilitar CORS antes de Autenticación
app.UseCors("AllowFrontends");

// Habilitar Autenticación y Autorización
app.UseAuthentication();
app.UseAuthorization();

// 8. Mapear rutas de los Controladores
app.MapControllers();

app.Run();
