using Microsoft.AspNetCore.Mvc;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private static readonly List<Product> Products =
    [
        new Product
        {
            Id = 1,
            Name = "Mechanical Keyboard",
            Price = 4999.00m,
            StockQuantity = 10
        },
        new Product
        {
            Id = 2,
            Name = "Wireless Mouse",
            Price = 1999.00m,
            StockQuantity = 25
        }
    ];

    [HttpGet]
    public ActionResult<List<Product>> GetProducts()
    {
        return Ok(Products);
    }
    [HttpGet("{id:int}")]
    public ActionResult<Product> GetProductById(int id)
    {
        var product = Products.FirstOrDefault(p => p.Id == id);

        if (product is null)
        {
            return NotFound(new
            {
                message = "Product not found"
            });
        }

        return Ok(product);
    }
}