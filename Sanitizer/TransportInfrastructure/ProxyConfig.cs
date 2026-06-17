using System.ComponentModel.DataAnnotations;

namespace Sanitizer.TransportInfrastructure;

public record ProxyConfig
{
    [Required(ErrorMessage = "Proxy.Address is required for OpenAi")]
    public string? Address { get; set; }
        
    [Required(ErrorMessage = "Proxy.Login is required for OpenAi")]
    public string? Login { get; set; }
        
    [Required(ErrorMessage = "Proxy.Password is required for OpenAi")]
    public string? Password { get; set; }
}