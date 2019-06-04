using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ILA_Server.Models
{
    public class CourseNews
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Body { get; set; }

        [NotMapped]
        public string BodyTrimmed => Body.Length > 50 ? Body.Substring(0, 50) + " ..." : Body;

        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}
