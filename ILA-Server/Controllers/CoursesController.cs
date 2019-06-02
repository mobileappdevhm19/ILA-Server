using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ILA_Server.Areas.Identity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ILA_Server.Data;
using ILA_Server.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace ILA_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CoursesController : ControllerBase
    {
        private readonly ILADbContext _context;

        public CoursesController(ILADbContext context)
        {
            _context = context;
        }

        // GET: api/Courses
        [HttpGet]
        public async Task<IEnumerable<Course>> Get()
        {
            string userId = GetUserId();

            var courses = await _context.Courses
                .Include(x => x.News)
                .Where(x => x.Members.Any(y => y.MemberId == userId))
                .ToListAsync();
            
            foreach (Course course in courses)
            {
                course.Members = null;
                course.Owner = null;
            }

            return courses;
        }

        // GET: api/Courses/5
        [HttpGet("{id}")]
        public async Task<Course> Get(int id)
        {
            var course = await _context.Courses
                .Include(x => x.News)
                .Where(x => x.Members.Any(y => y.MemberId == GetUserId()))
                .FirstOrDefaultAsync(x => x.Id == id);

            if (course == null)
                throw new UserException(404);

            course.Owner = null;
            course.Members = null;

            return course;
        }

        [HttpPost("{courseId}/join")]
        public async Task<IEnumerable<Course>> Join(int courseId, string token)
        {
            var course = await _context.Courses
                .Include(x => x.Tokens)
                .SingleOrDefaultAsync(x => x.Id == courseId);

            if (course == null)
                throw new UserException("CourseId and/or token wrong.", 404);

            if (course.Tokens.Any(x => x.Active && x.Token == token))
            {
                if (course.Members == null)
                    course.Members = new List<CourseMember>();

                course.Members.Add(new CourseMember { Course = course, CourseId = course.Id, MemberId = GetUserId() });
                await _context.SaveChangesAsync();
                return await Get();
            }

            throw new UserException("CourseId and/or token wrong.", 404);
        }

        [HttpPost("{courseId}/leave")]
        public async Task<IEnumerable<Course>> Leave(int courseId)
        {
            var course = await _context.Courses
                .Where(x => x.Members.Any(y => y.MemberId == GetUserId()))
                .Include(x => x.Members)
                .SingleOrDefaultAsync(x => x.Id == courseId);

            if (course == null)
                throw new UserException("Course not found.", 404);

            CourseMember courseMember = course.Members.FirstOrDefault(x => x.MemberId == GetUserId());
            if (courseMember == null)
                throw new UserException("No course found where you are a member.", 404);

            course.Members.Remove(courseMember);
            await _context.SaveChangesAsync();

            return await Get();
        }

        private string GetUserId()
        {
            string userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                throw new UserException("Internal Error");
            return userId;
        }
    }

    public class CourseCreateUpateModel
    {
        [Required] public string Title { get; set; }

        [Required] public string Description { get; set; }

        [Required] public bool Archived { get; set; }
    }
}