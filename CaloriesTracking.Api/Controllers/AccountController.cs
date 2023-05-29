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
    /// Registeres a user 
    /// </summary>
    /// <param name="model">UserRegisterModel</param>
    /// <returns>AuthResponse-User informations</returns>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponseModel))]
    public async Task<ActionResult<AuthResponseModel>> Register(UserRegisterModel model)
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

    /// <summary>
    /// Refreshed the token
    /// </summary>
    /// <param name="model">AuthReponseModel</param>
    /// <returns>AuthResponse-User informations</returns>
    [HttpPost("refreshtoken")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponseModel))]
    public async Task<ActionResult<AuthResponseModel>> RefreshToken([FromBody] AuthResponseModel model)
    {
        var authResponse = await accountManager.VerifyRefreshToken(model);
        return Ok(authResponse);
    }

    ///// <summary>
    ///// Verifies email
    ///// </summary>
    ///// <param name="model">UserLoginModel</param>
    ///// <returns>AuthResponse-User informations</returns>
    //[HttpPost("verify")]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status204NoContent)]
    //public async Task<IActionResult> RefreshToken()
    //{
    //    await accountManager.EmailVerify(model);
    //    return NoContent();
    //}
}
