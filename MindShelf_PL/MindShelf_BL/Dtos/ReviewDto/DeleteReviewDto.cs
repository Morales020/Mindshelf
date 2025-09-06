using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MindShelf_BL.Dtos.ReviewDto
{
    public class DeleteReviewDto
    {
        [Required(ErrorMessage = "Review ID is required."), Range(1, int.MaxValue, ErrorMessage = "Review ID must be greater than 0")]
        public int ReviewId { get; set; }
        
        [Required(ErrorMessage = "User Name is required."), MaxLength(100, ErrorMessage = "User Name must be less than 100 characters")]
        public string UserName { get; set; } = string.Empty;
    }
}