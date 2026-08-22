using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrderFlow.Api.Data;

namespace OrderFlow.Api.Tests;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private const string TestConnectionString =
        "Host=localhost;Port=5433;Database=orderflow_test;Username=orderflow;Password=orderflow";

    public CustomWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable(
            "Jwt__Key",
            "orderflow-integration-test-secret-key-123456789");

        Environment.SetEnvironmentVariable(
            "Jwt__Issuer",
            "OrderFlow");

        Environment.SetEnvironmentVariable(
            "Jwt__Audience",
            "OrderFlow");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<
                DbContextOptions<OrderFlowDbContext>>();

            services.RemoveAll<
                IDbContextOptionsConfiguration<OrderFlowDbContext>>();

            services.AddDbContext<OrderFlowDbContext>(options =>
                options.UseNpgsql(TestConnectionString));
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<OrderFlowDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();
    }
}