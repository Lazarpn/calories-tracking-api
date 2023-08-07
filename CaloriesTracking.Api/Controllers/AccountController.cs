using CaloriesTracking.Common.Helpers;
using CaloriesTracking.Common.Models.User;
using CaloriesTracking.Core;
using Microsoft.AspNetCore.Mvc;

namespace CaloriesTracking.Api.Controllers;

[Route("api/accounts")]
[ApiController]
public class AccountController : BaseController
{
    private readonly AccountManager accountManager;

    public AccountController(AccountManager accountManager)
    {
        this.accountManager = accountManager;
    }

    /// <summary>
    /// Registers a user 
    /// </summary>
    /// <param name="model">UserRegisterModel</param>
    /// <returns>AuthResponse-User informations</returns>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Register(UserRegisterModel model)
    {
        var authResponse = await accountManager.Register(model);
        return Ok(authResponse);
    }

    /// <summary>
    /// Logs a user
    /// </summary>
    /// <param name="model">UserLoginModel</param>
    /// <returns>AuthResponse-User informations</returns>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponseModel))]
    public async Task<ActionResult<AuthResponseModel>> Login([FromBody] UserLoginModel model)
    {
        var authResponse = await accountManager.Login(model);
        return Ok(authResponse);
    }

    //[HttpGet("confirm-email")]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status204NoContent)]
    //public async Task<IActionResult> ConfirmEmail(string code)
    //{
    //    return Ok();
    //}

    ///<summary>
    /// Starts the password reset process by sending a forgot password email
    /// </summary>
    /// <param name="model"></param>
    [HttpPut("password/forgot")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
    {
        await accountManager.ForgotPassword(model);
        return NoContent();
    }

    /// <summary>
    /// Verifies and completes the reset password process
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("password/reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
    {
        await accountManager.ResetPassword(model);
        return NoContent();
    }
}
