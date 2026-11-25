using RestaurantMenuApp.Models;
using RestaurantMenuApp.ViewModels;

namespace RestaurantMenuApp.Services;

public interface IMenuService
{
    Task<MenuIndexViewModel> SearchAsync(MenuIndexViewModel filter);
    Task<MenuItem?> GetByIdAsync(int id);
    Task<IReadOnlyList<MenuItem>> GetPopularAsync(int count = 6);
    Task<IReadOnlyList<MenuItem>> GetAvailableAsync(int count = 12);
}
