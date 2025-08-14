using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Learnlytics.API.Models
{
    public class RegisterModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Username { get; set; }

        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password should be minimum 6 characters")]
        public string Password { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password should be same as Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        [BsonRepresentation(BsonType.String)]
        public UserRole Role { get; set; } = UserRole.Learner;

    }
}
