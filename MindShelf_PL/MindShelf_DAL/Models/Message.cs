using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace MindShelf_DAL.Models
{
	public class Message
	{
		public int Id { get; set; }
		public string SenderId { get; set; }
        public string GroupId { get; set; }
        public string SenderName { get; set; }
		public string Content { get; set; }
		public DateTime SentAt { get; set; }
    }
}


