using Microsoft.EntityFrameworkCore;
using RestaurantMenuApp.Data;
using RestaurantMenuApp.Models;

namespace RestaurantMenuApp.Services;

public class CategoryService(ApplicationDbContext db) : ICategoryService
{
    public async Task<IReadOnlyList<Category>> GetActiveAsync() =>
        await db.Categories.AsNoTracking().Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder).ToListAsync();
}
