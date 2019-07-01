using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ILA_Server.Models
{
    public class ProfAnswer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Answer")]
        public string Answer { get; set; }

        public int ProfQuestionId { get; set; }
        public ProfQuestion ProfQuestion { get; set; }

        [Display(Name = "Answers")]
        public ICollection<ProfQuestionAnswer> ProfQuestionAnswers { get; set; }
    }
}
