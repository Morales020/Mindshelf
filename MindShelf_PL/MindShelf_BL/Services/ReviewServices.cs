using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_BL.Helper;
using MindShelf_BL.Dtos.ReviewDto;
using MindShelf_BL.UnitWork;
using MindShelf_DAL.Models;

namespace MindShelf_BL.Services
{
    public class ReviewServices : IReviewServices
    {
        private readonly UnitOfWork _UnitOfWork;

        public ReviewServices(UnitOfWork unitOf)
        {
            _UnitOfWork = unitOf;
        }

        #region GetBookReviews
        public async Task<ResponseMVC<GetBookReviewsDto>> GetBookReviews(GetBookReviewsDto getBookReviews)
        {
            try
            {
                var book = await _UnitOfWork.BookRepo.Query()
                    .Include(b => b.Reviews)
                        .ThenInclude(r => r.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.BookId == getBookReviews.BookId);

                if (book == null)
                    return new ResponseMVC<GetBookReviewsDto>(404, $"Book with ID {getBookReviews.BookId} not found", null);

                var reviews = book.Reviews?.ToList() ?? new List<Review>();
                var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

                var userReviews = reviews.Select(r => new UserReviewsDto
                {
                    ReviewId = r.ReviewId,
                    Comment = r.Comment,
                    Rating = r.Rating,
                    CreatedAt = r.CreatedAt,
                    UserName = r.User?.UserName ?? "Unknown User"
                }).ToList();

                var result = new GetBookReviewsDto
                {
                    BookId = book.BookId,
                    BookName = book.Title,
                    AverageRating = Math.Round(averageRating, 2),
                    ReviewCount = reviews.Count,
                    Reviews = userReviews
                };

                return new ResponseMVC<GetBookReviewsDto>(200, "Success", result);
            }
            catch (Exception ex)
            {
                return new ResponseMVC<GetBookReviewsDto>(500, $"Error: {ex.Message}", null);
            }
        }
        #endregion

        #region GetReviewById
        public async Task<ResponseMVC<ReviewResponseDto>> GetReviewById(int reviewId)
        {
            try
            {
                var review = await _UnitOfWork.ReviewRepo.Query()
                    .Include(r => r.Book)
                    .Include(r => r.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.ReviewId == reviewId);

                if (review == null)
                    return new ResponseMVC<ReviewResponseDto>(404, $"Review with ID {reviewId} not found", null);

                var result = new ReviewResponseDto
                {
                    ReviewId = review.ReviewId,
                    Comment = review.Comment,
                    Rating = review.Rating,
                    CreatedAt = review.CreatedAt,
                    BookId = review.BookId,
                    BookName = review.Book?.Title ?? "Unknown Book",
                    UserId = review.UserId,
                    UserName = review.User?.UserName ?? "Unknown User"
                };

                return new ResponseMVC<ReviewResponseDto>(200, "Success", result);
            }
            catch (Exception ex)
            {
                return new ResponseMVC<ReviewResponseDto>(500, $"Error: {ex.Message}", null);
            }
        }
        #endregion

        #region CreateReview
        public async Task<ResponseMVC<ReviewResponseDto>> CreateReviewAsync(CreateReviewDto review)
        {
            try
            {
                // Validate if book exists
                var book = await _UnitOfWork.BookRepo.GetById(review.BookId);
                if (book == null)
                    return new ResponseMVC<ReviewResponseDto>(404, $"Book with ID {review.BookId} not found", null);

                // Validate if user exists by username
                var user = await _UnitOfWork._dbcontext.Users.FirstOrDefaultAsync(u => u.UserName == review.UserName);
                if (user == null)
                    return new ResponseMVC<ReviewResponseDto>(404, $"User with username {review.UserName} not found", null);

                // Check if user already reviewed this book
                var existingReview = await _UnitOfWork.ReviewRepo.Query()
                    .FirstOrDefaultAsync(r => r.BookId == review.BookId && r.UserId == user.Id);

                if (existingReview != null)
                    return new ResponseMVC<ReviewResponseDto>(400, "User has already reviewed this book", null);

                var newReview = new Review
                {
                    Comment = review.Comment ?? string.Empty,
                    Rating = review.Rating,
                    BookId = review.BookId,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };

                await _UnitOfWork.ReviewRepo.Add(newReview);
                await _UnitOfWork.SaveChangesAsync();

                // Update book rating
                var bookReviews = await _UnitOfWork.ReviewRepo.Query()
                    .Where(r => r.BookId == review.BookId)
                    .ToListAsync();

                book.Rating = bookReviews.Average(r => r.Rating);
                await _UnitOfWork.SaveChangesAsync();

                var result = new ReviewResponseDto
                {
                    ReviewId = newReview.ReviewId,
                    Comment = newReview.Comment,
                    Rating = newReview.Rating,
                    CreatedAt = newReview.CreatedAt,
                    BookId = newReview.BookId,
                    BookName = book.Title,
                    UserId = newReview.UserId,
                    UserName = user.UserName
                };

                return new ResponseMVC<ReviewResponseDto>(201, "Review created successfully", result);
            }
            catch (Exception ex)
            {
                return new ResponseMVC<ReviewResponseDto>(500, $"Error: {ex.Message}", null);
            }
        }
        #endregion

        #region UpdateReview
        public async Task<ResponseMVC<ReviewResponseDto>> UpdateReviewAsync(UpdateReviewDto review)
        {
            try
            {
                var existingReview = await _UnitOfWork.ReviewRepo.Query()
                    .Include(r => r.Book)
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.ReviewId == review.ReviewId);

                if (existingReview == null)
                    return new ResponseMVC<ReviewResponseDto>(404, $"Review with ID {review.ReviewId} not found", null);

                // For now, allow updates without user validation since UpdateReviewDto doesn't have UserId
                // In a real application, you would get the current user from authentication context

                existingReview.Comment = review.Comment;
                existingReview.Rating = review.Rating;

                await _UnitOfWork.SaveChangesAsync();

                // Update book rating
                var bookReviews = await _UnitOfWork.ReviewRepo.Query()
                    .Where(r => r.BookId == existingReview.BookId)
                    .ToListAsync();

                existingReview.Book.Rating = bookReviews.Average(r => r.Rating);
                await _UnitOfWork.SaveChangesAsync();

                var result = new ReviewResponseDto
                {
                    ReviewId = existingReview.ReviewId,
                    Comment = existingReview.Comment,
                    Rating = existingReview.Rating,
                    CreatedAt = existingReview.CreatedAt,
                    BookId = existingReview.BookId,
                    BookName = existingReview.Book?.Title ?? "Unknown Book",
                    UserId = existingReview.UserId,
                    UserName = existingReview.User?.UserName ?? "Unknown User"
                };

                return new ResponseMVC<ReviewResponseDto>(200, "Review updated successfully", result);
            }
            catch (Exception ex)
            {
                return new ResponseMVC<ReviewResponseDto>(500, $"Error: {ex.Message}", null);
            }
        }
        #endregion

        #region DeleteReview
        public async Task<ResponseMVC<bool>> DeleteReviewAsync(DeleteReviewDto deleteReview)
        {
            try
            {
                var existingReview = await _UnitOfWork.ReviewRepo.Query()
                    .Include(r => r.Book)
                    .FirstOrDefaultAsync(r => r.ReviewId == deleteReview.ReviewId);

                if (existingReview == null)
                    return new ResponseMVC<bool>(404, $"Review with ID {deleteReview.ReviewId} not found", false);

                // Find user by username to validate ownership
                var user = await _UnitOfWork._dbcontext.Users.FirstOrDefaultAsync(u => u.UserName == deleteReview.UserName);
                if (user == null)
                    return new ResponseMVC<bool>(404, $"User with username {deleteReview.UserName} not found", false);

                // Check if user owns this review
                if (existingReview.UserId != user.Id)
                    return new ResponseMVC<bool>(403, "You can only delete your own reviews", false);

                var bookId = existingReview.BookId;

                _UnitOfWork.ReviewRepo.Delete(deleteReview.ReviewId);
                await _UnitOfWork.SaveChangesAsync();

                // Update book rating
                var remainingReviews = await _UnitOfWork.ReviewRepo.Query()
                    .Where(r => r.BookId == bookId)
                    .ToListAsync();

                if (remainingReviews.Any())
                {
                    existingReview.Book.Rating = remainingReviews.Average(r => r.Rating);
                }
                else
                {
                    existingReview.Book.Rating = 0;
                }

                await _UnitOfWork.SaveChangesAsync();

                return new ResponseMVC<bool>(200, "Review deleted successfully", true);
            }
            catch (Exception ex)
            {
                return new ResponseMVC<bool>(500, $"Error: {ex.Message}", false);
            }
        }
        #endregion
    }
}