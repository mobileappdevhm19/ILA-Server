using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ILA_Server.Models
{
    public class ProfQuestion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Question")]
        public string Question { get; set; }

        [Display(Name = "Created")]
        public DateTime CreatedAt { get; set; }

        public int LectureId { get; set; }
        public Lecture Lecture { get; set; }

        [Display(Name = "Answers")]
        public ICollection<ProfAnswer> Answers { get; set; }
    }
}
