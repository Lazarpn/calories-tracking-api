using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaloriesTracking.Entities;
public class User : IdentityUser<Guid>
{
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; }

    //[MaxLength(100, ErrorMessage = "Last name must be max 100 characters")]

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; }

    [Range(0, 20000)]
    public int CaloriesPreference { get; set; }
    public byte[] UserPhoto { get; set; }



    [InverseProperty(nameof(Meal.User))]
    public ICollection<Meal> Meals { get; set; } = new HashSet<Meal>();
}
