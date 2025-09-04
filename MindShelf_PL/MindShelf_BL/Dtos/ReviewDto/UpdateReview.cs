using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MindShelf_BL.Dtos.ReviewDto
{
    public class UpdateReview
    {
        [Required(ErrorMessage ="Review ID Is Required")]
        public int  ReviewId { get; set; }
        [Required(ErrorMessage ="Book ID Is Required")]
        public int BookId { get; set; }
        [Required(ErrorMessage = "Rating is required.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public double Rating { get; set; }

        [MaxLength(500, ErrorMessage = "Comment cannot exceed 500 characters.")]
        public string? Comment { get; set; }
    }
}