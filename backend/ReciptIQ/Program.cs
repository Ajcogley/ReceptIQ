using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ReciptIQ.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);

    // Solo en desarrollo: mostrar queries SQL sensibles
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                             ?? new[] { "http://localhost:5173" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});


var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key no configurado en appsettings.json");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero // Elimina el tiempo de gracia de 5 minutos
    };
});

builder.Services.AddAuthorization();


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configurar nombres de propiedades en camelCase
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;

        // Ignorar referencias circulares (importante para EF Core)
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

        // Permitir trailing commas en JSON
        options.JsonSerializerOptions.AllowTrailingCommas = true;
    });


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ReciptIQ API",
        Version = "v1",
        Description = "API para gestión inteligente de recibos con IA",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "ReciptIQ Team",
            Email = "admin@reciptiq.com"
        }
    });

    // Configurar autenticación JWT en Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// =====================================================
// 6. REGISTRAR SERVICIOS PERSONALIZADOS (próximos pasos)
// =====================================================
// builder.Services.AddScoped<IReceiptService, ReceiptService>();
// builder.Services.AddScoped<IAuthService, AuthService>();
// builder.Services.AddHttpClient(); // Para Claude API

// =====================================================
// 7. CONFIGURAR HTTPCONTEXT ACCESSOR (para obtener usuario actual)
// =====================================================
builder.Services.AddHttpContextAccessor();


var app = builder.Build();

// =====================================================
// MIDDLEWARE PIPELINE (orden importante)
// =====================================================


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ReciptIQ API v1");
        options.RoutePrefix = string.Empty; // Swagger en la raíz: http://localhost:5000
    });
}


app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowReactApp");


app.UseAuthentication();


app.UseAuthorization();


app.MapControllers();


app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
})).AllowAnonymous();


app.MapGet("/db-test", async (AppDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        var companiesCount = await db.Companies.CountAsync();

        return Results.Ok(new
        {
            connected = canConnect,
            companiesCount,
            message = "Conexión a PostgreSQL exitosa"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            title: "Error de conexión a base de datos"
        );
    }
}).AllowAnonymous();

app.Run();
