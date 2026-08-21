using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.Data;

public class OrderFlowDbContext : DbContext
{
    public OrderFlowDbContext(DbContextOptions<OrderFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
}