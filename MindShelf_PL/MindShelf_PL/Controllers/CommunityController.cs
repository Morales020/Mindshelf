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

			return View(messages);
		}
	}
}


