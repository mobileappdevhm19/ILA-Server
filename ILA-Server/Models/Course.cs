using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ILA_Server.Data;

namespace ILA_Server.Models
{
    public class Course
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required]
        [DefaultValue("")]
        [DataType(DataType.Text)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [DefaultValue(false)]
        [Display(Name = "Archived")]
        public bool Archived { get; set; }


        public ILAUser Owner { get; set; }

        public ICollection<CourseMember> Members { get; set; }
        public ICollection<Lecture> Lectures { get; set; }
        public ICollection<CourseToken> Tokens { get; set; }
        public ICollection<CourseNews> News { get; set; }
        
    }

    public class CourseMember
    {
        public Course Course { get; set; }
        public int CourseId { get; set; }

        public ILAUser Member { get; set; }
        public string MemberId { get; set; }
    }

}
