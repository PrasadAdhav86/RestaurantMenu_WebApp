using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantMenuApp.Data;
using RestaurantMenuApp.ViewModels;

namespace RestaurantMenuApp.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var vm = new DashboardViewModel
        {
            TotalCategories = await db.Categories.CountAsync(),
            TotalItems = await db.MenuItems.CountAsync(),
            AvailableItems = await db.MenuItems.CountAsync(m => m.IsAvailable),
            UnavailableItems = await db.MenuItems.CountAsync(m => !m.IsAvailable),
            PopularItems = await db.MenuItems.CountAsync(m => m.IsPopular)
        };
        return View(vm);
    }
}
