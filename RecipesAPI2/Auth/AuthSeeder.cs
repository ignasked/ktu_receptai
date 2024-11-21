
using Microsoft.AspNetCore.Identity;
using RecipesAPI2.Auth.Model;

namespace RecipesAPI2.Auth
{
    public class AuthSeeder
    {
        private readonly UserManager<RecipeUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthSeeder(UserManager<RecipeUser> userManager, RoleManager<IdentityRole> roleManager)
        { 
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task SeedAsync()
        {
            await AddDefaultRolesAsync();
            await AddAdminUserAsync();
        }

        private async Task AddDefaultRolesAsync()
        {
            foreach(var role in RecipeRoles.All)
            {
                var roleExists = await _roleManager.RoleExistsAsync(role);
                if (!roleExists)
                    await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
        private async Task AddAdminUserAsync()
        {
            var newAdminUser = new RecipeUser()
            {
                UserName = "admin4",
                Email = "admin@localhost.com"
            };

            var existingAdminUser = await _userManager.FindByNameAsync(newAdminUser.UserName);
            if (existingAdminUser == null)
            {
                var createAdminResult = await _userManager.CreateAsync(newAdminUser, "VerySafePassword1!");
                if (createAdminResult.Succeeded)
                {
                    await _userManager.AddToRolesAsync(newAdminUser, RecipeRoles.All);
                }
            }
        }        
    }
}
