using System.Net;
using System.Net.Http.Json;
using OrderFlow.Api.Dtos;

namespace OrderFlow.Api.Tests;

[Collection("Integration")]
public class AuthTests :
    IClassFixture<CustomWebApplicationFactory>,
    IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Register_WithValidRequest_CreatesUser()
    {
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);
    }
    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var request = new RegisterRequest
        {
            Email = "duplicate@example.com",
            Password = "password123"
        };

        var firstResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        Assert.Equal(
            HttpStatusCode.Created,
            firstResponse.StatusCode);

        var secondResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        Assert.Equal(
            HttpStatusCode.Conflict,
            secondResponse.StatusCode);
    }
}