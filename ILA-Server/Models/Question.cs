using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ILA_Server.Models
{
    public class Question
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Question")]
        public string PointedQuestion { get; set; }

        public int LectureId { get; set; }
        public Lecture Lecture { get; set; }
        public ILAUser User { get; set; }

        [Display(Name = "Answers")]
        public ICollection<Answer> Answers { get; set; }
    }

    public class QuestionCreate
    {
        [Required]
        public string PointedQuestion { get; set; }
    }
}
