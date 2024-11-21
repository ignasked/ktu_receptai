using Microsoft.EntityFrameworkCore;
using RecipesAPI2.Data.Entities;
using RecipesAPI2.Auth.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace RecipesAPI2.Data
{
    public class RecipeDbContext : IdentityDbContext<RecipeUser>
    {
        private readonly IConfiguration _configuration;
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Step> Steps { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Session> Sessions { get; set; }

        public RecipeDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("PostgreSQL"));
        }
    }
}
