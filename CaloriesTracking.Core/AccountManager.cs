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

        var token = await GenerateToken(user);
        //var userInfo = mapper.Map<UserMeModel>(user);

        // TODO: nije li ovo nepotreban query i projectTo jer vec imam user-a, mogao sam samo new Model?
        var userQuery = db.Users.Where(u => u.Id == user.Id);
        var userInfo = await mapper.ProjectTo<UserMeModel>(userQuery).FirstAsync();

        return new AuthResponseModel
        {
            RefreshToken = await CreateRefreshToken(user.Id),
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

        var token = await GenerateToken(newUser);
        var userInfo = new UserMeModel
        {
            FirstName = newUser.FirstName,
            LastName = newUser.LastName,
            Email = newUser.Email,
        };

        return new AuthResponseModel
        {
            RefreshToken = await CreateRefreshToken(newUser.Id),
            Token = token,
            UserId = newUser.Id,
            User = userInfo
        };
    }

    //public async Task<AuthResponseModel> EmailVerify(model)
    //{
    //    //model.VerifiedAt = DateTime.UtcNow;
    //    //User.EmailVerified

    //    return new AuthResponseModel
    //    {
    //        RefreshToken = await CreateRefreshToken(newUser.Id),
    //        Token = token,
    //        UserId = newUser.Id,
    //        User = userInfo
    //    };
    //}

    private async Task<string> GenerateToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:jwtKey"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var roles = await userManager.GetRolesAsync(user);
        var rolesClaims = roles.Select(x => new Claim("role", x)).ToList();
        var userClaims = await userManager.GetClaimsAsync(user);
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
        }.Union(userClaims).Union(rolesClaims);

        var token = new JwtSecurityToken(
            issuer: configuration["JwtSettings:jwtIssuer"],
            audience: configuration["JwtSettings:jwtAudience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToInt32(configuration["JwtSettings:durationInMinutes"])),
            signingCredentials: credentials
           );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> CreateRefreshToken(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        // TODO: videti da li treba ovde da se cuva rezultat i proverava za exception
        await userManager.RemoveAuthenticationTokenAsync(user, configuration["JwtSettings:jwtIssuer"], "RefreshToken");

        var newRefreshToken = await userManager.GenerateUserTokenAsync(user, configuration["JwtSettings:jwtIssuer"], "RefreshToken");
        var result = await userManager.SetAuthenticationTokenAsync(user, configuration["JwtSettings:jwtIssuer"], "RefreshToken", newRefreshToken);

        if (result == null)
        {
            throw new ValidationException(ErrorCode.IdentityError);
        }

        return newRefreshToken;
    }

    public async Task<AuthResponseModel> VerifyRefreshToken(AuthResponseModel model)
    {
        var user = await userManager.FindByIdAsync(model.UserId.ToString());
        ValidationHelper.MustExist(user);

        var isValidRefreshToken = await userManager.VerifyUserTokenAsync(user, configuration["JwtSettings:jwtIssuer"], "RefreshToken", model.RefreshToken);

        if (!isValidRefreshToken)
        {
            await userManager.UpdateSecurityStampAsync(user);
            // TODO: jel mi zapravo uopste treba exception ovde?
            throw new ValidationException(ErrorCode.IdentityError);
        }

        var newToken = await GenerateToken(user);
        var userInfo = new UserMeModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            CaloriesPreference = user.CaloriesPreference,
            UserPhotoByte = user.UserPhoto
        };
        var authResponse = new AuthResponseModel
        {
            RefreshToken = await CreateRefreshToken(user.Id),
            Token = newToken,
            UserId = user.Id,
            User = userInfo
        };

        return authResponse;
    }
}
