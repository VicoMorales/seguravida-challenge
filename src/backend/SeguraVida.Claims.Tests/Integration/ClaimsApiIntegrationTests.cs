using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SeguraVida.Claims.Api.Contracts.Auth;
using SeguraVida.Claims.Api.Contracts.Claims;
using SeguraVida.Claims.Application.Auth;
using SeguraVida.Claims.Application.Claims;
using SeguraVida.Claims.Domain.Parties;
using SeguraVida.Claims.Domain.Policies;
using SeguraVida.Claims.Infrastructure.Persistence;

namespace SeguraVida.Claims.Tests.Integration;

public sealed class ClaimsApiIntegrationTests : IClassFixture<ClaimsApiFactory>
{
    private readonly ClaimsApiFactory _factory;

    public ClaimsApiIntegrationTests(ClaimsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_claim_and_get_detail_returns_status_history()
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("operator@seguravida.com"));
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginBody = await login.Content.ReadFromJsonAsync<LoginResponse>();
        loginBody.Should().NotBeNull();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBody!.AccessToken);

        var createResponse = await client.PostAsJsonAsync(
            "/api/claims",
            new CreateClaimRequest(
                ClaimsApiFactory.PolicyId,
                "ACCIDENT",
                new DateOnly(2026, 6, 5),
                new DateOnly(2026, 6, 5),
                1200m,
                "Minor bumper collision"));

        var createError = await createResponse.Content.ReadAsStringAsync();
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created, createError);
        var createBody = await createResponse.Content.ReadFromJsonAsync<CreateClaimResponse>();
        createBody.Should().NotBeNull();

        var detail = await client.GetFromJsonAsync<ClaimDetailDto>($"/api/claims/{createBody!.ClaimId}");

        detail.Should().NotBeNull();
        detail!.PolicyId.Should().Be(ClaimsApiFactory.PolicyId);
        detail.Status.Should().Be("REPORTED");
        detail.History.Should().ContainSingle();
    }

    private sealed record CreateClaimResponse(Guid ClaimId);
}

public sealed class ClaimsApiFactory : WebApplicationFactory<Program>
{
    public static readonly Guid PartyId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid PolicyId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private readonly InMemoryDatabaseRoot _databaseRoot = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ClaimsDbContext>>();
            services.RemoveAll<IMockUserRepository>();
            services.AddDbContext<ClaimsDbContext>(options => options.UseInMemoryDatabase("claims-api", _databaseRoot));
            services.AddSingleton<IMockUserRepository, TestMockUserRepository>();

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
            dbContext.Database.EnsureCreated();
            dbContext.InsuredParties.Add(new InsuredParty(
                PartyId,
                "DNI900001",
                "Integration Test User",
                new DateOnly(1990, 1, 1),
                "integration@example.com"));
            dbContext.Policies.Add(new InsurancePolicy(
                PolicyId,
                "POL-INT-001",
                PartyId,
                PolicyBranch.Auto,
                1000m,
                10000m,
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 12, 31),
                PolicyStatus.Active));
            dbContext.SaveChanges();
        });
    }
}

public sealed class TestMockUserRepository : IMockUserRepository
{
    public Task<MockUserDto?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        MockUserDto? user = email.Equals("operator@seguravida.com", StringComparison.OrdinalIgnoreCase)
            ? new MockUserDto(Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), "operator@seguravida.com", "OPERATOR", "Mock Operator")
            : null;

        return Task.FromResult(user);
    }
}
