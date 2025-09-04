using MindShelf_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MindShelf_BL.Dtos.EventDtos;

namespace MindShelf_BL.Dtos.EventDtos
{
    public class EventDetailsDto
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartingDate { get; set; }
        public DateTime EndingDate { get; set; }
        public string Location { get; set; }
        public bool IsOnline { get; set; }
        public bool IsActive { get; set; }

        // Include registrations if needed
        public List<EventRegistrationResponseDto> Registrations { get; set; }
    }
}
