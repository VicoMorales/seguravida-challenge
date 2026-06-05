using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SeguraVida.Claims.Api.Middleware;
using SeguraVida.Claims.Application;
using SeguraVida.Claims.Infrastructure;
using SeguraVida.Claims.Infrastructure.Auth;
using SeguraVida.Claims.Infrastructure.Persistence;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Grafana.Loki;

var builder = WebApplication.CreateBuilder(args);
const string CorsPolicyName = "SeguraVidaFrontend";

builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    var lokiUrl = context.Configuration["Loki:Url"];
    loggerConfiguration
        .Enrich.FromLogContext()
        .WriteTo.Console(new RenderedCompactJsonFormatter());

    if (!string.IsNullOrWhiteSpace(lokiUrl))
    {
        loggerConfiguration.WriteTo.GrafanaLoki(
            lokiUrl,
            labels:
            [
                new LokiLabel { Key = "app", Value = "seguravida-claims-api" },
                new LokiLabel { Key = "service", Value = "api" }
            ],
            textFormatter: new RenderedCompactJsonFormatter());
    }
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()
            ?? ["http://localhost:4200", "http://127.0.0.1:4200"];

        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SeguraVida Claims API",
        Version = "v1",
        Description = "JWT roles: OPERATOR creates/list/detail, ADJUSTER reviews/approves/rejects/pays, AUDITOR reads details/history/reports."
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Use: Bearer {token}"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            []
        }
    });
});
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:ApplyMigrations"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();

    if (dbContext.Database.IsRelational())
    {
        dbContext.Database.Migrate();
    }
}

// Configure the HTTP request pipeline.
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseSerilogRequestLogging();

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}
app.UseCors(CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithName("Health")
    .WithOpenApi();
app.MapControllers();

app.Run();

public partial class Program
{
}
