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

			// Notify both participants in real-time. Use userId targeting.
			await Clients.User(receiverUserId).SendAsync("ReceivePrivateMessage", senderId, message, entity.SentAt);
			await Clients.User(senderId).SendAsync("ReceivePrivateMessageEcho", receiverUserId, message, entity.SentAt);
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


