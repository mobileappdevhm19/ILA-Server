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
        public string PointedQuestion { get; set; }

        public Lecture Lecture { get; set; }
        public ILAUser User { get; set; }

        public ICollection<Answer> Answers { get; set; }
    }

    public class QuestionCreate
    {
        [Required]
        public string PointedQuestion { get; set; }
    }
}
