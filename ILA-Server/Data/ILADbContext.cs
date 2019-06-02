using ILA_Server.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ILA_Server.Data
{
    public class ILADbContext : IdentityDbContext<ILAUser>
    {
        public DbSet<Course> Courses { get; set; }
        public DbSet<Lecture> Lectures { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<CourseToken> CourseTokens { get; set; }
        public DbSet<Pause> Pauses { get; set; }
        public DbSet<PushTokens> PushTokens { get; set; }
        public DbSet<CourseNews> CourseNews { get; set; }

        public ILADbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.Owner)
                .WithMany(c => c.MyCourses)
                .IsRequired();

            modelBuilder.Entity<CourseMember>()
                .HasKey(t => new { t.CourseId, t.MemberId });
            modelBuilder.Entity<CourseMember>()
                .HasOne(cm => cm.Course)
                .WithMany(c => c.Members)
                .HasForeignKey(fk => fk.CourseId);
            modelBuilder.Entity<CourseMember>()
                .HasOne(cm => cm.Member)
                .WithMany(c => c.MemberCourses)
                .HasForeignKey(fk => fk.MemberId);

            modelBuilder.Entity<Pause>()
                .HasOne(c => c.User)
                .WithMany(c => c.Pauses)
                .IsRequired();
            modelBuilder.Entity<Pause>()
                .HasOne(c => c.Lecture)
                .WithMany(c => c.Pauses)
                .IsRequired();

            modelBuilder.Entity<Lecture>()
                .HasOne(c => c.Course)
                .WithMany(c => c.Lectures)
                .HasForeignKey(k => k.CourseId)
                .IsRequired();

            modelBuilder.Entity<CourseToken>()
                .HasOne(ct => ct.Course)
                .WithMany(c => c.Tokens)
                .IsRequired();

            modelBuilder.Entity<CourseNews>()
                .HasOne(ct => ct.Course)
                .WithMany(c => c.News)
                .HasForeignKey(k=>k.CourseId)
                .IsRequired();

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Lecture)
                .WithMany(l => l.Questions)
                .HasForeignKey(k => k.LectureId)
                .IsRequired();
            modelBuilder.Entity<Question>()
                .HasOne(q => q.User)
                .WithMany(l => l.Questions)
                .IsRequired();

            modelBuilder.Entity<Answer>()
                .HasOne(q => q.User)
                .WithMany(l => l.Answers)
                .IsRequired();
            modelBuilder.Entity<Answer>()
                .HasOne(q => q.Question)
                .WithMany(l => l.Answers)
                .HasForeignKey(x => x.QuestionId)
                .IsRequired();

            modelBuilder.Entity<PushTokens>()
                .HasOne(q => q.User)
                .WithMany(l => l.PushTokens)
                .IsRequired();

        }
    }
}

