using Microsoft.AspNetCore.Mvc;
using RestaurantMenuApp.Services;

namespace RestaurantMenuApp.Controllers;

public class HomeController(IMenuService menu, ICategoryService categories) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.PopularItems = await menu.GetPopularAsync();
        ViewBag.Categories = await categories.GetActiveAsync();
        return View(await menu.GetAvailableAsync(8));
    }

    public IActionResult About() => View();
    public IActionResult Contact() => View();
    public IActionResult Error() => View();
}
