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
			var messages = _dbContext.Messages
				.OrderBy(m => m.SentAt)
				.Take(20)
                .ToList();

			// Pass avatar urls for initial render (tolerate nulls)
			ViewBag.Avatars = _dbContext.Users
				.Select(u => new { u.UserName, u.ProfileImageUrl })
				.ToDictionary(u => u.UserName, u => u.ProfileImageUrl ?? string.Empty);

			return View(messages);
		}
	}
}


