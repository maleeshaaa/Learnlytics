using System.ComponentModel.DataAnnotations;

namespace Learnlytics.API.Models
{
    public class LoginModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

    }
}
