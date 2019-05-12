using System.ComponentModel.DataAnnotations;

namespace ILA_Server.Areas.Identity.Models.User
{
    public class CreateUserModel
    {
        [Required]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }
    }
}
