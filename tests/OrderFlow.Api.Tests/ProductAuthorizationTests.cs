using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using OrderFlow.Api.Dtos;

namespace OrderFlow.Api.Tests;

[Collection("Integration")]
public class ProductAuthorizationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductAuthorizationTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateProduct_WithoutAuthentication_ReturnsUnauthorized()
    {
        var request = new CreateProductRequest
        {
            Name = "Test Product",
            Price = 100,
            StockQuantity = 5
        };

        var response = await _client.PostAsJsonAsync(
            "/api/products",
            request
        );

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode
        );
    }
}