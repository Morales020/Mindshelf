using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MindShelf_DAL.Data;
using MindShelf_DAL.Models;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace MindShelf_PL.Hubs
{
	[Authorize]
	public class CommunityHub : Hub
	{
		private readonly MindShelfDbContext _dbContext;

		public CommunityHub(MindShelfDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task SendMessage(string message)
		{
			var userName = Context?.User?.Identity?.Name ?? "Anonymous";
			var userId = Context?.UserIdentifier;
			var avatar = _dbContext.Users.FirstOrDefault(u => u.Id == userId)?.ProfileImageUrl;

			var msg = new Message
			{
				SenderId = userId,
				SenderName = userName,
				Content = message,
				SentAt = DateTime.UtcNow
			};

			_dbContext.Messages.Add(msg);
			await _dbContext.SaveChangesAsync();

			// Prune: keep only newest 50 messages
			var total = _dbContext.Messages.Count();
			if (total > 50)
			{
				var toRemove = _dbContext.Messages
					.OrderBy(m => m.SentAt)
					.Take(total - 50)
					.ToList();
				if (toRemove.Any())
				{
					_dbContext.Messages.RemoveRange(toRemove);
					await _dbContext.SaveChangesAsync();
				}
			}

			await Clients.All.SendAsync("ReceiveMessage", msg.SenderName, msg.Content, msg.SentAt, avatar);
		}
	}
}


