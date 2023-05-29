using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using CaloriesTracking.Common.Helpers;

namespace CaloriesTracking.Data.Configuration;
public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)

    {
        builder.HasData(
            new IdentityRole
            {
                Name = UserRoleConstants.Administrator,
                //NormalizedName = "ADMINISTRATOR"
            },
            new IdentityRole
            {
                Name = UserRoleConstants.User,
                //NormalizedName = "USER"
            }

        );

    }
}
