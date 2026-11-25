using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantMenuApp.Models;

public enum FoodType
{
    Veg = 0,
    NonVeg = 1
}

public enum SpicyLevel
{
    Mild = 0,
    Medium = 1,
    Hot = 2,
    ExtraHot = 3
}

public class MenuItem
{
    public int MenuItemId { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required, StringLength(120)]
    public string ItemName { get; set; } = string.Empty;

    [Required, StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 100000)]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public FoodType FoodType { get; set; } = FoodType.Veg;
    public bool IsAvailable { get; set; } = true;
    public bool IsPopular { get; set; }

    [StringLength(1000)]
    public string? Ingredients { get; set; }

    [Range(1, 600)]
    public int PreparationTime { get; set; } = 15;

    public SpicyLevel SpicyLevel { get; set; } = SpicyLevel.Mild;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }

    public Category? Category { get; set; }
}
