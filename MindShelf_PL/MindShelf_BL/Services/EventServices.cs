using Microsoft.EntityFrameworkCore;
using MindShelf_BL.Dtos.EventDtos;
using MindShelf_BL.Helper;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_BL.UnitWork;
using MindShelf_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Services
{
    public class EventServices : IEventServices
    {
        private readonly UnitOfWork _unitofwork;
        public EventServices(UnitOfWork _unitofwork)
        {
            this._unitofwork = _unitofwork;
        }

        public async Task<ResponseMVC<CreateEventDto>> CreateEvent(CreateEventDto createDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createDto.Title))
                    return ResponseMVC<CreateEventDto>.ErrorResponse("Title is required", 400);

                var newEvent = new Event
                {
                    Title = createDto.Title.Trim(),
                    Description = createDto.Description,
                    StartingDate = createDto.StartingDate,
                    EndingDate = createDto.EndingDate,
                    Location = createDto.Location,
                    IsOnline = createDto.IsOnline,
                    IsActive = true
                };

                await _unitofwork.EventRepo.Add(newEvent);
                await _unitofwork.SaveChangesAsync();

                return ResponseMVC<CreateEventDto>.SuccessResponse(createDto, "Event created successfully", 201);
            }
            catch (Exception ex)
            {
                return ResponseMVC<CreateEventDto>.ErrorResponse(ex.Message, 500);
            }
        }

        public async Task<ResponseMVC<IEnumerable<EventResponseDto>>> GetAllEvents()
        {
            try
            {
                var events = await _unitofwork.EventRepo
                    .Query()
                    .Include(e => e.EventRegistrations)
                    .ToListAsync();

                var result = events.Select(e => new EventResponseDto
                {
                    EventId = e.EventId,
                    Title = e.Title,
                    Description = e.Description,
                    StartingDate = e.StartingDate,
                    EndingDate = e.EndingDate,
                    Location = e.Location,
                    IsOnline = e.IsOnline,
                    IsActive = e.IsActive,
                    Registrations = e.EventRegistrations.Select(r => new EventRegistrationResponseDto
                    {
                        EventRegistrationId = r.EventRegistrationId,
                        UserId = r.UserId,
                        UserName = r.UserName,
                        RegistrationDate = r.RegistrationDate,
                        Notes = r.Notes
                    }).ToList()
                }).ToList();

                return ResponseMVC<IEnumerable<EventResponseDto>>.SuccessResponse(result, "Events retrieved successfully", 200);
            }
            catch (Exception ex)
            {
                return ResponseMVC<IEnumerable<EventResponseDto>>.ErrorResponse(ex.Message, 500);
            }
        }

        public async Task<ResponseMVC<EventResponseDto>> GetEventById(int id)
        {
            try
            {
                var ev = await _unitofwork.EventRepo.GetById(id);

                if (ev == null)
                    return ResponseMVC<EventResponseDto>.ErrorResponse("Event not found", 404);

                var result = new EventResponseDto
                {
                    EventId = ev.EventId,
                    Title = ev.Title,
                    Description = ev.Description,
                    StartingDate = ev.StartingDate,
                    EndingDate = ev.EndingDate,
                    IsOnline = ev.IsOnline,
                    IsActive = ev.IsActive
                };

                return ResponseMVC<EventResponseDto>.SuccessResponse(result, "Event retrieved successfully", 200);
            }
            catch (Exception ex)
            {
                return ResponseMVC<EventResponseDto>.ErrorResponse(ex.Message, 500);
            }
        }

        public async Task<ResponseMVC<EventDetailsDto>> GetEventDetails(int id)
        {
            try
            {
                var ev = await _unitofwork.EventRepo
                    .Query()
                    .Include(e => e.EventRegistrations)
                    .ThenInclude(er => er.User)
                    .FirstOrDefaultAsync(e => e.EventId == id);

                if (ev == null)
                    return ResponseMVC<EventDetailsDto>.ErrorResponse("Event not found", 404);

                var result = new EventDetailsDto
                {
                    EventId = ev.EventId,
                    Title = ev.Title,
                    Description = ev.Description,
                    StartingDate = ev.StartingDate,
                    EndingDate = ev.EndingDate,
                    Location = ev.Location,
                    IsOnline = ev.IsOnline,
                    IsActive = ev.IsActive,
                    Registrations = ev.EventRegistrations.Select(r => new EventRegistrationResponseDto
                    {
                        EventRegistrationId = r.EventRegistrationId,
                        UserId = r.UserId,
                        UserName = r.UserName,
                        RegistrationDate = r.RegistrationDate,
                        Notes = r.Notes
                    }).ToList()
                };

                return ResponseMVC<EventDetailsDto>.SuccessResponse(result, "Event details retrieved successfully", 200);
            }
            catch (Exception ex)
            {
                return ResponseMVC<EventDetailsDto>.ErrorResponse(ex.Message, 500);
            }
        }

        public async Task<ResponseMVC<bool>> UpdateEventAsync(int id, UpdateEventDto updateDto)
        {
            try
            {
                var ev = await _unitofwork.EventRepo.GetById(id);

                if (ev == null)
                    return ResponseMVC<bool>.ErrorResponse("Event not found", 404);

                // Map new values
                ev.Title = updateDto.Title.Trim();
                ev.Description = updateDto.Description;
                ev.StartingDate = updateDto.StartingDate;
                ev.EndingDate = updateDto.EndingDate;
                ev.Location = updateDto.Location;
                ev.IsOnline = updateDto.IsOnline;
                ev.IsActive = updateDto.IsActive;

                _unitofwork.EventRepo.Update(ev);
                await _unitofwork.SaveChangesAsync();

                return ResponseMVC<bool>.SuccessResponse(true, "Event updated successfully", 200);
            }
            catch (Exception ex)
            {
                return ResponseMVC<bool>.ErrorResponse(ex.Message, 500);
            }
        }

        public async Task<ResponseMVC<bool>> DeleteEventAsync(int id)
        {
            try
            {
                var deleted = await _unitofwork.EventRepo.Delete(id);

                if (!deleted)
                    return ResponseMVC<bool>.ErrorResponse("Event not found", 404);

                await _unitofwork.SaveChangesAsync();

                return ResponseMVC<bool>.SuccessResponse(true, "Event deleted successfully", 200);
            }
            catch (Exception ex)
            {
                return ResponseMVC<bool>.ErrorResponse(ex.Message, 500);
            }
        }
    }
}
