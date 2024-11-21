namespace RecipesAPI2.Auth.Model
{
    public class RecipeRoles
    {
        public const string Admin = nameof(Admin);
        public const string RecipesUser = nameof(RecipesUser);

        public static readonly IReadOnlyCollection<string> All =
            new[] { Admin, RecipesUser };
    }
}
