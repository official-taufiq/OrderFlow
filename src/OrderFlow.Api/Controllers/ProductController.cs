using Microsoft.AspNetCore.Mvc;
using OrderFlow.Api.Dtos;
using OrderFlow.Api.Models;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Data;

namespace OrderFlow.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly OrderFlowDbContext _dbContext;

    public ProductsController(OrderFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetProducts()
    {
        var products = await _dbContext.Products
            .AsNoTracking()
            .ToListAsync();

        return Ok(products);
    }
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetProductById(int id)
    {
        var product = await _dbContext.Products.FindAsync(id);

        if (product is null)
        {
            return NotFound(new
            {
                message = "Product not found"
            });
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(
    CreateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            StockQuantity = request.StockQuantity
        };

        _dbContext.Products.Add(product);

        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetProductById),
            new { id = product.Id },
            product
        );
    }

    [HttpPut("{id:int}")]

    public async Task<ActionResult<Product>> UpdateProduct(int id,
        UpdateProductRequest updateRequest
    )
    {
        var product = await _dbContext.Products.FindAsync(id);

        if (product is null)
        {
            return NotFound(
                new
                {
                    message = "Product not found"
                }
            );
        }
        product.Name = updateRequest.Name;
        product.Price = updateRequest.Price;
        product.StockQuantity = updateRequest.StockQuantity;

        await _dbContext.SaveChangesAsync();

        return NoContent();

    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _dbContext.Products.FindAsync(id);

        if (product is null)
        {
            return NotFound(new
            {
                message = "Product not found"
            });
        }

        _dbContext.Products.Remove(product);

        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}