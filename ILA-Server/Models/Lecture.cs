using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ILA_Server.Models
{
    public class Lecture
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [DataType(DataType.Text)]
        public string Description { get; set; }

        [Required]
        [DefaultValue(true)]
        public bool Visible { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Start { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Stop { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }
        public ICollection<Pause> Pauses { get; set; }
        public ICollection<Question> Questions { get; set; }
    }
}
