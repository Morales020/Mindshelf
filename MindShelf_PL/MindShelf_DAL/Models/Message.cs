using System;

namespace MindShelf_DAL.Models
{
	public class Message
	{
		public int Id { get; set; }
		public string SenderId { get; set; }
		public string SenderName { get; set; }
		public string Content { get; set; }
		public DateTime SentAt { get; set; }
	}
}


