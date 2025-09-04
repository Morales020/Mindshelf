using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Dtos.CategoryDto
{
    public class UpateCategoryDto
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }
        [MaxLength(300), MinLength(10, ErrorMessage = "Description must be between 10 and 300 characters.")]
        public string Description { get; set; }
    }
}
