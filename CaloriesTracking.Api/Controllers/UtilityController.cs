using CaloriesTracking.Common.Helpers;
using CaloriesTracking.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CaloriesTracking.Api.Controllers;

[Route("api/utilities")]
[ApiController]
public class UtilityController : ControllerBase
{
    private readonly CaloriesTrackingDbContext db;

    public UtilityController(CaloriesTrackingDbContext db)
    {
        this.db = db;
    }

    [HttpPost]
    [Route("seed-data")]
    //[Authorize(UserRoleConstants.Administrator)]
    public async Task<IActionResult> SeedData()
    {
        await db.SeedData();
        return NoContent();
    }
}
