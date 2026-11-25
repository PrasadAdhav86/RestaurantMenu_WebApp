using System.ComponentModel.DataAnnotations;

namespace RestaurantMenuApp.Models;

public class Category
{
    public int CategoryId { get; set; }

    [Required, StringLength(80)]
    public string CategoryName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(0, 999)]
    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}
