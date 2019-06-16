using System;
using System.ComponentModel.DataAnnotations;

namespace ILA_Server.Areas.Identity.Models.User
{
    public class UserDetailsModel
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
        public DateTime Birthday { get; set; }

        [Required]
        public bool Available{ get; set; }

        [Required]
        public bool Confirmed { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public bool EmailConfirmed { get; set; }
        
        public string PhoneNumber { get; set; }
        
        public bool PhoneNumberConfirmed { get; set; }
    }
}
