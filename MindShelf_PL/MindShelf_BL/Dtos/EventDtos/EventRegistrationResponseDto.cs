namespace MindShelf_BL.Dtos.EventDtos;

public class EventRegistrationResponseDto
{
    public int EventRegistrationId { get; set; }
    public DateTime RegistrationDate { get; set; }
    public string UserId { get; set; }

    public string? Notes { get; set; }
    public string UserName { get; set; }
    public int EventId { get; set; }
    public string EventTitle { get; set; } 
}