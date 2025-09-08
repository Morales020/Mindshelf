using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Dtos.AccountDto
{
    public class ForgetPasswordEmailDto
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
