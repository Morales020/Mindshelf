using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Dtos.AccountDto
{
    public class LoginDto
    {
        [Required(ErrorMessage ="Email is Required")]
        public string Email {  get; set; } = string.Empty;

        [Required (ErrorMessage ="Password is Required")]
        public string Password { get; set; } =string.Empty;
    }
}
