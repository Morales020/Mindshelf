using System;

namespace MindShelf_DAL.Models
{
	public class PrivateMessage
	{
		public int Id { get; set; }
		public string SenderId { get; set; } = string.Empty;
		public string ReceiverId { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
		public DateTime SentAt { get; set; }
		public bool IsRead { get; set; }
	}
}


