using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Data;
using OrderFlow.Api.Dtos;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly OrderFlowDbContext _dbContext;

    public AuthController(OrderFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register(RegisterRequest request)
    {
        var emailExists = await _dbContext.Users
            .AnyAsync(u => u.Email == request.Email);

        if (emailExists)
        {
            return Conflict(new
            {
                message = "Email already registered"
            });
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            Role = "Customer"
        };

        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync();

        var response = new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };

        return CreatedAtAction(
            nameof(Register),
            response
        );
    }
}