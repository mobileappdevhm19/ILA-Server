using System.ComponentModel.DataAnnotations;

namespace ILA_Server.Areas.Identity.Models.User
{
    public class UserModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public bool Confirmed { get; set; }

        [Required]
        public string Email { get; set; }
    }
}
