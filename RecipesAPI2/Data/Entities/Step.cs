using FluentValidation;
using RecipesAPI2.Auth.Model;
using System.ComponentModel.DataAnnotations;

namespace RecipesAPI2.Data.Entities
{
    public class Step
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public required TimeSpan Duration { get; set; }

        public Recipe Recipe { get; set; }

        [Required]
        public required string UserId { get; set; }
        public RecipeUser user { get; set; }

        public StepDto ToDto()
        {
            return new StepDto(Id, Text, Duration);
        }
    }

    public record StepDto(int Id, string Text, TimeSpan Duration);

    public record CreateStepDto(string Text, TimeSpan Duration)
    {
        public class CreateStepDTOValidator : AbstractValidator<CreateStepDto>
        {
            public CreateStepDTOValidator()
            {
                RuleFor(x => x.Text).NotEmpty().Length(min: 2, max: 500);
                RuleFor(x => x.Duration).NotNull().GreaterThan(TimeSpan.FromSeconds(5));
            }
        }
    };
    public record UpdateStepDto(string Text, TimeSpan Duration)
    {
        public class UpdateStepDTOValidator : AbstractValidator<UpdateStepDto>
        {
            public UpdateStepDTOValidator()
            {
                RuleFor(x => x.Text).NotEmpty().Length(min: 2, max: 500);
                RuleFor(x => x.Duration).NotNull().GreaterThan(TimeSpan.FromSeconds(5));
            }
        }
    }
}
