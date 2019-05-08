using System.ComponentModel.DataAnnotations;

namespace ILA_Server.Areas.Identity.Models
{
    public class SignIn
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
