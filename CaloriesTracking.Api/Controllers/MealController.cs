using CaloriesTracking.Common.Models.Meal;
using CaloriesTracking.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaloriesTracking.Api.Controllers;

[Route("api/meals")]
[ApiController]
public class MealController : BaseController
{
    private readonly MealManager mealManager;

    public MealController(MealManager mealManager)
    {
        this.mealManager = mealManager;
    }

    /// <summary>
    /// Gives a list of user's meals
    /// </summary>
    /// <returns>Meal List</returns>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MealModel>))]
    public async Task<ActionResult<List<MealModel>>> GetUserMeals()
    {
        var mealsModels = await mealManager.GetUserMeals(GetCurrentUserId().Value);
        return Ok(mealsModels);
    }

    /// <summary>
    /// Creates a meal for a user
    /// </summary>
    /// <param name="model">MealCreateModel</param>
    /// <returns>Created meal with Id</returns>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MealModel))]
    public async Task<ActionResult<MealModel>> CreateMeal(MealCreateModel model)
    {
        var mealModel = await mealManager.Create(GetCurrentUserId().Value, model);
        return Ok(mealModel);
    }

    /// <summary>
    /// Gives a specific meal
    /// </summary>
    /// <param name="id">Meal's Id</param>
    /// <returns>Meal with specified Id</returns>
    [Authorize]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MealModel))]
    public async Task<ActionResult<MealModel>> GetMeal(Guid id)
    {
        var mealModel = await mealManager.Get(GetCurrentUserId().Value, id);
        return Ok(mealModel);
    }

    /// <summary>
    /// Updates a meal with a specified Id
    /// </summary>
    /// <param name="id">Meal's Id</param>
    /// <param name="model">MealUpdateModel</param>
    [Authorize]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateMeal(Guid id, MealUpdateModel model)
    {
        await mealManager.Update(GetCurrentUserId().Value, id, model);
        return NoContent();
    }

    /// <summary>
    /// Deletes a meal with a specified Id
    /// </summary>
    /// <param name="id">Meal's Id</param>
    [Authorize]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteMeal(Guid id)
    {
        await mealManager.Delete(id, GetCurrentUserId().Value);
        return NoContent();
    }
}
