using MindShelf_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Dtos.ReviewDto
{
    public class ReviewResponseDto
    {
        public string Comment { get; set; } = string.Empty;
        public double Rating { get; set; }
        public int ReviewId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public int BookId { get; set; }
        public string BookName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    
}