using System.ComponentModel.DataAnnotations;

namespace Extermination.ViewModels;

public class LoginViewModel
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
