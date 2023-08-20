using CaloriesTracking.Api.Auth;
using CaloriesTracking.Common.Helpers;
using CaloriesTracking.Common.Models.User;
using CaloriesTracking.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CaloriesTracking.Api.Controllers;
[Route("api/users")]
[ApiController]
public class UserController : BaseController
{
    private readonly CtUserManager ctUserManager;

    public UserController(CtUserManager ctUserManager)
    {
        this.ctUserManager = ctUserManager;
    }

    /// <summary>
    /// Updates user's calory preference
    /// </summary>
    /// <param name="model">UserCaloriesModel</param>
    [HttpPut("me/calories")]
    [Authorize(Policy = Policies.EmailConfirmed)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateCaloriesPreference(UserCaloriesModel model)
    {
        await ctUserManager.UpdateCaloriesPreference(GetCurrentUserId().Value, model);
        return NoContent();
    }

    /// <summary>
    /// Updates a user photo
    /// </summary>
    /// <param name="model">File to be uploaded</param>
    [HttpPut("me/photo")]
    [Consumes("multipart/form-data")]
    [Authorize(Policy = Policies.EmailConfirmed)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserPhotoModel))]
    public async Task<ActionResult<UserPhotoModel>> UpdatePhoto([FromForm] UserPhotoUploadModel model)
    {
        var userPhoto = await ctUserManager.UpdatePhoto(GetCurrentUserId().Value, model);
        return Ok(userPhoto);
    }

    /// <summary>
    /// Gets a user photo
    /// </summary>
    /// <returns>UserPhoto</returns>
    [HttpGet("me/photo")]
    [Authorize(Policy = Policies.EmailConfirmed)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserPhotoModel))]
    public async Task<ActionResult<UserPhotoModel>> GetPhoto()
    {
        var userPhoto = await ctUserManager.GetPhoto(GetCurrentUserId().Value);
        return Ok(userPhoto);
    }

    /// <summary>
    /// Updates user info
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("me")]
    [Authorize(Policy = Policies.EmailConfirmed)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateUserInfo(UserUpdateModel model)
    {
        await ctUserManager.UpdateUserInfo(GetCurrentUserId().Value, model);
        return NoContent();
    }

    /// <summary>
    /// Gets user informations
    /// </summary>
    /// <returns>User informations</returns>
    [HttpGet("me")]
    [Authorize(Policy = Policies.RegisteredUser)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserMeModel))]
    public async Task<ActionResult<UserMeModel>> GetUserInfo()
    {
        var user = await ctUserManager.GetUserInfo(GetCurrentUserId().Value);
        return Ok(user);
    }

    // ADMINISTRATOR ROLE LOGIC

    /// <summary>
    /// Gets users for administration
    /// </summary>
    /// <returns>Users</returns>
    [HttpGet("admin/all")]
    [Authorize(Policy = Policies.AdministratorUser)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserAdminModel>))]
    public async Task<ActionResult<List<UserAdminModel>>> GetUsers()
    {
        var users = await ctUserManager.GetUsers();
        return Ok(users);
    }

    /// <summary>
    /// Updates a user with a given Email
    /// </summary>
    /// <param name="email">User's email</param>
    /// <param name="model">UserAdminUpdateModel</param>
    [HttpPut("admin/{email}")]
    [Authorize(Policy = Policies.AdministratorUser)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateUser(string email, UserAdminUpdateModel model)
    {
        await ctUserManager.UpdateUser(email, model);
        return NoContent();
    }

    /// <summary>
    /// Deletes a user with a given email
    /// </summary>
    /// <param name="email">User's Email</param>
    [HttpDelete("admin/{email}")]
    [Authorize(Policy = Policies.AdministratorUser)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteUser(string email)
    {
        await ctUserManager.DeleteUser(email);
        return NoContent();
    }
}
