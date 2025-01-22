using AutoMapper;
using AutoMapper.QueryableExtensions;
using CaloriesTracking.Common.Helpers;
using CaloriesTracking.Common.Models.Meal;
using CaloriesTracking.Data;
using CaloriesTracking.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaloriesTracking.Core;
public class MealManager
{
    private readonly IMapper mapper;
    private readonly CaloriesTrackingDbContext db;

    public MealManager(IMapper mapper, CaloriesTrackingDbContext db)
    {
        this.mapper = mapper;
        this.db = db;
    }

    public async Task<MealModel> Get(Guid currentUserId, Guid id)
    {
        var mealExists = await db.Meals.AnyAsync(m => m.Id == id && m.UserId == currentUserId);
        ValidationHelper.MustExist<Meal>(mealExists);

        var mealQuery = db.Meals.Where(m => m.Id == id);
        var mealModel = await mapper.ProjectTo<MealModel>(mealQuery).FirstAsync();

        return mealModel;
    }

    public async Task<MealModel> Create(Guid userId, MealCreateModel model)
    {
        var meal = new Meal
        {
            Calories = model.Calories,
            Name = model.Name,
            Date = model.Date,
            UserId = userId
        };

        await db.Meals.AddAsync(meal);
        await db.SaveChangesAsync();

        var mealModel = new MealModel
        {
            Id = meal.Id,
            Name = model.Name,
            Date = model.Date,
            Calories = model.Calories
        };

        return mealModel;
    }

    public async Task Update(Guid currentUserId, Guid id, MealUpdateModel mealUpdate)
    {
        var meal = await db.Meals.FirstOrDefaultAsync(m => m.Id == id && m.UserId == currentUserId);
        ValidationHelper.MustExist(meal);

        meal.Name = mealUpdate.Name;
        meal.Calories = mealUpdate.Calories;
        meal.Date = mealUpdate.Date;

        await db.SaveChangesAsync();
    }

    public async Task Delete(Guid id, Guid currentUserId)
    {
        var meal = await db.Meals.FirstOrDefaultAsync(m => m.Id == id && m.UserId == currentUserId);
        ValidationHelper.MustExist(meal);
        db.Meals.Remove(meal);
        await db.SaveChangesAsync();
    }

    public async Task<List<MealModel>> GetUserMeals(Guid id)
    {
        var mealsQuery = db.Meals.Where(meal => meal.UserId == id);
        var mealsModels = await mapper.ProjectTo<MealModel>(mealsQuery).ToListAsync();
        return mealsModels;
    }
}
