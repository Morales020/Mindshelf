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
			var userId = Context?.UserIdentifier;
			var userEntity = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
			// Prefer UserName as display label; if it's an email, use local-part
			var baseName = userEntity?.UserName
						   ?? Context?.User?.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value
						   ?? Context?.User?.Identity?.Name
						   ?? "Anonymous";
			var displayName = baseName.Contains('@') ? baseName.Split('@')[0] : baseName;

			// Avatar: DB.ProfileImageUrl -> Google picture claim
			var avatar = userEntity?.ProfileImageUrl;
			if (string.IsNullOrWhiteSpace(avatar))
			{
				avatar = Context?.User?.Claims.FirstOrDefault(c => c.Type == "urn:google:picture")?.Value
					  ?? Context?.User?.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;
			}

			var msg = new Message
			{
				SenderId = userId,
				SenderName = displayName,
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

			await Clients.All.SendAsync("ReceiveMessage", msg.SenderId, msg.SenderName, msg.Content, msg.SentAt, avatar);
		}
	}
}


