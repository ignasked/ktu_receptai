using FluentValidation;
using RecipesAPI2.Auth.Model;
using System.ComponentModel.DataAnnotations;

namespace RecipesAPI2.Data.Entities
{
    public class Ingredient
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required double Amount { get; set; }
        public required string Unit { get; set; }

        public Step Step { get; set; }

        [Required]
        public required string UserId { get; set; }
        public RecipeUser user { get; set; }

        public IngredientDto ToDto()
        {
            return new IngredientDto(Id, Name, Amount, Unit);
        }

        

    }
    public record IngredientDto(int Id, string Name, double Amount, string Unit);

    public record CreateIngredientDto(string Name, double Amount, string Unit)
    {
        public class CreateIngredientDTOValidator : AbstractValidator<CreateIngredientDto>
        {
            public CreateIngredientDTOValidator()
            {
                RuleFor(x => x.Name).NotEmpty().Length(min: 2, max: 500);
                RuleFor(x => x.Amount).NotNull().GreaterThan(0);
                RuleFor(x => x.Unit).NotEmpty().Length(min: 1, max: 50);
            }
        }
    };
    public record UpdateIngredientDto(string Name, double Amount, string Unit)
    {
        public class UpdateIngredientDTOValidator : AbstractValidator<UpdateIngredientDto>
        {
            public UpdateIngredientDTOValidator()
            {
                RuleFor(x => x.Name).NotEmpty().Length(min: 2, max: 500);
                RuleFor(x => x.Amount).NotNull().GreaterThan(0);
                RuleFor(x => x.Unit).NotEmpty().Length(min: 1, max: 50);
            }
        }
    }
}
