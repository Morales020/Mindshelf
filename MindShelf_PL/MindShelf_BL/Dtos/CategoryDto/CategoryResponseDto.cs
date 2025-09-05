using MindShelf_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Dtos.CategoryDto
{
    public class CategoryResponseDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public List<Book> Books { get; set; }
    }
}
