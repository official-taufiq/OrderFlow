using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Api.Dtos;

public class UpdateOrderStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
}