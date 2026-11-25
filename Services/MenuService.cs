using Microsoft.EntityFrameworkCore;
using RestaurantMenuApp.Data;
using RestaurantMenuApp.Models;
using RestaurantMenuApp.ViewModels;

namespace RestaurantMenuApp.Services;

public class MenuService(ApplicationDbContext db) : IMenuService
{
    public async Task<MenuIndexViewModel> SearchAsync(MenuIndexViewModel filter)
    {
        var query = db.MenuItems.Include(m => m.Category).AsNoTracking()
            .Where(m => m.Category != null && m.Category.IsActive);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(m => m.ItemName.Contains(filter.Search) || m.Description.Contains(filter.Search));
        if (filter.CategoryId.HasValue)
            query = query.Where(m => m.CategoryId == filter.CategoryId.Value);
        if (Enum.TryParse<FoodType>(filter.FoodType, true, out var type))
            query = query.Where(m => m.FoodType == type);
        if (filter.AvailableOnly)
            query = query.Where(m => m.IsAvailable);
        if (filter.PopularOnly)
            query = query.Where(m => m.IsPopular);

        query = filter.Sort switch
        {
            "price-low" => query.OrderBy(m => m.Price),
            "price-high" => query.OrderByDescending(m => m.Price),
            "name" => query.OrderBy(m => m.ItemName),
            _ => query.OrderBy(m => m.Category!.DisplayOrder).ThenBy(m => m.ItemName)
        };

        filter.Items = await query.ToListAsync();
        filter.Categories = await db.Categories.AsNoTracking()
            .Where(c => c.IsActive).OrderBy(c => c.DisplayOrder).ToListAsync();
        return filter;
    }

    public Task<MenuItem?> GetByIdAsync(int id) =>
        db.MenuItems.Include(m => m.Category).AsNoTracking().FirstOrDefaultAsync(m => m.MenuItemId == id);

    public async Task<IReadOnlyList<MenuItem>> GetPopularAsync(int count = 6) =>
        await db.MenuItems.Include(m => m.Category).AsNoTracking()
            .Where(m => m.IsPopular && m.IsAvailable && m.Category!.IsActive)
            .OrderBy(m => m.ItemName).Take(count).ToListAsync();

    public async Task<IReadOnlyList<MenuItem>> GetAvailableAsync(int count = 12) =>
        await db.MenuItems.Include(m => m.Category).AsNoTracking()
            .Where(m => m.IsAvailable && m.Category!.IsActive)
            .OrderBy(m => m.Category!.DisplayOrder).ThenBy(m => m.ItemName)
            .Take(count).ToListAsync();
}
