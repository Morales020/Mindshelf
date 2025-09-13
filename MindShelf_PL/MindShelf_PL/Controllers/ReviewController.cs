using Microsoft.AspNetCore.Mvc;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_BL.Dtos.ReviewDto;
using MindShelf_PL.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace MindShelf_PL.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly IReviewServices _reviewService;

        public ReviewController(IReviewServices reviewService)
        {
            _reviewService = reviewService;
        }

        private IActionResult ErrorResult(string message)
        {
            return View("Error", new ErrorViewModel
            {
                Message = message,
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        #region GetBookReviews
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetBookReviews(int bookId)
        {
            var request = new GetBookReviewsDto { BookId = bookId };
            var response = await _reviewService.GetBookReviews(request);
            
            if (response.StatusCode != 200 || response.Data == null)
                return ErrorResult(response.Message);

            return View("BookReviews", response.Data);
        }
        #endregion

        #region GetReviewById
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var response = await _reviewService.GetReviewById(id);
            
            if (response.StatusCode != 200 || response.Data == null)
                return ErrorResult(response.Message);

            return View(response.Data);
        }
        #endregion

        #region CreateReview
        [HttpGet]
        public IActionResult Create(int bookId)
        {
            var model = new CreateReviewDto
            {
                BookId = bookId
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Create(CreateReviewDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _reviewService.CreateReviewAsync(model);
            
            if (response.StatusCode != 201 || response.Data == null)
            {
                ModelState.AddModelError("", response.Message);
                return View(model);
            }

            TempData["SuccessMessage"] = "Review created successfully!";
            return RedirectToAction("GetBookReviews", new { bookId = model.BookId });
        }
        #endregion

        #region UpdateReview
        [Authorize(Roles = "User")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _reviewService.GetReviewById(id);
            
            if (response.StatusCode != 200 || response.Data == null)
                return ErrorResult(response.Message);

            if (response.Data.UserName != User.Identity?.Name)
                return Forbid();

            var model = new UpdateReviewDto
            {
                ReviewId = response.Data.ReviewId,
                BookId = response.Data.BookId,
                Rating = response.Data.Rating,
                Comment = response.Data.Comment
            };

            ViewBag.BookTitle = response.Data.BookName;
            return View(model);
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateReviewDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var review = await _reviewService.GetReviewById(model.ReviewId);
            if (review.Data == null || review.Data.UserName != User.Identity?.Name)
                return Forbid();

            var response = await _reviewService.UpdateReviewAsync(model);
            
            if (response.StatusCode != 200 || response.Data == null)
            {
                ModelState.AddModelError("", response.Message);
                return View(model);
            }

            TempData["SuccessMessage"] = "Review updated successfully!";
            return RedirectToAction("GetBookReviews", new { bookId = model.BookId });
        }
        #endregion

        #region DeleteReview
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _reviewService.GetReviewById(id);
            
            if (response.StatusCode != 200 || response.Data == null)
                return ErrorResult(response.Message);

            if (!User.IsInRole("Admin") && response.Data.UserName != User.Identity?.Name)
                return Forbid();

            return View(response.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int reviewId, string userName)
        {

            if (!User.IsInRole("Admin") && userName != User.Identity?.Name)
                return Forbid();

            var model = new DeleteReviewDto
            {
                ReviewId = reviewId,
                UserName = userName
            };

            var response = await _reviewService.DeleteReviewAsync(model);
            
            if (response.StatusCode != 200)
            {
                TempData["ErrorMessage"] = response.Message;
                return RedirectToAction("GetBookReviews", new { bookId = model.ReviewId });
            }

            TempData["SuccessMessage"] = "Review deleted successfully!";
            return RedirectToAction("Index", "Books");
        }
        #endregion

        #region UserReviews
        [HttpGet]
        public async Task<IActionResult> UserReviews(string userName)
        {
            // This would need to be implemented in the service
            // For now, redirect to books index
            TempData["InfoMessage"] = "User reviews feature coming soon!";
            return RedirectToAction("Index", "Books");
        }
        #endregion
    }
}
