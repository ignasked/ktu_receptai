using System.ComponentModel.DataAnnotations;
using RecipesAPI2.Auth.Model;

namespace RecipesAPI2.Data.Entities
{
    public class Session
    {
        public Guid Id { get; set; }

        public string LastRefreshToken { get; set; }

        public DateTime InitiatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; }

        [Required]
        public required string UserId { get; set; }

        public RecipeUser User { get; set; }
    }
}
