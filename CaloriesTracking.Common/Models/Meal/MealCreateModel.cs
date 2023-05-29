using System.ComponentModel.DataAnnotations;

namespace CaloriesTracking.Common.Models.Meal;
public class MealCreateModel
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    public DateTime Date { get; set; }

    [Range(0, 20000)]
    public int Calories { get; set; }

}
