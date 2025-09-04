using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MindShelf_BL.Dtos.ReviewDto;

namespace MindShelf_BL.Dtos.ReviewDto
    {
    public class GetBookReviewsDto
    {

        public int BookId { get; set; }
        public string BookName { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<UserReviewsDto> Reviews { get; set; }=new List<UserReviewsDto>();
    }
}