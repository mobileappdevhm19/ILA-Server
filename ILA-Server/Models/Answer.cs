using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ILA_Server.Models
{
    public class Answer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Comment { get; set; }

        [Required]
        [DefaultValue(0)]
        public int Votes { get; set; }

        [Display(Name = "Created")]
        public DateTime CreatedAt { get; set; }

        public ILAUser User { get; set; }
        public int QuestionId { get; set; }
        public Question Question { get; set; }
    }

    public class AnswerCreate
    {
        [Required]
        public string Comment { get; set; }
    }
}
