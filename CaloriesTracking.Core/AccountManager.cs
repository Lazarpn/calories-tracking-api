using AutoMapper;
using CaloriesTracking.Common.Enums;
using CaloriesTracking.Common.Exceptions;
using CaloriesTracking.Common.Helpers;
using CaloriesTracking.Common.Models.User;
using CaloriesTracking.Data;
using CaloriesTracking.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RTools_NTS.Util;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CaloriesTracking.Core;


public class AccountManager
{
    private readonly CaloriesTrackingDbContext db;
    private readonly SendGridEmailManager emailManager;
    private readonly JwtHelper jwtHelper;
    private readonly IMapper mapper;
    private readonly UserManager<User> userManager;
    private readonly IConfiguration configuration;
    private readonly string ANGULAR_APP_URL;

    public AccountManager(
        IMapper mapper,
        UserManager<User> userManager,
        IConfiguration configuration,
        CaloriesTrackingDbContext db,
        SendGridEmailManager emailManager
        )
    {
        this.mapper = mapper;
        this.configuration = configuration;
        this.userManager = userManager;
        this.db = db;
        this.emailManager = emailManager;
        jwtHelper = new JwtHelper(configuration);
        ANGULAR_APP_URL = configuration["AngularAppUrl"];
    }

    public async Task<AuthResponseModel> Login(UserLoginModel model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);

        //if (user == null)
        //{
        //    throw new Exception("User with a given email doesn't exist, please register!");
        //}

        //if (user == null)
        //{
        //    throw new ValidationException(ErrorCode.EntityDoesNotExist, new { Entity = "User" });
        //}

        ValidationHelper.MustExist(user);

        bool isValidPassword = await userManager.CheckPasswordAsync(user, model.Password);

        if (!isValidPassword)
        {
            throw new ValidationException(ErrorCode.InvalidCredentials);
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = jwtHelper.GenerateJwtToken(user.Id, user.Email, roles);

        return new AuthResponseModel
        {
            Token = token,
        };
    }

    public async Task<AuthResponseModel> Register(UserRegisterModel model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        var roless =  await db.Roles.ToListAsync();
        ValidationHelper.MustNotExist(user);

        var passwordValidator = new PasswordValidator<User>();
        var passwordValidationResult = await passwordValidator.ValidateAsync(userManager, null, model.Password);

        if (!passwordValidationResult.Succeeded)
        {
            throw new ValidationException(ErrorCode.RequirementsNotMet);
        }

        using var transaction = await db.Database.BeginTransactionAsync();
        var newUser = new User
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            UserName = model.Email,
            EmailConfirmed = false
        };
        
        try
        {
            var userCreateResult = await userManager.CreateAsync(newUser, model.Password);

            if (!userCreateResult.Succeeded)
            {
                throw new ValidationException(ErrorCode.IdentityError);
            }

            var roleResult = await userManager.AddToRoleAsync(newUser, UserRoleConstants.User);

            if (!roleResult.Succeeded)
            {
                throw new ValidationException(ErrorCode.IdentityError);
            }

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw new ValidationException(ErrorCode.IdentityError);
        }

        var code = userManager.GenerateEmailConfirmationTokenAsync(newUser);

        var roles = await userManager.GetRolesAsync(newUser);
        var token = jwtHelper.GenerateJwtToken(newUser.Id, newUser.Email, roles);

        return new AuthResponseModel
        {
            Token = token,
        };
    }

    public async Task ForgotPassword(ForgotPasswordModel model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        ValidationHelper.MustExist(user);

        var resetPasswordToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetPasswordToken));
        var resetPasswordUrl = $"{ANGULAR_APP_URL}/auth/reset-password/{user.Id}/{encodedToken})";
        await emailManager.SendForgotPasswordEmail(model.Email, resetPasswordUrl);
    }

    public async Task ResetPassword(ResetPasswordModel model)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == model.UserId);
        ValidationHelper.MustExist(user);

        //var userBeforeChanges = user.ShallowCopy();

        model.Token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
        var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);

        ValidationHelper.CheckIdentityResult(result, new ErrorOverrideModel
        {
            TextToOverride = "Invalid token.",
            OverrideWith = "This reset password link has already been used. Please go back to Log In page and start the process again."
        });

        //user.LoginSettings.UnsuccessfulLoginAttempts = 0;

        await db.SaveChangesAsync();
        //await platformUserManager.StoreChangeLog(userBeforeChanges, user, changeMadeByUserId: user.Id);
    }
}
