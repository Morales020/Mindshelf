using MindShelf_BL.Interfaces.IServices;
using MindShelf_DAL.Models.EmailModels;
using System.Net;
using System.Net.Mail;

namespace MindShelf_BL.Services
{
    public class EmailServices : IEmailServies
    {
        public void Send(Email email)
        {
            using (var client = new SmtpClient("smtp.gmail.com"))
            {
                client.Port = 587;
                client.Credentials = new NetworkCredential("Email.com", "yourAppPassword");
                client.EnableSsl = true;

                var mail = new MailMessage
                {
                    From = new MailAddress("yourEmail@gmail.com"),
                    Subject = email.Subject,
                    Body = email.Body,
                    IsBodyHtml = true
                };

                mail.To.Add(email.To);

                client.Send(mail);
                Console.WriteLine
                    ($" Sending to :{email.To } subject : {email.Subject} body {email.Body} from :{email.From}");
            }
        }
    }
}
