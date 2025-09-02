using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_DAL.Data
{
    public class MindShelfDbContext :IdentityDbContext
    {
        public MindShelfDbContext(DbContextOptions<MindShelfDbContext> options) : base(options) { }


        // dbset 

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Flunt Api 
        }

    }
}
