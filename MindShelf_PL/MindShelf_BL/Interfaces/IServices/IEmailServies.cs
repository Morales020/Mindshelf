using MindShelf_DAL.Models.EmailModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Interfaces.IServices
{
    public interface IEmailServies
    {
        public void Send (Email email);
    }
}
