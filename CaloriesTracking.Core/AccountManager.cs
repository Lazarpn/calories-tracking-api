using AutoMapper;
using CaloriesTracking.Common.Enums;
using CaloriesTracking.Common.Exceptions;
using CaloriesTracking.Common.Helpers;
using CaloriesTracking.Common.Models.User;
using CaloriesTracking.Data;
using CaloriesTracking.Entities;
using Microsoft.AspNetCore.Identity;
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
    private readonly JwtHelper jwtHelper;
    private readonly IMapper mapper;
    private readonly UserManager<User> userManager;
    private readonly IConfiguration configuration;

    public AccountManager(
        IMapper mapper,
        UserManager<User> userManager,
        IConfiguration configuration,
        CaloriesTrackingDbContext db)
    {
        this.mapper = mapper;
        this.configuration = configuration;
        this.userManager = userManager;
        this.db = db;
        this.jwtHelper = new JwtHelper(configuration);
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
        //var userInfo = mapper.Map<UserMeModel>(user);

        // TODO: nije li ovo nepotreban query i projectTo jer vec imam user-a, mogao sam samo new Model?
        var userQuery = db.Users.Where(u => u.Id == user.Id);
        var userInfo = await mapper.ProjectTo<UserMeModel>(userQuery).FirstAsync();

        return new AuthResponseModel
        {
            Token = token,
            UserId = user.Id,
            User = userInfo
        };
    }

    public async Task<AuthResponseModel> Register(UserRegisterModel model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
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
            UserName = model.Email
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

        var roles = await userManager.GetRolesAsync(newUser);
        var token = jwtHelper.GenerateJwtToken(newUser.Id, newUser.Email, roles);
        var userInfo = new UserMeModel
        {
            FirstName = newUser.FirstName,
            LastName = newUser.LastName,
            Email = newUser.Email,
        };

        return new AuthResponseModel
        {
            Token = token,
            UserId = newUser.Id,
            User = userInfo
        };
    }
}
