using Microsoft.AspNetCore.Mvc;
using RestaurantMenuApp.Services;
using RestaurantMenuApp.ViewModels;

namespace RestaurantMenuApp.Controllers;

public class MenuController(IMenuService menu) : Controller
{
    public async Task<IActionResult> Index(MenuIndexViewModel filter) =>
        View(await menu.SearchAsync(filter));

    public async Task<IActionResult> Details(int id)
    {
        var item = await menu.GetByIdAsync(id);
        return item is null ? NotFound() : View(item);
    }
}
