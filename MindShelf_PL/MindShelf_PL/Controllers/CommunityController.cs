using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MindShelf_DAL.Data;
using System.Linq;

namespace MindShelf_PL.Controllers
{
	[Authorize]
	public class CommunityController : Controller
	{
		private readonly MindShelfDbContext _dbContext;
		private readonly UserManager<MindShelf_DAL.Models.User> _userManager;

		public CommunityController(MindShelfDbContext dbContext, UserManager<MindShelf_DAL.Models.User> userManager)
		{
			_dbContext = dbContext;
			_userManager = userManager;
		}

		public async Task<IActionResult> Index()
		{
			// Load latest 50, then order ascending so newest don't disappear on refresh
			var messages = _dbContext.Messages
				.OrderByDescending(m => m.SentAt)
				.Take(50)
				.OrderBy(m => m.SentAt)
				.ToList();

			// Pass avatar urls for initial render keyed by UserId, respecting privacy settings
			var usersWithAvatars = _dbContext.Users.ToList();
			var avatarDict = new Dictionary<string, string>();
			
			foreach (var user in usersWithAvatars)
			{
				var claims = await _userManager.GetClaimsAsync(user);
				var shareAvatarClaim = claims.FirstOrDefault(c => c.Type == "share_avatar")?.Value;
				var shareAvatar = shareAvatarClaim == null ? true : shareAvatarClaim == "true";
				
				if (shareAvatar && !string.IsNullOrWhiteSpace(user.ProfileImageUrl))
				{
					avatarDict[user.Id] = user.ProfileImageUrl;
				}
			}
			
			ViewBag.Avatars = avatarDict;

			// Pass current user's display name for the view
			var currentUser = await _userManager.GetUserAsync(User);
			if (currentUser != null)
			{
				var claims = await _userManager.GetClaimsAsync(currentUser);
				var displayName = claims.FirstOrDefault(c => c.Type == "display_name")?.Value;
				ViewBag.MyDisplayName = displayName ?? currentUser.UserName;
				// Debug: Log what we're loading
				System.Diagnostics.Debug.WriteLine($"Community loading display_name claim: {displayName}, fallback: {currentUser.UserName}");
			}

			return View(messages);
		}
	}
}


