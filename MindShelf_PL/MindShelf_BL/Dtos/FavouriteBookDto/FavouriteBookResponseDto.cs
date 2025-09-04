using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Dtos.FavouriteBookDto
{
    public class FavouriteBookResponseDto
    {

        public int BookId { get; set; }
        public int FavouriteBookId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime? AddedDate { get; set; }
    }
}
