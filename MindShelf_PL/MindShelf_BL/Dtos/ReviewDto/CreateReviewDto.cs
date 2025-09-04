using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MindShelf_BL.Dtos.ReviewDto
{
    public class CreateReviewDto
    {
       [Required(ErrorMessage = "User Name is required.")]
        public string UserName{ get; set; }=string.Empty;

        [Required(ErrorMessage = "Book ID is required.")]
        public int BookId { get; set; }
        [Required(ErrorMessage = "Rating is required.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public double Rating { get; set; }
        [MaxLength(500, ErrorMessage = "Comment cannot exceed 500 characters.")]
        public string? Comment { get; set; } 
    }
}