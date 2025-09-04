using MindShelf_DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Dtos.BookDto
{
    public class UpdateBookDto:CreateBookDto
    {
        [Required]
        public int BookId { get; set; }
      
    }
}
