using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_BL.Helper;
using MindShelf_BL.Dtos.ReviewDto;

namespace MindShelf_BL.Services
{
    public class ReviewServices : IReviewServices
    {
        public Task<ResponseMVC<IEnumerable<ReviewResponseDto>>> GetBookReviews(GetBookReviewsDto getBookReviews)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseMVC<ReviewResponseDto>> GetReviewById(int reviewId)
        {
            throw new NotImplementedException();
        }   

        public Task<ResponseMVC<ReviewResponseDto>> CreateReviewAsync(CreateReviewDto review)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseMVC<ReviewResponseDto>> UpdateReviewAsync(UpdateReviewDto review)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseMVC<bool>> DeleteReviewAsync(DeleteReviewDto deleteReview)
        {
            throw new NotImplementedException();
        }
    }
}