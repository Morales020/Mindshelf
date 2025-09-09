using Microsoft.AspNetCore.SignalR;
using MindShelf_BL.Dtos.BookDto;

namespace MindShelf_PL.Hubs
{
    public class BookNotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context?.UserIdentifier;
            System.Diagnostics.Debug.WriteLine($"User {userId} connected to BookNotificationHub");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context?.UserIdentifier;
            System.Diagnostics.Debug.WriteLine($"User {userId} disconnected from BookNotificationHub: {exception?.Message}");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinBookNotifications()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "BookNotifications");
            System.Diagnostics.Debug.WriteLine($"User {Context.UserIdentifier} joined BookNotifications group");
        }

        public async Task LeaveBookNotifications()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "BookNotifications");
            System.Diagnostics.Debug.WriteLine($"User {Context.UserIdentifier} left BookNotifications group");
        }
    }
}
