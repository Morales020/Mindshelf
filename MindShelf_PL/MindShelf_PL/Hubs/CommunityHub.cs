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

		public override async Task OnConnectedAsync()
		{
			var userId = Context?.UserIdentifier;
			System.Diagnostics.Debug.WriteLine($"User {userId} connected to CommunityHub");
			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			var userId = Context?.UserIdentifier;
			System.Diagnostics.Debug.WriteLine($"User {userId} disconnected from CommunityHub: {exception?.Message}");
			await base.OnDisconnectedAsync(exception);
		}

		public async Task SendMessage(string message)
		{
			try
			{
				var userId = Context?.UserIdentifier;
				System.Diagnostics.Debug.WriteLine($"SendMessage called by user: {userId}, message: {message}");
				
				if (string.IsNullOrEmpty(userId))
				{
					System.Diagnostics.Debug.WriteLine("UserIdentifier is null or empty");
					return;
				}
				
				var userEntity = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
			// Prefer display_name claim → then UserName → then identity name; if email, use local-part
			var claimDisplay = Context?.User?.Claims.FirstOrDefault(c => c.Type == "display_name")?.Value;
			var baseName = claimDisplay
					   ?? userEntity?.UserName
					   ?? Context?.User?.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value
					   ?? Context?.User?.Identity?.Name
					   ?? "Anonymous";
			var displayName = baseName.Contains('@') ? baseName.Split('@')[0] : baseName;

			// Avatar: Check privacy setting first, then DB.ProfileImageUrl -> Google picture claim
			var shareAvatarClaim = Context?.User?.Claims.FirstOrDefault(c => c.Type == "share_avatar")?.Value;
			var shareAvatar = shareAvatarClaim == null ? true : shareAvatarClaim == "true";
			
			var avatar = shareAvatar ? userEntity?.ProfileImageUrl : null;
			if (shareAvatar && string.IsNullOrWhiteSpace(avatar))
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
			System.Diagnostics.Debug.WriteLine($"Message broadcasted successfully");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error in SendMessage: {ex.Message}");
				throw;
			}
		}
	}
}


