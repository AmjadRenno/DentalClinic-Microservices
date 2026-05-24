using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GatewayService.IntegrationTests;

public class SecurityHeadersTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SecurityHeadersTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Get_AnyEndpoint_ShouldIncludeXContentTypeOptionsHeader()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.GetValues("X-Content-Type-Options").First().Should().Be("nosniff");
    }

    [Fact]
    public async Task Get_AnyEndpoint_ShouldIncludeXFrameOptionsHeader()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.GetValues("X-Frame-Options").First().Should().Be("DENY");
    }

    [Fact]
    public async Task Get_AnyEndpoint_ShouldIncludeXXSSProtectionHeader()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.Headers.Should().ContainKey("X-XSS-Protection");
        response.Headers.GetValues("X-XSS-Protection").First().Should().Be("1; mode=block");
    }

    [Fact]
    public async Task Get_AnyEndpoint_ShouldIncludeContentSecurityPolicyHeader()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.Headers.Should().ContainKey("Content-Security-Policy");
        var csp = response.Headers.GetValues("Content-Security-Policy").First();
        
        csp.Should().Contain("default-src 'self'");
        csp.Should().Contain("script-src 'self'");
        csp.Should().Contain("frame-ancestors 'none'");
    }

    [Fact]
    public async Task Get_AnyEndpoint_ShouldIncludeReferrerPolicyHeader()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.Headers.Should().ContainKey("Referrer-Policy");
        response.Headers.GetValues("Referrer-Policy").First()
            .Should().Be("strict-origin-when-cross-origin");
    }

    [Fact]
    public async Task Get_AnyEndpoint_ShouldIncludePermissionsPolicyHeader()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.Headers.Should().ContainKey("Permissions-Policy");
        var permissionsPolicy = response.Headers.GetValues("Permissions-Policy").First();
        
        permissionsPolicy.Should().Contain("geolocation=()");
        permissionsPolicy.Should().Contain("microphone=()");
        permissionsPolicy.Should().Contain("camera=()");
    }

    [Fact]
    public async Task Get_LocalhostRequest_ShouldNotIncludeHSTSHeader()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert - HSTS should not be present for localhost
        response.Headers.Should().NotContainKey("Strict-Transport-Security");
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/health")]
    [InlineData("/api/nonexistent")]
    public async Task Get_DifferentEndpoints_ShouldAllIncludeSecurityHeaders(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert - All endpoints should have security headers
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.Should().ContainKey("X-XSS-Protection");
        response.Headers.Should().ContainKey("Content-Security-Policy");
        response.Headers.Should().ContainKey("Referrer-Policy");
        response.Headers.Should().ContainKey("Permissions-Policy");
    }

    [Fact]
    public async Task Post_Request_ShouldIncludeSecurityHeaders()
    {
        // Act
        var response = await _client.PostAsync("/", null);

        // Assert
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.Should().ContainKey("Content-Security-Policy");
    }

    [Fact]
    public async Task Put_Request_ShouldIncludeSecurityHeaders()
    {
        // Act
        var response = await _client.PutAsync("/", null);

        // Assert
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.Should().ContainKey("Content-Security-Policy");
    }

    [Fact]
    public async Task Delete_Request_ShouldIncludeSecurityHeaders()
    {
        // Act
        var response = await _client.DeleteAsync("/");

        // Assert
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.Should().ContainKey("Content-Security-Policy");
    }

    [Fact]
    public async Task Options_Request_ShouldIncludeSecurityHeaders()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.Should().ContainKey("X-Frame-Options");
    }

    [Fact]
    public async Task Get_AnyEndpoint_SecurityHeadersShouldPreventCommonAttacks()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert - Verify all critical security headers are present
        var headers = response.Headers;

        // Anti-MIME-Sniffing
        headers.GetValues("X-Content-Type-Options").First().Should().Be("nosniff",
            "prevents browsers from MIME-sniffing away from the declared Content-Type");

        // Anti-Clickjacking
        headers.GetValues("X-Frame-Options").First().Should().Be("DENY",
            "prevents the page from being embedded in frames, protecting against clickjacking");

        // XSS Protection (legacy but good for older browsers)
        headers.GetValues("X-XSS-Protection").First().Should().Be("1; mode=block",
            "enables browser's XSS filter in block mode");

        // Content Security Policy - prevents XSS and injection attacks
        var csp = headers.GetValues("Content-Security-Policy").First();
        csp.Should().Contain("default-src 'self'",
            "restricts loading resources to same origin by default");
        csp.Should().Contain("frame-ancestors 'none'",
            "prevents embedding in frames (modern alternative to X-Frame-Options)");

        // Referrer Policy - prevents information leakage
        headers.GetValues("Referrer-Policy").First().Should().Be("strict-origin-when-cross-origin",
            "controls referrer information sent with requests");

        // Permissions Policy - disables unnecessary browser features
        var permissionsPolicy = headers.GetValues("Permissions-Policy").First();
        permissionsPolicy.Should().Contain("camera=()",
            "disables camera access");
        permissionsPolicy.Should().Contain("microphone=()",
            "disables microphone access");
        permissionsPolicy.Should().Contain("geolocation=()",
            "disables geolocation");
    }
}
