using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MindShelf_DAL.Data;
using MindShelf_DAL.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MindShelf_PL.Hubs
{
	[Authorize]
	public class PrivateChatHub : Hub
	{
		private readonly MindShelfDbContext _dbContext;

		public PrivateChatHub(MindShelfDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task SendPrivateMessage(string receiverUserId, string message)
		{
			var senderId = Context?.UserIdentifier;
			if (string.IsNullOrWhiteSpace(senderId) || string.IsNullOrWhiteSpace(receiverUserId) || string.IsNullOrWhiteSpace(message))
				return;

			var entity = new PrivateMessage
			{
				SenderId = senderId,
				ReceiverId = receiverUserId,
				Content = message,
				SentAt = DateTime.UtcNow,
				IsRead = false
			};
			_dbContext.PrivateMessages.Add(entity);
			await _dbContext.SaveChangesAsync();

			// Prepare sender display and avatar for notifications
			var userEntity = _dbContext.Users.FirstOrDefault(u => u.Id == senderId);
			var claimDisplay = Context?.User?.Claims.FirstOrDefault(c => c.Type == "display_name")?.Value
						   ?? userEntity?.UserName
						   ?? Context?.User?.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value
						   ?? "";
			var displayName = string.IsNullOrWhiteSpace(claimDisplay) ? "" : (claimDisplay.Contains('@') ? claimDisplay.Split('@')[0] : claimDisplay);

			var shareAvatarClaim = Context?.User?.Claims.FirstOrDefault(c => c.Type == "share_avatar")?.Value;
			var shareAvatar = shareAvatarClaim == null ? true : shareAvatarClaim == "true";
			var avatar = shareAvatar ? userEntity?.ProfileImageUrl : null;
			if (shareAvatar && string.IsNullOrWhiteSpace(avatar))
			{
				avatar = Context?.User?.Claims.FirstOrDefault(c => c.Type == "urn:google:picture")?.Value
					  ?? Context?.User?.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;
			}

			// Notify both participants in real-time using per-user groups for reliability
			var recvGroup = $"user:{receiverUserId}";
			var sendGroup = $"user:{senderId}";
			await Clients.Group(recvGroup).SendAsync("ReceivePrivateMessage", senderId, displayName, message, entity.SentAt, avatar);
			await Clients.Group(sendGroup).SendAsync("ReceivePrivateMessageEcho", receiverUserId, message, entity.SentAt);
		}

		public override async Task OnConnectedAsync()
		{
			var userId = Context?.UserIdentifier;
			if (!string.IsNullOrWhiteSpace(userId))
			{
				await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
			}
			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			var userId = Context?.UserIdentifier;
			if (!string.IsNullOrWhiteSpace(userId))
			{
				await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user:{userId}");
			}
			await base.OnDisconnectedAsync(exception);
		}

		public async Task MarkAsRead(int messageId)
		{
			var userId = Context?.UserIdentifier;
			var msg = _dbContext.PrivateMessages.FirstOrDefault(m => m.Id == messageId && m.ReceiverId == userId);
			if (msg == null) return;
			msg.IsRead = true;
			await _dbContext.SaveChangesAsync();
		}
	}
}


