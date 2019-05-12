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
        [HttpGet("member")]
        public async Task<IEnumerable<Course>> GetMemberCourses()
        {
            string userId = GetUserId();

            return await _context.Courses
                .Where(x => x.Members.Any(y => y.MemberId == userId))
                .ToListAsync();
        }

        // GET: api/Courses
        [HttpGet("owner")]
        public async Task<IEnumerable<Course>> GetOwnerCourses()
        {
            string userId = GetUserId();

            return await _context.Courses
                .Where(x => x.Owner.Id == userId)
                .Include(x => x.Tokens)
                .Include(x => x.Lectures)
                .ToListAsync();
        }

        // GET: api/Courses/5
        [HttpGet("owner/{id}")]
        public async Task<Course> GetOwnerCourse(int id)
        {
            var course = await _context.Courses
                .Where(x => x.Id == id)
                .Where(x => x.Owner.Id == GetUserId())
                .Include(x => x.Tokens)
                .Include(x => x.Lectures)
                .ToListAsync();

            if (course == null || course.Count == 0)
                throw new UserException(404);

            return course[0];
        }

        // GET: api/Courses/5
        [HttpGet("member/{id}")]
        public async Task<Course> GetMemberCourse(int id)
        {
            var course = await _context.Courses
                .Where(x => x.Id == id)
                .Where(x => x.Members.Any(y => y.MemberId == GetUserId()))
                .ToListAsync();

            if (course == null || course.Count == 0)
                throw new UserException(404);

            return course[0];
        }

        // PUT: api/Courses/5
        [HttpPut("{id}")]
        public async Task<Course> PutCourse(int id, [FromBody] CourseCreateUpateModel courseModel)
        {
            var course = await _context.Courses
                .Include(x => x.Owner)
                .SingleOrDefaultAsync(x => x.Id == id);

            if (course == null)
                throw new UserException(404);

            if (course.Owner.Id != GetUserId())
                throw new UserException(403);

            try
            {
                course.Title = courseModel.Title;
                course.Archived = courseModel.Archived;
                course.Description = courseModel.Description;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new UserException(500);
            }

            return course;
        }

        // POST: api/Courses
        [HttpPost]
        public async Task<ActionResult<Course>> PostCourse([FromBody] CourseCreateUpateModel courseModel)
        {
            ILAUser user = await _context.Users.FindAsync(GetUserId());
            Course course = new Course
            {
                Archived = courseModel.Archived,
                Owner = user,
                Description = courseModel.Description,
                Title = courseModel.Title,
            };

            try
            {
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new UserException(500);
            }

            return RedirectToAction("GetOwnerCourse", new { id = course.Id });
        }

        // DELETE: api/Courses/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Course>> DeleteCourse(int id)
        {
            var course = await _context.Courses
                .Include(x => x.Owner)
                .SingleOrDefaultAsync(x => x.Id == id);

            if (course == null)
                throw new UserException(404);

            if (course.Owner.Id != GetUserId())
                throw new UserException(403);

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return course;
        }

        [HttpPost("generateToken/{courseId}")]
        public async Task<ActionResult> GenerateToken(int courseId)
        {
            var course = await _context.Courses
                .Include(x => x.Owner)
                .SingleOrDefaultAsync(x => x.Id == courseId);

            if (course == null)
                throw new UserException(404);

            if (course.Owner.Id != GetUserId())
                throw new UserException(403);

            try
            {
                CourseToken token = new CourseToken { Active = true, Course = course, Token = RandomString(10, false) };

                await _context.CourseTokens.AddAsync(token);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new UserException(500);
            }

            return RedirectToAction("GetOwnerCourse", new { id = course.Id });
        }

        [HttpDelete("deleteToken/{tokenId}")]
        public async Task<ActionResult> DeleteToken(int tokenId)
        {
            var token = await _context.CourseTokens
                .Include(x => x.Course)
                .ThenInclude(x => x.Owner)
                .SingleOrDefaultAsync(x => x.Id == tokenId);

            if (token == null)
                throw new UserException(404);

            if (token.Course.Owner.Id != GetUserId())
                throw new UserException(403);

            try
            {
                _context.CourseTokens.Remove(token);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new UserException(500);
            }
            return RedirectToAction("GetOwnerCourse", new { id = token.Course.Id });
        }

        [HttpPut("updateToken/{tokenId}")]
        public async Task<CourseToken> UpdateToken(int tokenId, [FromBody] bool state)
        {
            var token = await _context.CourseTokens
                .Include(x => x.Course)
                .ThenInclude(x => x.Owner)
                .SingleOrDefaultAsync(x => x.Id == tokenId);

            if (token == null)
                throw new UserException(404);

            if (token.Course.Owner.Id != GetUserId())
                throw new UserException(403);

            try
            {
                token.Active = state;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new UserException(500);
            }

            return token;
        }

        [HttpPost("join/{courseId}")]
        public async Task<ActionResult> JoinCourse(int courseId, [FromBody] string token)
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
                return RedirectToAction("GetMemberCourse", new { id = course.Id });
            }
            throw new UserException("CourseId and/or token wrong.", 404);
        }

        [HttpPost("leave/{courseId}")]
        public async Task<ActionResult> LeaveCourse(int courseId)
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
            return RedirectToAction("GetMemberCourses");
        }

        private string RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            for (int i = 0; i < size; i++)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
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
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public bool Archived { get; set; }
    }
}
