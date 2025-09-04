using System;


namespace MindShelf_BL.Dtos.ReviewDto
{
    public class UserReviewsDto
    {
        public int ReviewId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public double Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}