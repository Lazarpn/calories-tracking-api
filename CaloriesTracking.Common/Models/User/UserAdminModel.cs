using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaloriesTracking.Common.Models.User;
public class UserAdminModel
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int? CaloriesPreference { get; set; }
    public string ThumbUrl { get; set; }
}
