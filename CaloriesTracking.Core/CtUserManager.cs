using AutoMapper;
using Azure.Storage.Blobs;
using CaloriesTracking.Common;
using CaloriesTracking.Common.Helpers;
using CaloriesTracking.Common.Models.User;
using CaloriesTracking.Data;
using CaloriesTracking.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace CaloriesTracking.Core;
public class CtUserManager
{
    private readonly CaloriesTrackingDbContext db;
    private readonly IConfiguration configuration;
    private readonly IMapper mapper;
    private readonly UserManager<User> userManager;
    //private readonly FileManager fileManager;
    private readonly int MINUTES_VERIFICATION_CODE_IS_VALID;

    public CtUserManager(
        CaloriesTrackingDbContext db,
        IConfiguration configuration,
        IMapper mapper, UserManager<User> userManager
        )
    {
        this.db = db;
        this.configuration = configuration;
        this.mapper = mapper;
        this.userManager = userManager;
        //this.fileManager = fileManager;
        MINUTES_VERIFICATION_CODE_IS_VALID = Convert.ToInt32(configuration["minutesVerificationCodeIsValid"]);
    }

    public async Task UpdateCaloriesPreference(Guid userId, UserCaloriesModel model)
    {
        var user = await db.Users.FindAsync(userId);
        ValidationHelper.MustExist(user);

        user.CaloriesPreference = model.CaloriesPreference;
  
        await db.SaveChangesAsync();
    }

    public async Task<UserPhotoModel> UpdatePhoto(Guid userId, UserPhotoUploadModel model)
    {
        if (model.File == null)
        {
            return null;
        };
        var user = await db.Users.FindAsync(userId);
        ValidationHelper.MustExist(user);

        //user.FileName = await fileManager.ProcessFileStorageUpload<User>(model.File, user.FileName);
        user.FileOriginalName = model.File.FileName;
        //user.FileUrl = await fileManager.GetFileStorageUrl<User>(user.FileName);
        //user.ThumbUrl = await fileManager.GetFileStorageUrl<User>(user.FileName, isThumb: true);
        await db.SaveChangesAsync();

        return new UserPhotoModel { FileUrl = user.ThumbUrl };
    }

    public async Task UpdateUserInfo(Guid userId, UserUpdateModel model)
    {
        var user = await db.Users.FindAsync(userId);
        ValidationHelper.MustExist(user);

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;

        await db.SaveChangesAsync();
    }

    public async Task<UserPhotoModel> GetPhoto(Guid userId)
    {
        var user = await db.Users.FindAsync(userId);
        ValidationHelper.MustExist(user);
        return new UserPhotoModel { FileUrl = user.ThumbUrl };
    }

    public async Task<UserMeModel> GetUserInfo(Guid userId)
    {
        var user = await db.Users.FirstAsync(u => u.Id == userId);
        ValidationHelper.MustExist(user);

        var userInfo = new UserMeModel
        {
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            EmailConfirmed = user.EmailConfirmed,
            DateVerificationCodeExpires = user.DateVerificationCodeSent.Value.AddMinutes(MINUTES_VERIFICATION_CODE_IS_VALID),
            CaloriesPreference = user.CaloriesPreference,
        };

        return userInfo;
    }

    // ADMIN

    public async Task<List<UserAdminModel>> GetUsers()
    {
        var users = await userManager.GetUsersInRoleAsync("user");
        // TODO: jel je ovde okej koristiti map jer pod jedan imam listu, pod 2 user-e
        // po roli mogu samo ovako da dobijem i ne mogu nikako projectTo?
        var usersList = mapper.Map<List<UserAdminModel>>(users);
        return usersList;
    }

    public async Task UpdateUser(string email, UserAdminUpdateModel model)
    {
        var user = await userManager.FindByEmailAsync(email);
        ValidationHelper.MustExist(user);

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Email = model.Email;
        user.CaloriesPreference = model.CaloriesPreference;

        await userManager.UpdateAsync(user);
    }

    public async Task DeleteUser(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        ValidationHelper.MustExist(user);
        await userManager.DeleteAsync(user);
    }
}
