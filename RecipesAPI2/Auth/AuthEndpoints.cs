using Microsoft.AspNetCore.Identity;
using RecipesAPI2.Auth.Model;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using RecipesAPI2.Data;

namespace RecipesAPI2.Auth
{
    public static class AuthEndpoints
    {

        public static void AddAuthApi(this WebApplication app)
        {
            //register
            app.MapPost("api/accounts", async(UserManager<RecipeUser> userManager, RecipeDbContext recipesDbContext, RegisterUserDto dto) =>
            { 
                //check if user exists
                var user = await userManager.FindByNameAsync(dto.Username);
                if (user != null)
                    return Results.UnprocessableEntity("Username already taken");

                var newUser = new RecipeUser()
                {
                    UserName = dto.Username,
                    Email = dto.Email,
                };

                using (var transaction = recipesDbContext.Database.BeginTransaction())
                {
                    var createUserResult = await userManager.CreateAsync(newUser, dto.Password);
                    if (!createUserResult.Succeeded)
                    {
                        transaction.Rollback();
                        return Results.UnprocessableEntity();
                    }

                    await userManager.AddToRoleAsync(newUser, RecipeRoles.RecipesUser);
                    transaction.Commit();
                }
                return Results.Created();
            });

            //login
            app.MapPost("api/login", async (UserManager<RecipeUser> userManager,JwtTokenService jwtTokenService,SessionService sessionService,HttpContext httpContext, LoginUserDto dto) =>
            {
                Console.WriteLine("Login username: " + dto.Username);
                //check if user exists
                var user = await userManager.FindByNameAsync(dto.Username);
                
                if (user == null)
                    return Results.UnprocessableEntity("User with this username does not exist");

                var isPasswordValid = await userManager.CheckPasswordAsync(user, dto.Password);
                if(!isPasswordValid)
                    return Results.UnprocessableEntity("Username or password is incorrect");             

                var roles = await userManager.GetRolesAsync(user);

                var sessionId = Guid.NewGuid();
                var expiresAt = DateTime.SpecifyKind(DateTime.Now.AddDays(3), DateTimeKind.Utc);
                var accessToken = jwtTokenService.CreateAccessToken(user.UserName, user.Id, roles);
                var refreshToken = jwtTokenService.CreateRefreshToken(sessionId ,user.Id, expiresAt);

                await sessionService.CreateSessionAsync(sessionId, user.Id, refreshToken, expiresAt);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = expiresAt,
                    SameSite = SameSiteMode.None,
                    Secure = false //TODO: set to true in production
                };

                httpContext.Response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);

                return Results.Ok(new SuccessfulLoginDto(accessToken));
            });

            app.MapPost("api/accessToken", async (UserManager<RecipeUser> userManager, JwtTokenService jwtTokenService, SessionService sessionService, HttpContext httpContext) =>
            {
                Console.WriteLine("--api/accessToken");
                foreach (var cookie in httpContext.Request.Cookies)
                {
                    Console.WriteLine($"Cookie: {cookie.Key} = {cookie.Value}");
                }
                if (!httpContext.Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
                {
                    return Results.UnprocessableEntity();
                }
                Console.WriteLine("RefreshToken: ",refreshToken);
                if (!jwtTokenService.TryParseRefreshToken(refreshToken, out var claims))
                {
                    return Results.UnprocessableEntity();
                }

                var sessionId = claims.FindFirstValue("SessionId");
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    return Results.UnprocessableEntity();
                }

                var sessionIdAsGuid = Guid.Parse(sessionId);
                if (!await sessionService.IsSessionValidAsync(sessionIdAsGuid, refreshToken))
                {
                    return Results.UnprocessableEntity();
                }

                var userId = claims.FindFirstValue(JwtRegisteredClaimNames.Sub);
                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Results.UnprocessableEntity();
                }

                var roles = await userManager.GetRolesAsync(user);

                var expiresAt = DateTime.SpecifyKind(DateTime.Now.AddDays(3), DateTimeKind.Utc);
                var accessToken = jwtTokenService.CreateAccessToken(user.UserName, user.Id, roles);
                var newRefreshToken = jwtTokenService.CreateRefreshToken(sessionIdAsGuid, user.Id, expiresAt);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.None,
                    Expires = expiresAt,
                    Secure = false //=> should be true possibly
                };

                httpContext.Response.Cookies.Append("RefreshToken", newRefreshToken, cookieOptions);

                await sessionService.ExtendSessionAsync(sessionIdAsGuid, newRefreshToken, expiresAt);

                return Results.Ok(new SuccessfulLoginDto(accessToken));
            });

            app.MapPost("api/logout", async (UserManager<RecipeUser> userManager, JwtTokenService jwtTokenService, SessionService sessionService, HttpContext httpContext) =>
            {

                if (!httpContext.Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
                {
                    return Results.UnprocessableEntity();
                }

                if (!jwtTokenService.TryParseRefreshToken(refreshToken, out var claims))
                {
                    return Results.UnprocessableEntity();
                }

                var sessionId = claims.FindFirstValue("SessionId");
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    return Results.UnprocessableEntity();
                }

                await sessionService.InvalidateSessionAsync(Guid.Parse(sessionId));
                httpContext.Response.Cookies.Delete("RefreshToken");

                return Results.Ok();
            });

        }

        public record RegisterUserDto(string Username, string Password, string Email);
        public record LoginUserDto(string Username, string Password);
        public record SuccessfulLoginDto(string AccessToken);
    }
}
