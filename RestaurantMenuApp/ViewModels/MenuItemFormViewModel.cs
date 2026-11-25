using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using RestaurantMenuApp.Models;

namespace RestaurantMenuApp.ViewModels;

public class MenuItemFormViewModel
{
    public int MenuItemId { get; set; }

    [Required, StringLength(120)]
    public string ItemName { get; set; } = "";

    [Required, StringLength(1000)]
    public string Description { get; set; } = "";

    [Range(0.01, 100000)]
    public decimal Price { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public FoodType FoodType { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsPopular { get; set; }

    [StringLength(1000)]
    public string? Ingredients { get; set; }

    [Range(1, 600)]
    public int PreparationTime { get; set; } = 15;

    public SpicyLevel SpicyLevel { get; set; }
    public string? ExistingImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }
    public IEnumerable<SelectListItem> Categories { get; set; } = [];
}
