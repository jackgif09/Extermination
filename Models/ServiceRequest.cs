using System.ComponentModel.DataAnnotations;

namespace Extermination.Models;

public enum PestType
{
    Ants,
    Bed_Bugs,
    Cockroaches,
    Fleas,
    Mosquitoes,
    Rodents,
    Spiders,
    Termites,
    Wasps,
    Other
}

public enum ServiceStatus
{
    New,
    Scheduled,
    Completed
}

public class ServiceRequest
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required, MaxLength(200), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    [Required]
    public PestType PestType { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime? PreferredDate { get; set; }

    public ServiceStatus Status { get; set; } = ServiceStatus.New;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
