using AutoMapper;
using CaloriesTracking.Common.Models.Meal;
using CaloriesTracking.Common.Models.User;
using CaloriesTracking.Entities;

namespace CaloriesTracking.Common;
public class MapperConfig : Profile
{
    public MapperConfig()
    {
        CreateMap<MealCreateModel, Meal>().ReverseMap();
        CreateMap<MealModel, Meal>().ReverseMap();
        CreateMap<MealUpdateModel, Meal>().ReverseMap();

        CreateMap<UserRegisterModel, User>().ReverseMap();
        CreateMap<UserLoginModel, User>().ReverseMap();
        CreateMap<UserCaloriesModel, User>().ReverseMap();
        CreateMap<UserUpdateModel, User>().ReverseMap();
        //CreateMap<User, UserMeModel>().ReverseMap();
        //CreateProjection<User, UserMeModel>()
        //    .ForMember(d => d.MealsCreated,
        //        opt => opt.MapFrom(c => c.Meals.Count()));
        CreateMap<User, UserMeModel>().ReverseMap();
        CreateMap<UserAdminUpdateModel, User>().ReverseMap();
        //
        CreateMap<UserCaloriesModel, UserMeModel>().ReverseMap();
        //
        CreateMap<UserRegisterModel, UserLoginModel>().ReverseMap();
    }

}
