using RestaurantMenuApp.Models;

namespace RestaurantMenuApp.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetActiveAsync();
}
