using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_DAL.Models
{
    public class EventRegistration
    {
        [Key]
        public int EventRegistrationId { get; set; }

        [Required]
        public DateTime RegistrationDate { get; set; }

        public string? Notes { get; set; }

        [Required]
        [StringLength(100)]
        public string UserName { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public User User { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public Event Event { get; set; }
    }
}
