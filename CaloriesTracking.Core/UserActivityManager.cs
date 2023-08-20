using CaloriesTracking.Common.Helpers;
using CaloriesTracking.Data;
using CaloriesTracking.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaloriesTracking.Core;
public class UserActivityManager
{
    private readonly CaloriesTrackingDbContext db;

    public UserActivityManager(CaloriesTrackingDbContext db)
    {
        this.db = db;
    }

    public async Task<User> GetUser(Guid userId)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<string> GetUserRole(Guid userId)
    {
        var roleRow = await db.UserRoles.FirstOrDefaultAsync(row => row.UserId == userId);
        var role = await db.Roles.FirstOrDefaultAsync(r => r.Id == roleRow.RoleId);
        return role.Name;
    }
}
