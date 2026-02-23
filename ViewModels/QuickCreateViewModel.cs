using System.ComponentModel.DataAnnotations;
using Extermination.Models;

namespace Extermination.ViewModels;

public class QuickCreateViewModel
{
    [Required, MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(200), EmailAddress]
    public string? Email { get; set; }

    [Required, MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    [Required]
    public PestType PestType { get; set; }

    public DateTime? ScheduledFor { get; set; }

    public decimal? Price { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }
}
