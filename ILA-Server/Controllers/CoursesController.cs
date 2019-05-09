﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ILA_Server.Areas.Identity.Models;
using Microsoft.AspNetCore.Http;
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
        public async Task<IActionResult> PutCourse(int id, CourseCreateUpateModel courseModel)
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

            return NoContent();
        }

        // POST: api/Courses
        [HttpPost]
        public async Task<ActionResult<Course>> PostCourse(CourseCreateUpateModel courseModel)
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

            return CreatedAtAction("GetOwnerCourse", new { id = course.Id }, course);
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

        [HttpPost("generateToken")]
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

            return CreatedAtAction("GetOwnerCourse", new { id = course.Id }, course);
        }

        [HttpPost("join")]
        public async Task<ActionResult> JoinCourse(int courseId, string token)
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
                return CreatedAtAction("GetMemberCourse", new { id = course.Id }, course);
            }
            throw new UserException("CourseId and/or token wrong.", 404);
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