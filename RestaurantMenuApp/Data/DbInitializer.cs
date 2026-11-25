using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantMenuApp.Models;

namespace RestaurantMenuApp.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        var db = services.GetRequiredService<ApplicationDbContext>();
        if (!await db.Categories.AnyAsync())
        {
            var names = new[] { "Starters", "Soups", "Salads", "Main Course", "Indian Cuisine",
                "Chinese Cuisine", "Italian Cuisine", "Fast Food", "Beverages", "Desserts",
                "Special Combos", "Today's Specials" };
            for (var i = 0; i < names.Length; i++)
                db.Categories.Add(new Category { CategoryName = names[i], DisplayOrder = i + 1, IsActive = true });
            await db.SaveChangesAsync();
        }

        var email = configuration["AdminSeed:Email"];
        var password = configuration["AdminSeed:Password"];

        // Do not ship a default password. Set AdminSeed__Email and AdminSeed__Password
        // as environment variables (or use user-secrets) before first startup.
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser { UserName = email, Email = email, FullName = "Restaurant Administrator", EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException("Admin creation failed: " +
                    string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        if (!await userManager.IsInRoleAsync(user, "Admin"))
            await userManager.AddToRoleAsync(user, "Admin");

    }
}
