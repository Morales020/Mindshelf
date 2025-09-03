using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MindShelf_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Helper.SeedData
{
    public class AddUser
    {
        public static async Task Initiez(IServiceProvider service)
        {
            var _Rolemanger = service.GetRequiredService<RoleManager<IdentityRole>>();
            var _UserManger = service.GetRequiredService<UserManager<User>>();

            string[] RoleNames = { "Admin","Vender", "User" };
            foreach (var roles in RoleNames)
            {
                if (!await _Rolemanger.RoleExistsAsync(roles))
                {
                    await _Rolemanger.CreateAsync(new IdentityRole(roles));
                }
            }
            var adminEmail = "admin@system.com";
            var adminUserName = "admin";
            var adminPassword = "Admin@123";

            var UserAdmin = await _UserManger.FindByEmailAsync(adminEmail);
            if (UserAdmin == null)
            {
                UserAdmin = new User
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true
                };


                var result = await _UserManger.CreateAsync(UserAdmin, adminPassword);
                if (result.Succeeded)
                {
                    await _UserManger.AddToRoleAsync(UserAdmin, "Admin");
                }
                else
                {
                    throw new Exception("Failed to create admin user: " +
                              string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
