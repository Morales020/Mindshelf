using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MindShelf_BL.Dtos.ReviewDto;
using MindShelf_BL.Helper;

namespace MindShelf_BL.Interfaces.IServices
{
    public interface IReviewServices
    {
        Task<ResponseMVC<IEnumerable<ReviewResponseDto>>> GetBookReviews(GetBookReviewsDto getBookReviews);
        Task<ResponseMVC<ReviewResponseDto>> GetReviewById(int reviewId);
        Task<ResponseMVC<ReviewResponseDto>> CreateReviewAsync(CreateReviewDto review);
        Task<ResponseMVC<ReviewResponseDto>> UpdateReviewAsync(UpdateReviewDto review);
        Task<ResponseMVC<bool>> DeleteReviewAsync(DeleteReviewDto deleteReview);
    }
}