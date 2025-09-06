using Microsoft.AspNetCore.Mvc;
using MindShelf_BL.Dtos.AuthorDto;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_BL.Helper;
using Microsoft.AspNetCore.Authorization;

namespace MindShelf_MVC.Controllers
{
    public class AuthorController : Controller
    {
        private readonly IAuthorServies _authorService;

        public AuthorController(IAuthorServies authorService)
        {
            _authorService = authorService;
        }

        // GET: /Author
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var result = await _authorService.GetAllAuthor(page, pageSize);
            if (result.StatusCode != 200)
                return NotFound(result.Message);

            return View(result.Data);
        }

        // GET: /Author/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var result = await _authorService.GetAuthorById(id);
            if (result.StatusCode != 200)
                return NotFound(result.Message);

            return View(result.Data);
        }

        // GET: /Author/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Author/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> Create(AuthorCreateDto authorDto)
        {
            if (!ModelState.IsValid)
                return View(authorDto);

            var result = await _authorService.CreateAuthor(authorDto);
            if (result.StatusCode != 201)
                return BadRequest(result.Message);

            return RedirectToAction(nameof(Index));
        }

        // GET: /Author/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _authorService.GetAuthorById(id);
            if (result.StatusCode != 200)
                return NotFound(result.Message);

            var author = result.Data;
            var updateDto = new AuthorUpdateDto
            {
                Name = author.Name,
                Bio = author.Bio,
                DateOfBirth = author.DateOfBirth,
                ImageUrl = author.ImageUrl
            };

            return View(updateDto);
        }

        // POST: /Author/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, AuthorUpdateDto authorDto)
        {
            if (!ModelState.IsValid)
                return View(authorDto);

            var result = await _authorService.UpdateAuthor(id, authorDto);
            if (result.StatusCode != 200)
                return BadRequest(result.Message);

            return RedirectToAction(nameof(Index));
        }

        // GET: /Author/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _authorService.GetAuthorById(id);
            if (result.StatusCode != 200)
                return NotFound(result.Message);

            return View(result.Data);
        }

        // POST: /Author/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _authorService.DeleteAuthor(id);
            if (result.StatusCode != 200)
                return BadRequest(result.Message);

            return RedirectToAction(nameof(Index));
        }
    }
}