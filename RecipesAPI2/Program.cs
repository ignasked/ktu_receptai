using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using RecipesAPI2;
using RecipesAPI2.Data;
using RecipesAPI2.Auth.Model;
using RecipesAPI2.Data.Entities;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Results;
using SharpGrip.FluentValidation.AutoValidation.Shared.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using RecipesAPI2.Auth;
using Microsoft.AspNetCore.DataProtection;
/*using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;*/

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<RecipeDbContext>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation(configuration =>
{
    configuration.OverrideDefaultResultFactoryWith<ProblemDetailsResultFactory>();
});

builder.Services.AddDataProtection().
    PersistKeysToFileSystem(new DirectoryInfo("/keys"));

builder.Services.AddTransient<JwtTokenService>();
builder.Services.AddTransient<SessionService>();
builder.Services.AddScoped<AuthSeeder>();

builder.Services.AddIdentity<RecipeUser, IdentityRole>()
    .AddEntityFrameworkStores<RecipeDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters.ValidAudience = builder.Configuration["Jwt:ValidAudience"];
    options.TokenValidationParameters.ValidIssuer = builder.Configuration["Jwt:ValidIssuer"];
    options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]));
});

builder.Services.AddAuthorization();


var app = builder.Build();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<RecipeDbContext>();
dbContext.Database.Migrate();
var dbSeeder = scope.ServiceProvider.GetRequiredService<AuthSeeder>();
await dbSeeder.SeedAsync();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendOrigin", policy =>
    {
        policy.WithOrigins("http://localhost:8080").AllowAnyHeader().AllowAnyHeader();
    });
});
// Use CORS
app.UseCors("AllowFrontendOrigin");

app.AddAuthApi();
app.AddRecipeApi();
app.AddStepApi();
app.AddIngredientApi();

app.MapGet("/", () => "Hello World!");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();



public class ProblemDetailsResultFactory : IFluentValidationAutoValidationResultFactory
{
    public IResult CreateResult(EndpointFilterInvocationContext context, ValidationResult validationResult)
    {
        var problemDetails = new HttpValidationProblemDetails(validationResult.ToValidationProblemErrors())
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Unprocessable Entity",
            Status = StatusCodes.Status422UnprocessableEntity
        };
        return TypedResults.Problem(problemDetails);
    }
}

