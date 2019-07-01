using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ILA_Server.Models
{
    public class ProfQuestionAnswer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Created")]
        public DateTime CreatedAt { get; set; }

        public int ProfAnswerId { get; set; }
        public ProfAnswer ProfAnswer { get; set; }

        public ILAUser User { get; set; }
    }
}
