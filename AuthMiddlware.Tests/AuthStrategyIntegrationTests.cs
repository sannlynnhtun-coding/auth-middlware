using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace AuthMiddlware.Tests;

public class AuthStrategyIntegrationTests
{
    [Fact]
    public async Task Cookie_ProtectedWithoutCookie_RedirectsToSignIn()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var response = await client.GetAsync("/CookieTest/Protected");

        AssertRedirectToSignIn(response);
    }

    [Fact]
    public async Task Cookie_SetupThenProtected_ReturnsOk()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var setup = await client.GetAsync("/CookieTest/SetupCookie");
        var protectedResponse = await client.GetAsync("/CookieTest/Protected");

        Assert.Equal(HttpStatusCode.OK, setup.StatusCode);
        Assert.Equal(HttpStatusCode.OK, protectedResponse.StatusCode);
    }

    [Fact]
    public async Task Session_ProtectedWithoutSession_RedirectsToSignIn()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var response = await client.GetAsync("/SessionTest/Protected");

        AssertRedirectToSignIn(response);
    }

    [Fact]
    public async Task Session_SetupThenProtected_ReturnsOk()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var setup = await client.GetAsync("/SessionTest/SetupSession");
        var protectedResponse = await client.GetAsync("/SessionTest/Protected");

        Assert.Equal(HttpStatusCode.OK, setup.StatusCode);
        Assert.Equal(HttpStatusCode.OK, protectedResponse.StatusCode);
    }

    [Fact]
    public async Task Jwt_ProtectedWithoutToken_RedirectsToSignIn()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var response = await client.GetAsync("/JwtTest/Protected");

        AssertRedirectToSignIn(response);
    }

    [Fact]
    public async Task Jwt_SetupThenProtected_ReturnsOkAndRefreshesTokenCookie()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var setup = await client.GetAsync("/JwtTest/SetupJwt");
        var protectedResponse = await client.GetAsync("/JwtTest/Protected");

        Assert.Equal(HttpStatusCode.OK, setup.StatusCode);
        Assert.Equal(HttpStatusCode.OK, protectedResponse.StatusCode);

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

    private static void AssertRedirectToSignIn(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var location = response.Headers.Location!.OriginalString;
        Assert.True(
            location.Equals("/", StringComparison.OrdinalIgnoreCase) ||
            location.EndsWith("/SignIn/Index", StringComparison.OrdinalIgnoreCase),
            $"Expected redirect to '/' or '/SignIn/Index' but got '{location}'.");
    }
}
