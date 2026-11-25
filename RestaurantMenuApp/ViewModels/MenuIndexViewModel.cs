using RestaurantMenuApp.Models;

namespace RestaurantMenuApp.ViewModels;

public class MenuIndexViewModel
{
    public IReadOnlyList<MenuItem> Items { get; set; } = [];
    public IReadOnlyList<Category> Categories { get; set; } = [];
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public string? FoodType { get; set; }
    public string? Sort { get; set; }
    public bool AvailableOnly { get; set; }
    public bool PopularOnly { get; set; }
}
