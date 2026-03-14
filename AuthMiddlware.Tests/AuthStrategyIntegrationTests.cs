using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;
using Xunit;

namespace AuthMiddlware.Tests;

public class AuthStrategyIntegrationTests
{
    [Fact]
    public async Task Cookie_ProtectedWithoutCookie_ReturnsUnauthorizedJson()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var response = await client.GetAsync("/CookieTest/Protected");
        var payload = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(payload.Success);
        Assert.Equal("cookie", payload.Filter);
        Assert.Equal(401, payload.StatusCode);
    }

    [Fact]
    public async Task Cookie_SetupThenProtected_ReturnsOkJson()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var setup = await client.GetAsync("/CookieTest/SetupCookie");
        var setupPayload = await ReadJsonAsync(setup);
        var protectedResponse = await client.GetAsync("/CookieTest/Protected");
        var protectedPayload = await ReadJsonAsync(protectedResponse);

        Assert.Equal(HttpStatusCode.OK, setup.StatusCode);
        Assert.True(setupPayload.Success);
        Assert.Equal("cookie", setupPayload.Filter);

        Assert.Equal(HttpStatusCode.OK, protectedResponse.StatusCode);
        Assert.True(protectedPayload.Success);
        Assert.Equal("cookie", protectedPayload.Filter);
    }

    [Fact]
    public async Task Session_ProtectedWithoutSession_ReturnsUnauthorizedJson()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var response = await client.GetAsync("/SessionTest/Protected");
        var payload = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(payload.Success);
        Assert.Equal("session", payload.Filter);
        Assert.Equal(401, payload.StatusCode);
    }

    [Fact]
    public async Task Session_SetupThenProtected_ReturnsOkJson()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var setup = await client.GetAsync("/SessionTest/SetupSession");
        var setupPayload = await ReadJsonAsync(setup);
        var protectedResponse = await client.GetAsync("/SessionTest/Protected");
        var protectedPayload = await ReadJsonAsync(protectedResponse);

        Assert.Equal(HttpStatusCode.OK, setup.StatusCode);
        Assert.True(setupPayload.Success);
        Assert.Equal("session", setupPayload.Filter);

        Assert.Equal(HttpStatusCode.OK, protectedResponse.StatusCode);
        Assert.True(protectedPayload.Success);
        Assert.Equal("session", protectedPayload.Filter);
    }

    [Fact]
    public async Task Jwt_ProtectedWithoutToken_ReturnsUnauthorizedJson()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var response = await client.GetAsync("/JwtTest/Protected");
        var payload = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(payload.Success);
        Assert.Equal("jwt", payload.Filter);
        Assert.Equal(401, payload.StatusCode);
    }

    [Fact]
    public async Task Jwt_SetupThenProtected_ReturnsOkJsonAndRefreshesTokenCookie()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var setup = await client.GetAsync("/JwtTest/SetupJwt");
        var setupPayload = await ReadJsonAsync(setup);
        var protectedResponse = await client.GetAsync("/JwtTest/Protected");
        var protectedPayload = await ReadJsonAsync(protectedResponse);

        Assert.Equal(HttpStatusCode.OK, setup.StatusCode);
        Assert.True(setupPayload.Success);
        Assert.Equal("jwt", setupPayload.Filter);

        Assert.Equal(HttpStatusCode.OK, protectedResponse.StatusCode);
        Assert.True(protectedPayload.Success);
        Assert.Equal("jwt", protectedPayload.Filter);

        var hasJwtSetCookie = protectedResponse.Headers.TryGetValues("Set-Cookie", out var setCookies)
            && setCookies.Any(x => x.Contains("jwt_token=", StringComparison.OrdinalIgnoreCase));

        Assert.True(hasJwtSetCookie);
    }

    [Fact]
    public async Task StaticFile_IsServed()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var response = await client.GetAsync("/css/site.css");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IDataProtectionProvider>(_ => new EphemeralDataProtectionProvider());
            });
        });
    }

    private static HttpClient CreateClient(WebApplicationFactory<Program> factory)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
            BaseAddress = new Uri("https://localhost")
        });
    }

    private static async Task<FilterResponse> ReadJsonAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var payload = JsonSerializer.Deserialize<FilterResponse>(json, options);
        Assert.NotNull(payload);
        return payload!;
    }

    private sealed class FilterResponse
    {
        public bool Success { get; set; }
        public string Filter { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? StatusCode { get; set; }
        public DateTime TimestampUtc { get; set; }
    }
}


