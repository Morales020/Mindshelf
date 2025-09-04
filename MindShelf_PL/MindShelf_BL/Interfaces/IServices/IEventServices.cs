using MindShelf_BL.Dtos.EventDtos;
using MindShelf_BL.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Interfaces.IServices
{
    public interface IEventServices
    {
        Task<ResponseMVC<IEnumerable<EventResponseDto>>> GetAllEvents();
        Task<ResponseMVC<EventResponseDto>> GetEventById(int id);
        Task<ResponseMVC<EventDetailsDto>> GetEventDetails(int id);

        Task<ResponseMVC<CreateEventDto>> CreateEvent(CreateEventDto createDto);
        Task<ResponseMVC<bool>> UpdateEventAsync(int id, UpdateEventDto updateDto);
        Task<ResponseMVC<bool>> DeleteEventAsync(int id);
    }
}
