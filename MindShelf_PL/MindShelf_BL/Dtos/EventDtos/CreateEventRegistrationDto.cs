using System.ComponentModel.DataAnnotations;

namespace MindShelf_BL.Dtos.EventDtos;

public class CreateEventRegistrationDto
{
    [Required]
    public int EventId { get; set; }

    [Required]
    [StringLength(100)]
    public string UserName { get; set; }

    [Required]
    public string UserId { get; set; }

    public string? Notes { get; set; }
}