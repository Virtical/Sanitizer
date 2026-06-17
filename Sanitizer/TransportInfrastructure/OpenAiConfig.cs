using System.ComponentModel.DataAnnotations;

namespace Sanitizer.TransportInfrastructure;

public record OpenAiConfig
{
    public string? Address { get; set; }
    
    public string Model { get; set; } = "gpt-4o";
    
    [Required(ErrorMessage = "ApiKey is required for OpenAi")]
    public required string ApiKey { get; set; }
}