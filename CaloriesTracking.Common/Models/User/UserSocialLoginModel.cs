using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaloriesTracking.Common.Models.User;
public class UserSocialLoginModel
{
    public string SocialAccountType { get; set; }
    public string ExpiresIn { get; set; }
    //public string RefreshToken { get; set; }
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public string SocialAccountUserId { get; set; }
    //public string ReferralCode { get; set; }
    //public string InviteId { get; set; }
    //public string RegisterCode { get; set; }

}
