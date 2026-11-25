using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantMenuApp.Data;
using RestaurantMenuApp.Models;
using RestaurantMenuApp.ViewModels;

namespace RestaurantMenuApp.Controllers;

[Authorize(Roles = "Admin")]
public class MenuManagementController(ApplicationDbContext db, IWebHostEnvironment environment) : Controller
{
    private const long MaxImageBytes = 5 * 1024 * 1024;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public async Task<IActionResult> Index() =>
        View(await db.MenuItems.Include(m => m.Category).OrderBy(m => m.ItemName).ToListAsync());

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new MenuItemFormViewModel();
        await PopulateCategories(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MenuItemFormViewModel vm)
    {
        await PopulateCategories(vm);
        if (!ModelState.IsValid) return View(vm);
        var imageUrl = await SaveImageAsync(vm.ImageFile);
        if (vm.ImageFile is not null && imageUrl is null) return View(vm);

        var item = new MenuItem
        {
            ItemName = vm.ItemName, Description = vm.Description, Price = vm.Price,
            CategoryId = vm.CategoryId, FoodType = vm.FoodType, IsAvailable = vm.IsAvailable,
            IsPopular = vm.IsPopular, Ingredients = vm.Ingredients,
            PreparationTime = vm.PreparationTime, SpicyLevel = vm.SpicyLevel, ImageUrl = imageUrl
        };
        db.MenuItems.Add(item);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await db.MenuItems.FindAsync(id);
        if (item is null) return NotFound();
        var vm = new MenuItemFormViewModel
        {
            MenuItemId = item.MenuItemId, ItemName = item.ItemName, Description = item.Description,
            Price = item.Price, CategoryId = item.CategoryId, FoodType = item.FoodType,
            IsAvailable = item.IsAvailable, IsPopular = item.IsPopular, Ingredients = item.Ingredients,
            PreparationTime = item.PreparationTime, SpicyLevel = item.SpicyLevel, ExistingImageUrl = item.ImageUrl
        };
        await PopulateCategories(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MenuItemFormViewModel vm)
    {
        if (id != vm.MenuItemId) return BadRequest();
        await PopulateCategories(vm);
        if (!ModelState.IsValid) return View(vm);
        var item = await db.MenuItems.FindAsync(id);
        if (item is null) return NotFound();

        if (vm.ImageFile is not null)
        {
            var newUrl = await SaveImageAsync(vm.ImageFile);
            if (newUrl is null) return View(vm);
            DeleteImage(item.ImageUrl);
            item.ImageUrl = newUrl;
        }

        item.ItemName = vm.ItemName; item.Description = vm.Description; item.Price = vm.Price;
        item.CategoryId = vm.CategoryId; item.FoodType = vm.FoodType; item.IsAvailable = vm.IsAvailable;
        item.IsPopular = vm.IsPopular; item.Ingredients = vm.Ingredients;
        item.PreparationTime = vm.PreparationTime; item.SpicyLevel = vm.SpicyLevel;
        item.ModifiedDate = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await db.MenuItems.FindAsync(id);
        if (item is null) return NotFound();
        DeleteImage(item.ImageUrl);
        db.MenuItems.Remove(item);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAvailability(int id)
    {
        var item = await db.MenuItems.FindAsync(id);
        if (item is null) return NotFound();
        item.IsAvailable = !item.IsAvailable;
        item.ModifiedDate = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateCategories(MenuItemFormViewModel vm) =>
        vm.Categories = await db.Categories.Where(c => c.IsActive).OrderBy(c => c.DisplayOrder)
            .Select(c => new SelectListItem(c.CategoryName, c.CategoryId.ToString())).ToListAsync();

    private async Task<string?> SaveImageAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (file.Length > MaxImageBytes || !AllowedExtensions.Contains(extension))
        {
            ModelState.AddModelError(nameof(MenuItemFormViewModel.ImageFile), "Use JPG, PNG, or WEBP images up to 5 MB.");
            return null;
        }
        var folder = Path.Combine(environment.WebRootPath, "uploads");
        Directory.CreateDirectory(folder);
        var name = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(folder, name);
        await using var stream = System.IO.File.Create(path);
        await file.CopyToAsync(stream);
        return $"/uploads/{name}";
    }

    private void DeleteImage(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase)) return;
        var path = Path.Combine(environment.WebRootPath, "uploads", Path.GetFileName(url));
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
    }
}
