using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaloriesTracking.Common.Models.User;

public class AuthResponseModel
{
    public Guid UserId { get; set; }
    public string Token { get; set; }
    public UserMeModel User { get; set; }
}
