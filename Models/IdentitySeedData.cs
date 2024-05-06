using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityApp.Models
{
    
    public static class IdentitySeedData
    {
        private const string adminUser ="admin";
         private const string adminPassword ="Admin_123";

         public static async void IdentityTestUser(IApplicationBuilder app)
         {
            var context = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<IdentityContext>();

            if(context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }

            var userManager = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            var user = await userManager.FindByNameAsync(adminUser);

            if(user == null)
            {
                user = new AppUser{
                    FullName="Furkan Sarı",
                    UserName = adminUser,
                    Email = "admin@fs.com",
                    PhoneNumber = "333333",

                };

                await userManager.CreateAsync(user, adminPassword);
            }
            
         }
    }
}