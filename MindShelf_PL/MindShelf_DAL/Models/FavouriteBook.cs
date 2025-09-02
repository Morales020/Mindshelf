using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_DAL.Models
{
    public class FavouriteBook
    {
        [Key]
        public int FavouriteBookId { get; set; }

        [Required]
        public string UserId { get; set; }
        public string UserName { get; set; }
        public User User { get; set; }


        [Required]
        public int BookId { get; set; }
        public Book Book { get; set; }


        [DataType(DataType.DateTime)]
        public DateTime AddedDate { get; set; }

    }
}
