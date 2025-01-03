﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RecipesAPI2.Auth.Model;
using RecipesAPI2.Data;
using RecipesAPI2.Data.Entities;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;

namespace RecipesAPI2
{
    public static class Endpoints
    {
        public static void AddRecipeApi(this WebApplication app)
        {
            var recipeGroups = app.MapGroup("/api").AddFluentValidationAutoValidation();
            recipeGroups.MapGet("/recipes", async (RecipeDbContext db) =>
            {
                return (await db.Recipes.ToListAsync()).Select(o => o.ToDto());
            });
            recipeGroups.MapPost("/recipes", [Authorize(Roles = RecipeRoles.RecipesUser)] async (CreateRecipeDto dto, RecipeDbContext db, HttpContext httpContext) =>
            {
                var recipe = new Recipe { Title = dto.Title, Description = dto.Description, Rating = 5 , UserId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) };
                db.Recipes.Add(recipe);

                await db.SaveChangesAsync();

                return TypedResults.Created($"/api/recipes/{recipe.Id}", recipe.ToDto());
            });
            recipeGroups.MapGet("/recipes/{recipeId}", async (int recipeId, RecipeDbContext db) =>
            {
                var recipe = await db.Recipes.FindAsync(recipeId);
                return recipe == null ? Results.NotFound() : TypedResults.Ok(recipe.ToDto());
            });
            recipeGroups.MapPut("/recipes/{recipeId}",[Authorize] async (int recipeId, UpdateRecipeDto dto, RecipeDbContext db, HttpContext httpContext) =>
            {
                var recipe = await db.Recipes.FindAsync(recipeId);

                if (recipe == null)
                {
                    return Results.NotFound();
                }
                if (httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != recipe.UserId && !httpContext.User.IsInRole(RecipeRoles.Admin))
                    return Results.Forbid();

                recipe.Description = dto.Description;
                db.Recipes.Update(recipe);

                await db.SaveChangesAsync();

                return Results.Ok(recipe.ToDto());
            });
            recipeGroups.MapDelete("/recipes/{recipeId}", [Authorize] async (int recipeId, RecipeDbContext db, HttpContext httpContext) =>
            {
                var recipe = await db.Recipes.FindAsync(recipeId);

                if (recipe == null)
                {
                    return Results.NotFound();
                }
                if (!httpContext.User.IsInRole(RecipeRoles.Admin) && httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != recipe.UserId)
                    return Results.Forbid();

                db.Recipes.Remove(recipe);

                await db.SaveChangesAsync();

                return Results.NoContent();
            });
        }

        public static void AddStepApi(this WebApplication app)
        {
            var stepGroups = app.MapGroup("/api/recipes/{recipeId}").AddFluentValidationAutoValidation();

            ///recipes/{recipeId}/steps
            stepGroups.MapGet("/steps", async (int recipeId, RecipeDbContext db) =>
            {
                var recipe = await db.Recipes.FindAsync(recipeId);
                if(recipe == null)
                    return Results.NotFound();

                var steps = (await db.Steps.Where(o => o.Recipe.Id == recipeId).ToListAsync()).Select(o => o.ToDto());

                if(steps == null)
                {
                    return  Results.NotFound();
                }
                else
                {
                    return TypedResults.Ok(steps);
                }

            });
            stepGroups.MapPost("/steps", [Authorize(Roles = RecipeRoles.RecipesUser)] async (int recipeId, CreateStepDto dto, RecipeDbContext db, HttpContext httpContext) =>
            {
                var recipe = await db.Recipes.FindAsync(recipeId);

                if (recipe == null)
                {
                    return Results.NotFound();
                }
                else
                { 
                    if(!httpContext.User.IsInRole(RecipeRoles.Admin) && httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != recipe.UserId)
                        return Results.Forbid();

                    var step = new Step { Text = dto.Text, Duration = dto.Duration, Recipe = recipe , UserId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)};

                    db.Steps.Add(step);

                    await db.SaveChangesAsync();

                    return TypedResults.Created($"/api/recipes/{recipe.Id}/steps/{step.Id}", step.ToDto());
                }
            });
            stepGroups.MapGet("/steps/{stepId}", async (int recipeId,int stepId, RecipeDbContext db) =>
            {
                var step = await db.Steps
                .Include(s => s.Recipe) // Ensure Recipe is loaded with Step
                .FirstOrDefaultAsync(s => s.Id == stepId);

                if (step == null)
                {
                    return Results.NotFound();
                }
                else
                {
                    var recipe = await db.Recipes.FindAsync(recipeId);

                    if(recipe == null || step.Recipe.Id != recipeId)
                        return Results.NotFound();
                    else
                        return TypedResults.Ok(step.ToDto());
                }
            });
            stepGroups.MapPut("/steps/{stepId}",[Authorize] async (int recipeId,int stepId, UpdateStepDto dto, RecipeDbContext db, HttpContext httpContext) =>
            {
                var recipe = await db.Recipes.FindAsync(recipeId);
                if (recipe == null)
                    return Results.NotFound();
                if (!httpContext.User.IsInRole(RecipeRoles.Admin) && httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != recipe.UserId)
                    return Results.Forbid();

                var step = await db.Steps
                .Include(s => s.Recipe) // Ensure Recipe is loaded with Step
                .FirstOrDefaultAsync(s => s.Id == stepId);
                if (step == null || step.Recipe.Id != recipeId)
                    return Results.NotFound();

                step.Text = dto.Text;
                step.Duration = dto.Duration;
                db.Steps.Update(step);
                await db.SaveChangesAsync();

                return Results.Ok(step.ToDto());
            });
            stepGroups.MapDelete("/steps/{stepId}", [Authorize] async (int recipeId, int stepId, RecipeDbContext db, HttpContext httpContext) =>
            {
                var recipe = await db.Recipes.FindAsync(recipeId);
                if (recipe == null)
                    return Results.NotFound();

                var step = await db.Steps
                .Include(s => s.Recipe) // Ensure Recipe is loaded with Step
                .FirstOrDefaultAsync(s => s.Id == stepId);
                if (step == null || step.Recipe.Id != recipeId)
                    return Results.NotFound();
                if (!httpContext.User.IsInRole(RecipeRoles.Admin) && httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != step.UserId)
                    return Results.Forbid();

                db.Steps.Remove(step);
                await db.SaveChangesAsync();

                return Results.NoContent();
            });
        }

        public static void AddIngredientApi(this WebApplication app)
        {
            var ingredientGroups = app.MapGroup("/api/recipes/{recipeId}/steps/{stepId}").AddFluentValidationAutoValidation();

            ingredientGroups.MapGet("/ingredients", async (int recipeId, int stepId, RecipeDbContext db) =>
            {
                var recipe = await db.Recipes.FindAsync(recipeId);
                if (recipe == null)
                    return Results.NotFound();

                var step = await db.Steps
                .Include(s => s.Recipe) // Ensure Recipe is loaded with Step
                .FirstOrDefaultAsync(s => s.Id == stepId);
                if (step == null || step.Recipe.Id != recipeId)
                    return Results.NotFound();

                var ingredients = (await db.Ingredients.Where(o => o.Step.Id == stepId).ToListAsync()).Select(o => o.ToDto());

                if(ingredients == null)
                    return Results.NotFound();
                else
                    return TypedResults.Ok(ingredients);
            });
            ingredientGroups.MapPost("/ingredients", [Authorize(Roles = RecipeRoles.RecipesUser)] async (int recipeId, int stepId, CreateIngredientDto dto, RecipeDbContext db, HttpContext httpContext) =>
            {
                var recipe = await db.Recipes.FindAsync(recipeId);
                if (recipe == null)
                    return Results.NotFound();
                if (!httpContext.User.IsInRole(RecipeRoles.Admin) && httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != recipe.UserId)
                    return Results.Forbid();

                var step = await db.Steps
                .Include(s => s.Recipe) // Ensure Recipe is loaded with Step
                .FirstOrDefaultAsync(s => s.Id == stepId);
                if (step == null || step.Recipe.Id != recipeId)
                    return Results.NotFound();
                if (!httpContext.User.IsInRole(RecipeRoles.Admin) && httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != step.UserId)
                    return Results.Forbid();

                var ingredient = new Ingredient{ Name = dto.Name, Amount = dto.Amount, Unit = dto.Unit, Step = step , UserId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)};
                db.Ingredients.Add(ingredient);
                await db.SaveChangesAsync();

                return TypedResults.Created($"/api/recipes/{recipe.Id}/steps/{step.Id}/ingredients/{ingredient.Id}", ingredient.ToDto());

            });
            ingredientGroups.MapGet("/ingredients/{ingredientId}", async (int recipeId, int stepId, int ingredientId, RecipeDbContext db) =>
            {
                var recipe = await db.Recipes.FindAsync(recipeId);
                if (recipe == null)
                    return Results.NotFound();

                var step = await db.Steps
                .Include(s => s.Recipe) // Ensure Recipe is loaded with Step
                .FirstOrDefaultAsync(s => s.Id == stepId);
                if (step == null || step.Recipe.Id != recipeId)
                    return Results.NotFound();

                var ingredient = await db.Ingredients
                .Include(s => s.Step) // Ensure Step is loaded with Step
                .FirstOrDefaultAsync(s => s.Id == ingredientId);
                if (ingredient == null || ingredient.Step.Id != stepId) 
                    return Results.NotFound();

                return TypedResults.Ok(ingredient.ToDto());
            });
            ingredientGroups.MapPut("/ingredients/{ingredientId}", [Authorize] async (int recipeId, int stepId,int ingredientId, UpdateIngredientDto dto, RecipeDbContext db, HttpContext httpContext) =>
            {
                var recipe = await db.Recipes.FindAsync(recipeId);
                if (recipe == null)
                    return Results.NotFound();
                if (!httpContext.User.IsInRole(RecipeRoles.Admin) && httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != recipe.UserId)
                    return Results.Forbid();

                var step = await db.Steps
                .Include(s => s.Recipe) // Ensure Recipe is loaded with Step
                .FirstOrDefaultAsync(s => s.Id == stepId);
                if (step == null || step.Recipe.Id != recipeId)
                    return Results.NotFound();
                if (!httpContext.User.IsInRole(RecipeRoles.Admin) && httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != step.UserId)
                    return Results.Forbid();

                var ingredient = await db.Ingredients
                .Include(s => s.Step) // Ensure Step is loaded with Step
                .FirstOrDefaultAsync(s => s.Id == ingredientId);
                if (ingredient == null || ingredient.Step.Id != stepId)
                    return Results.NotFound();
                if (!httpContext.User.IsInRole(RecipeRoles.Admin) && httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != ingredient.UserId)
                    return Results.Forbid();

                ingredient.Name = dto.Name;
                ingredient.Amount = dto.Amount;
                ingredient.Unit = dto.Unit;
                db.Ingredients.Update(ingredient);
                await db.SaveChangesAsync();

                return Results.Ok(ingredient.ToDto());
            });
            ingredientGroups.MapDelete("/ingredients/{ingredientId}",[Authorize] async (int recipeId, int stepId, int ingredientId, RecipeDbContext db, HttpContext httpContext) =>
            {
                var recipe = await db.Recipes.FindAsync(recipeId);
                if (recipe == null)
                    return Results.NotFound();
                if (!httpContext.User.IsInRole(RecipeRoles.Admin) && httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != recipe.UserId)
                    return Results.Forbid();

                var step = await db.Steps
                .Include(s => s.Recipe) // Ensure Recipe is loaded with Step
                .FirstOrDefaultAsync(s => s.Id == stepId); ;
                if (step == null || step.Recipe.Id != recipeId)
                    return Results.NotFound();
                if (!httpContext.User.IsInRole(RecipeRoles.Admin) && httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != step.UserId)
                    return Results.Forbid();

                var ingredient = await db.Ingredients
                .Include(s => s.Step) // Ensure Step is loaded with Step
                .FirstOrDefaultAsync(s => s.Id == ingredientId);
                if (ingredient == null || ingredient.Step.Id != stepId)
                    return Results.NotFound();
                if (!httpContext.User.IsInRole(RecipeRoles.Admin) && httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != ingredient.UserId)
                    return Results.Forbid();

                db.Ingredients.Remove(ingredient);
                await db.SaveChangesAsync();

                return Results.NoContent();
            });
        }

    }
}
