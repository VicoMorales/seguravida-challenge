using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SeguraVida.Claims.Application.Abstractions;
using SeguraVida.Claims.Application.Auth;
using SeguraVida.Claims.Application.Reports;
using SeguraVida.Claims.Infrastructure.Auth;
using SeguraVida.Claims.Infrastructure.Persistence;
using SeguraVida.Claims.Infrastructure.Persistence.Repositories;

namespace SeguraVida.Claims.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ClaimsDatabase")
            ?? "Server=localhost,1433;Database=SeguraVidaClaims;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True;Encrypt=False";

        services.AddDbContext<ClaimsDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IClaimRepository, ClaimRepository>();
        services.AddScoped<IClaimReadRepository, ClaimReadRepository>();
        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddScoped<IPolicyReadRepository, PolicyReadRepository>();
        services.AddScoped<IClaimsReportRepository, ClaimsReportRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.Configure<JwtOptions>(options =>
        {
            var section = configuration.GetSection("Jwt");
            options.Issuer = section["Issuer"] ?? options.Issuer;
            options.Audience = section["Audience"] ?? options.Audience;
            options.Secret = section["Secret"] ?? options.Secret;
            options.ExpirationMinutes = int.TryParse(section["ExpirationMinutes"], out var minutes)
                ? minutes
                : options.ExpirationMinutes;
        });
        services.AddScoped<IAuthTokenService, JwtTokenService>();
        services.AddScoped<IMockUserRepository, MockUserRepository>();

        return services;
    }
}
