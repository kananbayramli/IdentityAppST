using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityApp.Models;

public static class IdentitySeedData
{
    private const string adminUser = "Admin";
    private const string adminPassword = "Admin_123";

    public static async void IdentityTestUser(IApplicationBuilder app)
    {
        var context = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<IdentityContext>();

        if(context.Database.GetAppliedMigrations().Any()) // burda yoxluyurug gozleyen migration var ya yox
        {
            //yoxdusa burda update eliyirik
            context.Database.Migrate();
        }

        var userManager = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var user = await userManager.FindByNameAsync(adminUser);

        if(user == null)
        {
            user = new IdentityUser
            {
                UserName = adminUser,
                Email = "bayramlikenan920@gmail.com",
                PhoneNumber = "777725217"
            };

            await userManager.CreateAsync(user, adminPassword);
        }
    }
}
