using System.Net;
using System.Text.Json;
using AwesomeAssertions;

namespace CleanApi.Api.IntegrationTests;

/// <summary>
/// Pipeline-level integration tests that don't need a database. They validate that the real host
/// wires up auth and OpenAPI correctly. For DB-backed flows see <see cref="ProductsEndpointTests"/>
/// (Testcontainers, skipped when Docker is unavailable).
/// </summary>
public sealed class ApiPipelineTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory = factory;

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/products");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task OpenApiDocument_IsServed_WithBearerScheme()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var schemes = document.RootElement
            .GetProperty("components")
            .GetProperty("securitySchemes");

        schemes.TryGetProperty("Bearer", out _).Should().BeTrue();
    }
}
