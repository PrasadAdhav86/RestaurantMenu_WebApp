using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantMenuApp.Data;
using RestaurantMenuApp.Models;

namespace RestaurantMenuApp.Controllers;

[Authorize(Roles = "Admin")]
public class CategoryController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index() =>
        View(await db.Categories.Include(c => c.MenuItems).OrderBy(c => c.DisplayOrder).ToListAsync());

    [HttpGet]
    public IActionResult Create() => View(new Category());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (!ModelState.IsValid) return View(category);
        db.Categories.Add(category);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { ModelState.AddModelError(nameof(category.CategoryName), "A category with this name may already exist."); return View(category); }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await db.Categories.FindAsync(id);
        return category is null ? NotFound() : View(category);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.CategoryId) return BadRequest();
        if (!ModelState.IsValid) return View(category);
        category.CreatedDate = (await db.Categories.AsNoTracking().Where(c => c.CategoryId == id).Select(c => c.CreatedDate).FirstOrDefaultAsync());
        db.Categories.Update(category);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { ModelState.AddModelError(nameof(category.CategoryName), "Unable to save. Check for duplicate category names."); return View(category); }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null) return NotFound();
        category.IsActive = !category.IsActive;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await db.Categories.Include(c => c.MenuItems).FirstOrDefaultAsync(c => c.CategoryId == id);
        if (category is null) return NotFound();
        if (category.MenuItems.Count != 0)
        {
            TempData["Error"] = "Move or delete this category's menu items before deleting it.";
            return RedirectToAction(nameof(Index));
        }
        db.Categories.Remove(category);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
