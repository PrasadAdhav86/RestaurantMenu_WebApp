namespace RestaurantMenuApp.ViewModels;

public class DashboardViewModel
{
    public int TotalCategories { get; set; }
    public int TotalItems { get; set; }
    public int AvailableItems { get; set; }
    public int UnavailableItems { get; set; }
    public int PopularItems { get; set; }
}
