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

		public ChatController(MindShelfDbContext dbContext, UserManager<User> userManager)
		{
			_dbContext = dbContext;
			_userManager = userManager;
		}

		[HttpGet]
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

			return View(history);
		}
	}
}


