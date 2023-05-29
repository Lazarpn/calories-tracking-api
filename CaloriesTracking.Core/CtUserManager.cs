using AutoMapper;
using CaloriesTracking.Common.Helpers;
using CaloriesTracking.Common.Models.User;
using CaloriesTracking.Data;
using CaloriesTracking.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace CaloriesTracking.Core;
public class CtUserManager
{
    private readonly CaloriesTrackingDbContext db;
    private readonly IMapper mapper;
    private readonly UserManager<User> userManager;

    public CtUserManager(CaloriesTrackingDbContext db, IMapper mapper, UserManager<User> userManager)
    {
        this.db = db;
        this.mapper = mapper;
        this.userManager = userManager;
    }

    public async Task UpdateCaloriesPreference(Guid userId, UserCaloriesModel model)
    {
        var user = await db.Users.FindAsync(userId);
        user.CaloriesPreference = model.CaloriesPreference;

        await db.SaveChangesAsync();
    }

    public async Task UpdatePhoto(Guid userId, UserPhotoModel model)
    {
        var user = await db.Users.FindAsync(userId);
        byte[] photoBytes = Convert.FromBase64String(model.UserPhoto);
        user.UserPhoto = photoBytes;

        await db.SaveChangesAsync();
    }
    public async Task UpdateUserInfo(Guid userId, UserUpdateModel model)
    {
        var user = await db.Users.FindAsync(userId);
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;

        await db.SaveChangesAsync();
    }

    public async Task<UserPhotoModel> GetPhoto(Guid userId)
    {
        var user = await db.Users.FindAsync(userId);
        var userPhoto = new UserPhotoModel { UserPhoto = Convert.ToBase64String(user.UserPhoto) };

        return userPhoto;
    }

    public async Task<UserMeModel> GetUserInfo(string email)
    {
        var user = await db.Users.FirstAsync(u => u.Email == email);
        var userInfo = new UserMeModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = email,
            CaloriesPreference = user.CaloriesPreference,
            UserPhotoByte = user.UserPhoto,
        };

        return userInfo;
    }

    // ADMIN

    public async Task<List<UserMeModel>> GetUsers()
    {
        var users = await userManager.GetUsersInRoleAsync("user");
        // TODO: jel je ovde okej koristiti map jer pod jedan imam listu, pod 2 user-e
        // po roli mogu samo ovako da dobijem i ne mogu nikako projectTo?
        var usersList = mapper.Map<List<UserMeModel>>(users);
        return usersList;
    }

    public async Task UpdateUser(Guid userId, UserAdminUpdateModel model)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Email = model.Email;
        user.CaloriesPreference = model.CaloriesPreference;

        await userManager.UpdateAsync(user);
    }

    public async Task DeleteUser(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        await userManager.DeleteAsync(user);
    }
}
