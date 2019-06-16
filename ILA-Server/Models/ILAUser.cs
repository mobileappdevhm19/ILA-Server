using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ILA_Server.Data;
using Microsoft.AspNetCore.Identity;

namespace ILA_Server.Models
{
    public class ILAUser : IdentityUser
    {
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public ICollection<Course> MyCourses { get; set; }
        
        public ICollection<CourseMember> MemberCourses { get; set; }
        public ICollection<Pause> Pauses { get; set; }
        public ICollection<Question> Questions { get; set; }
        public ICollection<Answer> Answers { get; set; }
        public ICollection<PushTokens> PushTokens { get; set; }
    }
}
