using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Interfaces
{
    public interface IRepository<T> where T : class
    {

        //GetAll 
        Task<IEnumerable<T>> GetAll();

        // GetBY Id
        Task<T> GetById(int id);
        //Add 
        Task<T>Add(T entity);
        // Updata 
        void Update(T entity);
        // Delete 
        Task<bool> Delete(int id);
        IQueryable<T> Query();

        Task<int> SaveChangesAsync();
    }
}
