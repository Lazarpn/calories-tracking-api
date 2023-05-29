using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaloriesTracking.Common.Models.Meal;
public class MealUpdateModel
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    public DateTime Date { get; set; }

    [Range(0, 20000)]
    public int Calories { get; set; }

}
