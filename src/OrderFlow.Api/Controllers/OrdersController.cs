using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Data;
using OrderFlow.Api.Dtos;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]

public class OrderControllers : ControllerBase
{
    private readonly OrderFlowDbContext _dbContext;

    public OrderControllers(OrderFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder(
        CreateOrderRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var order = new Order
            {
                UserId = userId,
                Status = "Pending"
            };

            decimal totalAmount = 0;

            foreach (var requestedItem in request.Items)
            {
                var product = await _dbContext.Products
                    .FirstOrDefaultAsync(
                        p => p.Id == requestedItem.ProductId);

                if (product is null)
                {
                    return BadRequest(new
                    {
                        message =
                            $"Product {requestedItem.ProductId} does not exist"
                    });
                }

                if (product.StockQuantity < requestedItem.Quantity)
                {
                    return BadRequest(new
                    {
                        message =
                            $"Insufficient stock for product {product.Name}"
                    });
                }

                product.StockQuantity -= requestedItem.Quantity;

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = requestedItem.Quantity,
                    UnitPrice = product.Price
                };

                order.Items.Add(orderItem);

                totalAmount += product.Price * requestedItem.Quantity;
            }

            order.TotalAmount = totalAmount;

            _dbContext.Orders.Add(order);

            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            var response = new OrderResponse
            {
                Id = order.Id,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(item => new OrderItemResponse
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList()
            };

            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = order.Id },
                response
            );
        }
        catch
        {
            await transaction.RollbackAsync();

            throw;
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetOrderById(int id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var order = await _dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(
                o => o.Id == id && o.UserId == userId);

        if (order is null)
        {
            return NotFound(new
            {
                message = "Order not found"
            });
        }

        var response = new OrderResponse
        {
            Id = order.Id,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(item => new OrderItemResponse
            {
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };

        return Ok(response);
    }
}
