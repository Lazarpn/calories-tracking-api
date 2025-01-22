using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaloriesTracking.Entities;
public class Meal
{
    public Guid Id { get; set; }

    [MaxLength(50)]
    public string Name { get; set; }
    public DateTime Date { get; set; }

    [Range(0, 20000)]
    public int Calories { get; set; }

    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    [InverseProperty(nameof(Entities.User.Meals))]
    public User User { get; set; }
}
