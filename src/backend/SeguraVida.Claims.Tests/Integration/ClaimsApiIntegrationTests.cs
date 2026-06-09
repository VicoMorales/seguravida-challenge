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
using SeguraVida.Claims.Application.Policies;
using SeguraVida.Claims.Domain.Claims;
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
                "POL-INT-001",
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
        detail.Policy.PolicyNumber.Should().Be("POL-INT-001");
        detail.Policy.Branch.Should().Be("AUTO");
        detail.InsuredParty.FullName.Should().Be("Integration Test User");
        detail.InsuredParty.MaskedDocumentId.Should().Be("DNI****01");
        detail.InsuredParty.MaskedEmail.Should().Be("i***@example.com");
        detail.History.Should().ContainSingle();
    }

    [Fact]
    public async Task Get_policy_by_number_returns_policy_and_masked_insured_party_data()
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("operator@seguravida.com"));
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginBody = await login.Content.ReadFromJsonAsync<LoginResponse>();
        loginBody.Should().NotBeNull();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBody!.AccessToken);

        var response = await client.GetAsync("/api/policies/POL-INT-001");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PolicyLookupDto>();
        body.Should().NotBeNull();
        body!.Policy.PolicyNumber.Should().Be("POL-INT-001");
        body.Policy.InsuredAmount.Should().Be(10000m);
        body.InsuredParty.FullName.Should().Be("Integration Test User");
        body.InsuredParty.MaskedDocumentId.Should().Be("DNI****01");
        body.InsuredParty.MaskedEmail.Should().Be("i***@example.com");
    }

    [Fact]
    public async Task Approve_claim_outside_review_returns_business_message()
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("adjuster@seguravida.com"));
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginBody = await login.Content.ReadFromJsonAsync<LoginResponse>();
        loginBody.Should().NotBeNull();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBody!.AccessToken);

        var response = await client.PostAsJsonAsync(
            $"/api/claims/{ClaimsApiFactory.ReportedClaimId}/approve",
            new ApproveClaimRequest(1000m, "Revision completa"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        body.Should().NotBeNull();
        body!.Message.Should().Be("La accion no es valida para el estado actual del siniestro.");
    }

    private sealed record CreateClaimResponse(Guid ClaimId);

    private sealed record ApiErrorResponse(string TraceId, int StatusCode, string Message, string[] Errors);
}

public sealed class ClaimsApiFactory : WebApplicationFactory<Program>
{
    public static readonly Guid PartyId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid PolicyId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid ReportedClaimId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
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
            var policy = new InsurancePolicy(
                PolicyId,
                "POL-INT-001",
                PartyId,
                PolicyBranch.Auto,
                1000m,
                10000m,
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 12, 31),
                PolicyStatus.Active);
            var reportedClaim = Claim.Report(
                policy,
                "CLM-INT-REPORTED",
                ClaimType.Accident,
                new DateOnly(2026, 6, 6),
                new DateOnly(2026, 6, 6),
                "Reported only claim",
                1000m,
                "operator@seguravida.com",
                new DateTimeOffset(2026, 6, 6, 10, 0, 0, TimeSpan.Zero));

            typeof(Claim)
                .GetProperty(nameof(Claim.Id))!
                .SetValue(reportedClaim, ReportedClaimId);

            dbContext.Policies.Add(policy);
            dbContext.Claims.Add(reportedClaim);
            dbContext.SaveChanges();
        });
    }
}

public sealed class TestMockUserRepository : IMockUserRepository
{
    public Task<MockUserDto?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        MockUserDto? user = email.ToLowerInvariant() switch
        {
            "operator@seguravida.com" => new MockUserDto(Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), "operator@seguravida.com", "OPERATOR", "Mock Operator"),
            "adjuster@seguravida.com" => new MockUserDto(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "adjuster@seguravida.com", "ADJUSTER", "Mock Adjuster"),
            _ => null
        };

        return Task.FromResult(user);
    }
}
