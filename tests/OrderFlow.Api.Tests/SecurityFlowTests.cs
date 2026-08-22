using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Api.Data;
using OrderFlow.Api.Dtos;

namespace OrderFlow.Api.Tests;

[Collection("Integration")]
public class SecurityFlowTests :
    IClassFixture<CustomWebApplicationFactory>,
    IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SecurityFlowTests(
        CustomWebApplicationFactory factory)
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
    public async Task Customer_CannotCreateProduct()
    {
        var email = "customer@example.com";
        var password = "password123";

        await RegisterUser(email, password);

        var token = await Login(email, password);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var request = new CreateProductRequest
        {
            Name = "Keyboard",
            Price = 4999,
            StockQuantity = 10
        };

        var response = await _client.PostAsJsonAsync(
            "/api/products",
            request);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task Admin_CanCreateProduct()
    {
        var email = "admin@example.com";
        var password = "password123";

        await RegisterUser(email, password);

        await MakeUserAdmin(email);

        var token = await Login(email, password);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var request = new CreateProductRequest
        {
            Name = "Mechanical Keyboard",
            Price = 4999,
            StockQuantity = 10
        };

        var response = await _client.PostAsJsonAsync(
            "/api/products",
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);
    }

    private async Task RegisterUser(
        string email,
        string password)
    {
        var request = new RegisterRequest
        {
            Email = email,
            Password = password
        };

        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);
    }

    private async Task<string> Login(
        string email,
        string password)
    {
        var request = new LoginRequest
        {
            Email = email,
            Password = password
        };

        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var loginResponse =
            await response.Content
                .ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(loginResponse);
        Assert.False(
            string.IsNullOrWhiteSpace(loginResponse.Token));

        return loginResponse.Token;
    }

    private async Task MakeUserAdmin(string email)
    {
        using var scope =
            _factory.Services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<OrderFlowDbContext>();

        var user = await dbContext.Users
            .SingleAsync(u => u.Email == email);

        user.Role = "Admin";

        await dbContext.SaveChangesAsync();
    }
}