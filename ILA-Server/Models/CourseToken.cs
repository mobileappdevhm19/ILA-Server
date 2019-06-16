using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ILA_Server.Models
{
    public class CourseToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(10)]
        [MinLength(10)]
        public string Token { get; set; }

        [DefaultValue(true)]
        public bool Active { get; set; }

        public Course Course { get; set; }

        public static CourseToken GenerateNewToken(Course course)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            for (int i = 0; i < 10; i++)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return new CourseToken { Token = builder.ToString(), Active = true, Course = course };
        }
    }
}
