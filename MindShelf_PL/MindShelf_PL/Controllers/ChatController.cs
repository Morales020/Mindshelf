using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindShelf_DAL.Data;
using MindShelf_DAL.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MindShelf_PL.Controllers
{
	[Authorize]
	public class ChatController : Controller
	{
		private readonly MindShelfDbContext _dbContext;
		private readonly UserManager<User> _userManager;

		public class ConversationVm
		{
			public string OtherUserId { get; set; } = string.Empty;
			public DateTime LastMessageAt { get; set; }
			public int UnreadCount { get; set; }
			public string? DisplayName { get; set; }
			public string? AvatarUrl { get; set; }
		}

		public ChatController(MindShelfDbContext dbContext, UserManager<User> userManager)
		{
			_dbContext = dbContext;
			_userManager = userManager;
		}

		[HttpGet]
		public async Task<IActionResult> UnreadTotal()
		{
			var me = await _userManager.GetUserAsync(User);
			if (me == null) return Unauthorized();
			var count = _dbContext.PrivateMessages.Count(m => m.ReceiverId == me.Id && !m.IsRead);
			return Json(count);
		}

		[HttpGet]
		public async Task<IActionResult> Conversations()
		{
			var me = await _userManager.GetUserAsync(User);
			if (me == null) return RedirectToAction("Login", "Account");

			// Build conversation summaries: per other user, latest message time, unread count
			var query = _dbContext.PrivateMessages
				.Where(m => m.SenderId == me.Id || m.ReceiverId == me.Id)
				.Select(m => new {
					OtherId = m.SenderId == me.Id ? m.ReceiverId : m.SenderId,
					Message = m,
				});

			var grouped = query
				.AsEnumerable()
				.GroupBy(x => x.OtherId)
				.Select(g => new ConversationVm
				{
					OtherUserId = g.Key,
					LastMessageAt = g.Max(x => x.Message.SentAt),
					UnreadCount = g.Count(x => x.Message.ReceiverId == me.Id && !x.Message.IsRead)
				})
				.OrderByDescending(c => c.LastMessageAt)
				.ToList();

			// Load user names/avatars
			var otherIds = grouped.Select(c => c.OtherUserId).ToList();
			var users = _dbContext.Users.Where(u => otherIds.Contains(u.Id)).ToList();
			foreach (var c in grouped)
			{
				var u = users.FirstOrDefault(u => u.Id == c.OtherUserId);
				c.DisplayName = u?.UserName ?? c.OtherUserId;
				if (!string.IsNullOrWhiteSpace(c.DisplayName) && c.DisplayName.Contains('@'))
					c.DisplayName = c.DisplayName.Split('@')[0];
				c.AvatarUrl = u?.ProfileImageUrl;
			}

			return View(grouped);
		}

		public async Task<IActionResult> Private(string userId)
		{
			var me = await _userManager.GetUserAsync(User);
			if (me == null) return RedirectToAction("Login", "Account");
			if (string.IsNullOrWhiteSpace(userId) || userId == me.Id) return RedirectToAction("Index", "Community");

			var other = await _userManager.FindByIdAsync(userId);
			if (other == null) return NotFound();

			// Build other user's display name and avatar (handle Google sign-in)
			var otherClaims = await _userManager.GetClaimsAsync(other);
			var otherDisplay = otherClaims.FirstOrDefault(c => c.Type == "display_name")?.Value
						   ?? other.UserName
						   ?? other.Email
						   ?? other.Id;
			if (!string.IsNullOrWhiteSpace(otherDisplay) && otherDisplay.Contains('@'))
				otherDisplay = otherDisplay.Split('@')[0];
			var otherAvatar = other.ProfileImageUrl;
			if (string.IsNullOrWhiteSpace(otherAvatar))
			{
				otherAvatar = otherClaims.FirstOrDefault(c => c.Type == "urn:google:picture")?.Value
						   ?? otherClaims.FirstOrDefault(c => c.Type == "picture")?.Value;
			}
			ViewBag.OtherUserId = other.Id;
			ViewBag.OtherDisplayName = otherDisplay;
			ViewBag.OtherAvatar = otherAvatar;

			// My display name and avatar
			var myClaims = await _userManager.GetClaimsAsync(me);
			var myDisplay = myClaims.FirstOrDefault(c => c.Type == "display_name")?.Value
						  ?? me.UserName
						  ?? me.Email
						  ?? me.Id;
			if (!string.IsNullOrWhiteSpace(myDisplay) && myDisplay.Contains('@'))
				myDisplay = myDisplay.Split('@')[0];
			var myAvatar = me.ProfileImageUrl;
			if (string.IsNullOrWhiteSpace(myAvatar))
			{
				myAvatar = myClaims.FirstOrDefault(c => c.Type == "urn:google:picture")?.Value
						 ?? myClaims.FirstOrDefault(c => c.Type == "picture")?.Value;
			}
			ViewBag.MyDisplayName = myDisplay;
			ViewBag.MyAvatar = myAvatar;

			var history = await _dbContext.PrivateMessages
				.Where(m => (m.SenderId == me.Id && m.ReceiverId == other.Id) || (m.SenderId == other.Id && m.ReceiverId == me.Id))
				.OrderBy(m => m.SentAt)
				.Take(200)
				.ToListAsync();

			// Mark partner's unread messages as read now
			var unread = _dbContext.PrivateMessages
				.Where(m => m.SenderId == other.Id && m.ReceiverId == me.Id && !m.IsRead)
				.ToList();
			if (unread.Any())
			{
				foreach (var m in unread) m.IsRead = true;
				await _dbContext.SaveChangesAsync();
			}

			return View(history);
		}
	}
}


