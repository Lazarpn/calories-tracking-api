using CaloriesTracking.Common.Helpers;
using CaloriesTracking.Data.Configuration;
using CaloriesTracking.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaloriesTracking.Data;

public class CaloriesTrackingDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{

    public DbSet<Meal> Meals { get; set; }

    public CaloriesTrackingDbContext(DbContextOptions options) : base(options)
    {
        Database.Migrate();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //modelBuilder.ApplyConfiguration(new RoleConfiguration());
        //modelBuilder.ApplyConfiguration(new MealsConfiguration());
    }

    public async Task SeedData()
    {
        var rolesExist = await Roles.AnyAsync();

        if (!rolesExist)
        {
            await Roles.AddRangeAsync(new List<IdentityRole<Guid>> {
                new IdentityRole<Guid>
                {
                    Name = UserRoleConstants.Administrator,
                    NormalizedName = "ADMINISTRATOR"
                },
                new IdentityRole<Guid>
                {
                    Name = UserRoleConstants.User,
                    NormalizedName = "USER"
                }
            });

            await SaveChangesAsync();
        }
    }
}
