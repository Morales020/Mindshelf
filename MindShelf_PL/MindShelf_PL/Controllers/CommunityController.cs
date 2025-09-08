using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindShelf_DAL.Data;
using System.Linq;

namespace MindShelf_PL.Controllers
{
	[Authorize]
	public class CommunityController : Controller
	{
		private readonly MindShelfDbContext _dbContext;

		public CommunityController(MindShelfDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public IActionResult Index()
		{
			// Load latest 50, then order ascending so newest don't disappear on refresh
			var messages = _dbContext.Messages
				.OrderByDescending(m => m.SentAt)
				.Take(50)
				.OrderBy(m => m.SentAt)
				.ToList();

			// Pass avatar urls for initial render keyed by UserId
			ViewBag.Avatars = _dbContext.Users
				.Select(u => new { u.Id, u.ProfileImageUrl })
				.ToDictionary(u => u.Id, u => u.ProfileImageUrl ?? string.Empty);

			return View(messages);
		}
	}
}


