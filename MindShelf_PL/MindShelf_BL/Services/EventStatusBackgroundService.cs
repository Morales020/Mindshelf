using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MindShelf_BL.UnitWork;
using MindShelf_DAL.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MindShelf_BL.Services
{
    public class EventStatusBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventStatusBackgroundService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromSeconds(10); // Check every 10 seconds for faster response

        public EventStatusBackgroundService(IServiceProvider serviceProvider, ILogger<EventStatusBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Event Status Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndCloseExpiredEvents();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking event status");
                }

                await Task.Delay(_period, stoppingToken);
            }

            _logger.LogInformation("Event Status Background Service stopped");
        }

        private async Task CheckAndCloseExpiredEvents()
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            try
            {
                var currentTime = DateTime.Now; // Use local time instead of UTC
                _logger.LogInformation($"Background service checking at: {currentTime:yyyy-MM-dd HH:mm:ss}");
                
                // Find active events that have passed their end time
                var expiredEvents = await unitOfWork.EventRepo
                    .Query()
                    .Where(e => e.IsActive == true && e.EndingDate <= currentTime)
                    .ToListAsync();

                if (expiredEvents.Any())
                {
                    foreach (var eventItem in expiredEvents)
                    {
                        eventItem.IsActive = false;
                        unitOfWork.EventRepo.Update(eventItem);
                        
                        _logger.LogInformation($"Event '{eventItem.Title}' has been automatically closed (ended at: {eventItem.EndingDate:yyyy-MM-dd HH:mm:ss} UTC)");
                        
                    }

                    await unitOfWork.SaveChangesAsync();
                    _logger.LogInformation($"Successfully closed {expiredEvents.Count} expired events");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating event status");
            }
        }
    }
}
