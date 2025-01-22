using Azure.Storage.Blobs;
using CaloriesTracking.Api.ActionResults;
using CaloriesTracking.Api.Helpers;
using CaloriesTracking.Common.Helpers;
using CaloriesTracking.Common.Middleware;
using CaloriesTracking.Core;
using CaloriesTracking.Data;
using CaloriesTracking.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using NLog.Web;
using SendGrid.Extensions.DependencyInjection;
using System.Globalization;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Azure;
using Microsoft.AspNetCore.Authorization;
using CaloriesTracking.Api.Auth;
using System.Net;
using Microsoft.AspNetCore.Hosting;

Logger logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Info("App: Starting Application");

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    logger.Info("App: Configuring identity");
    builder.Services.AddIdentity<User, IdentityRole<Guid>>()
        .AddTokenProvider<DataProtectorTokenProvider<User>>(builder.Configuration["jwtIssuer"])
        .AddEntityFrameworkStores<CaloriesTrackingDbContext>()
        .AddDefaultTokenProviders();

    //builder.Services.AddAuthentication().AddGoogle(googleOptions =>
    //{
    //    googleOptions.ClientId = builder.Configuration["Google:clientId"];
    //    googleOptions.ClientSecret = builder.Configuration["Google:clientSecret"];
    //});

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["jwtIssuer"],
            ValidateIssuer = true,
            ValidAudience = builder.Configuration["jwtAudience"],
            ValidateAudience = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["jwtKey"])),
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true
        };
    });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(Policies.NotConfirmedEmail, policy => policy.Requirements.Add(new NotConfirmedEmailRequirement()));
        options.AddPolicy(Policies.EmailConfirmed, policy => policy.Requirements.Add(new EmailConfirmedRequirement()));
        options.AddPolicy(Policies.RegisteredUser, policy => policy.Requirements.Add(new RegisteredUserRequirement()));
        options.AddPolicy(Policies.AdministratorUser, policy => policy.Requirements.Add(new AdministratorUserRequirement()));

    });

    logger.Info("App: Configuring authorization policies");
    builder.Services.AddScoped<IAuthorizationHandler, NotConfirmedEmailHandler>();
    builder.Services.AddScoped<IAuthorizationHandler, EmailConfirmedHandler>();
    builder.Services.AddScoped<IAuthorizationHandler, RegisteredUserHandler>();
    builder.Services.AddScoped<IAuthorizationHandler, AdministratorUserHandler>();

    logger.Info("App: Configuring services");
    builder.Services.AddScoped<AccountManager>();
    builder.Services.AddScoped<CtUserManager>();
    builder.Services.AddScoped<MealManager>();
    //builder.Services.AddScoped<FileManager>();
    builder.Services.AddScoped<UserActivityManager>();

    logger.Info("App: Configuring forwarded headers");
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    });


    logger.Info("App: Configuring CORS");
    var allowedDomains = builder.Configuration["allowedDomains"].Split(',');

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy",
            policyBuilder =>
            {
                policyBuilder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("refreshed-token")
                    .WithOrigins(allowedDomains);
                policyBuilder.SetPreflightMaxAge(TimeSpan.FromSeconds(600));
            });
    });

    logger.Info("App: Configuring AutoMapper");
    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

    logger.Info("App: Configuring DB Connections");
    builder.Services.AddDbContext<CaloriesTrackingDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("CaloriesTrackingDb")));

    logger.Info("App: Configuring SendGrid");
    //builder.Services.AddSendGrid(options =>
    //{
    //    options.ApiKey = builder.Configuration["SendGridApiKey"];
    //});


    logger.Info("App: Configuring Core services");
    //builder.Services.AddScoped<SendGridEmailManager>();

    logger.Info("App: Configuring web api / controllers");
    builder.Services.AddMemoryCache();

    builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
        options.SerializerSettings.Culture = new CultureInfo("en-GB");
        options.SerializerSettings.Converters.Add(new StringEnumConverter());
    });

    logger.Info("App: Configuring form options");
    builder.Services.Configure<FormOptions>(options =>
    {
        options.ValueLengthLimit = 52428800; // 50MB
        options.MultipartBodyLengthLimit = 52428800; // 50MB
    });

    logger.Info("App: Configuring validations");
    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.InvalidModelStateResponseFactory = _ => new ValidationProblemDetailsResult();
    });

    logger.Info("App: Configuring NLog DI");
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();

    logger.Info("App: Configuring Swagger");
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "CaloriesTracking API", Version = "v1" });
        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Name = "Authorization",
            Description = "Standard Authorization header using the Bearer scheme. Note: Keyword 'bearer' will be automatically prepended.",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });
        options.OperationFilter<SecurityRequirementsOperationFilter>();
        options.DescribeAllParametersInCamelCase();

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);
    });

    WebApplication app = builder.Build();

    logger.Info("App: Middleware pipeline start");
    if (builder.Environment.IsProduction())
    {
        app.UseHsts();
    }

    var cultureInfo = new CultureInfo("en-GB");
    CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
    CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

    app.UseStatusCodePages();

    app.UseStaticFiles();

    app.UseRouting();
    app.UseCors("CorsPolicy");

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<SessionManagementMiddleware>();

    bool shouldShowSwagger = bool.Parse(builder.Configuration["showSwagger"]);
    if (shouldShowSwagger)
    {
        app.UseSwagger();
        app.UseSwaggerUI(uiOptions =>
        {
            uiOptions.SwaggerEndpoint("/swagger/v1/swagger.json", "CalorieseTracking API V1");
            uiOptions.InjectStylesheet("/swagger-ui/styles.css");
        });
    }

    //StudyStreamDbContext.SeedData(dbContext, userManager, roleManager).Wait();

    if (builder.Environment.IsProduction())
    {
        app.UseHttpsRedirection();
    }

    app.MapControllers().RequireCors("CorsPolicy");

    logger.Info("App: Middleware pipeline end");

    app.Run();
    logger.Info("App: Running the Application");
}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "App: Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}



