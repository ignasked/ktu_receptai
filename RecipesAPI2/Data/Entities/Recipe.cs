using FluentValidation;
using System.ComponentModel.DataAnnotations;
using RecipesAPI2.Auth.Model;

namespace RecipesAPI2.Data.Entities
{
    public class Recipe
    {
        public int Id { get; set; }
        public required string Title { get; set; }

        public required string Description { get; set; }

        public required double Rating { get; set; }

        [Required]
        public required string UserId { get; set; }

        public RecipeUser user { get; set; }

        public RecipeDto ToDto()
        {
            return new RecipeDto(Id, Title, Description, Rating);
        }   
    }

    public record RecipeDto(int Id, string Title, string Description, double Rating);
    public record CreateRecipeDto(string Title, string Description)
    {
        public class CreateRecipeDTOValidator : AbstractValidator<CreateRecipeDto>
        {
            public CreateRecipeDTOValidator()
            {
                RuleFor(x => x.Title).NotEmpty().Length(min: 2, max: 100);
                RuleFor(x => x.Description).NotEmpty().Length(min: 5, max: 500);
            }
        }
    };
    public record UpdateRecipeDto(string Description)
    {
        public class UpdateRecipeDTOValidator : AbstractValidator<UpdateRecipeDto>
        {
            public UpdateRecipeDTOValidator()
            {
                RuleFor(x => x.Description).NotEmpty().Length(min: 5, max: 500);
            }
        }
    }
}
