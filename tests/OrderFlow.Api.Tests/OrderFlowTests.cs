using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Api.Data;
using OrderFlow.Api.Dtos;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.Tests;

[Collection("Integration")]
public class OrderFlowTests :
    IClassFixture<CustomWebApplicationFactory>,
    IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public OrderFlowTests(
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
    public async Task CreateOrder_WithSufficientStock_CreatesOrderAndReducesStock()
    {
        const string email = "ordercustomer@example.com";
        const string password = "password123";

        // Register customer
        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                new RegisterRequest
                {
                    Email = email,
                    Password = password
                });

        Assert.Equal(
            HttpStatusCode.Created,
            registerResponse.StatusCode);

        // Login
        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                new LoginRequest
                {
                    Email = email,
                    Password = password
                });

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var login =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(login);

        // Insert product directly into test DB.
        int productId;

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<OrderFlowDbContext>();

            var product = new Product
            {
                Name = "Test Keyboard",
                Price = 5000,
                StockQuantity = 10
            };

            dbContext.Products.Add(product);

            await dbContext.SaveChangesAsync();

            productId = product.Id;
        }

        // Authenticate subsequent API request
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.Token);

        // Create order
        var orderResponse =
            await _client.PostAsJsonAsync(
                "/api/orders",
                new CreateOrderRequest
                {
                    Items =
                    [
                        new CreateOrderItemRequest
                        {
                            ProductId = productId,
                            Quantity = 2
                        }
                    ]
                });

        Assert.Equal(
            HttpStatusCode.Created,
            orderResponse.StatusCode);

        var order =
            await orderResponse.Content
                .ReadFromJsonAsync<OrderResponse>();

        Assert.NotNull(order);

        Assert.Equal(10000m, order.TotalAmount);

        Assert.Single(order.Items);

        Assert.Equal(
            2,
            order.Items[0].Quantity);

        Assert.Equal(
            5000m,
            order.Items[0].UnitPrice);

        // Verify actual database state
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<OrderFlowDbContext>();

            var product =
                await dbContext.Products
                    .AsNoTracking()
                    .SingleAsync(p => p.Id == productId);

            Assert.Equal(
                8,
                product.StockQuantity);

            var savedOrder =
                await dbContext.Orders
                    .Include(o => o.Items)
                    .SingleAsync(o => o.Id == order.Id);

            Assert.Equal(
                10000m,
                savedOrder.TotalAmount);

            Assert.Single(savedOrder.Items);

            Assert.Equal(
                2,
                savedOrder.Items[0].Quantity);
        }
    }
}