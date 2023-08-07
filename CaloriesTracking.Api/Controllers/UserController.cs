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
    [Authorize]
    [HttpPut("me/calories")]
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
    [Authorize]
    [HttpPut("me/photo")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserPhotoModel))]
    public async Task<ActionResult<UserPhotoModel>> UpdatePhoto([FromForm] UserPhotoUploadModel model)
    {
        var userPhoto = await ctUserManager.UpdatePhoto(GetCurrentUserId().Value, model);
        return Ok(userPhoto);
    }

    /// <summary>
    /// Updates user info
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateUserInfo(UserUpdateModel model)
    {
        await ctUserManager.UpdateUserInfo(GetCurrentUserId().Value, model);
        return NoContent();
    }

    /// <summary>
    /// Gets a user photo
    /// </summary>
    /// <returns>UserPhoto</returns>
    [Authorize]
    [HttpGet("me/photo")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserPhotoModel))]
    public async Task<ActionResult<UserPhotoModel>> GetPhoto()
    {
        var userPhoto = await ctUserManager.GetPhoto(GetCurrentUserId().Value);
        return Ok(userPhoto);
    }

    /// <summary>
    /// Gets user informations
    /// </summary>
    /// <returns>User informations</returns>
    //[Authorize]
    [HttpGet("me")]
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
    //[Authorize(Roles = UserRoleConstants.Administrator)]
    [HttpGet("admin/all")]
    //[ProducesResponseType(StatusCodes.Status403Forbidden)]
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
    [Authorize(Roles = UserRoleConstants.Administrator)]
    [HttpPut("admin/{email}")]
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
    [Authorize(Roles = UserRoleConstants.Administrator)]
    [HttpDelete("admin/{email}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteUser(string email)
    {
        await ctUserManager.DeleteUser(email);
        return NoContent();
    }
}
