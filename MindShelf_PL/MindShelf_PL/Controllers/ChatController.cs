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

			ViewBag.OtherUserId = other.Id;
			ViewBag.OtherDisplayName = other.UserName;
			ViewBag.OtherAvatar = other.ProfileImageUrl;

			// My display name and avatar
			ViewBag.MyDisplayName = me.UserName;
			ViewBag.MyAvatar = me.ProfileImageUrl;

			var history = await _dbContext.PrivateMessages
				.Where(m => (m.SenderId == me.Id && m.ReceiverId == other.Id) || (m.SenderId == other.Id && m.ReceiverId == me.Id))
				.OrderBy(m => m.SentAt)
				.Take(200)
				.ToListAsync();

			return View(history);
		}
	}
}


