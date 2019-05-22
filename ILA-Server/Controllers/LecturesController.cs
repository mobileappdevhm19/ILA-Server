using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
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
    public class LecturesController : ControllerBase
    {
        private readonly ILADbContext _context;

        public LecturesController(ILADbContext context)
        {
            _context = context;
        }

        // GET: api/Lectures
        [HttpGet("member/{courseId}")]
        public async Task<IEnumerable<Lecture>> GetMemberLectures(int courseId)
        {
            return await _context.Lectures
                .Where(x => x.Course.Id == courseId)
                .Where(x => x.Course.Members.Any(y => y.MemberId == GetUserId()))
                .Include(x => x.Questions)
                .ThenInclude(x => x.Answers)
                .ToListAsync();
        }

        // GET: api/Lectures/5
        [HttpGet("owner/{courseId}")]
        public async Task<IEnumerable<Lecture>> GetOwnerLectures(int courseId)
        {
            return await _context.Lectures
                .Where(x => x.Course.Id == courseId)
                .Where(x => x.Course.Owner.Id == GetUserId())
                .Include(x => x.Questions)
                .ThenInclude(x => x.Answers)
                .Include(x => x.Pauses)
                .ToListAsync();
        }


        // GET: api/Lectures
        [HttpGet("memberLecture/{lectureId}")]
        public async Task<Lecture> GetMemberLecture(int lectureId)
        {
            return await _context.Lectures
                .Where(x => x.Id == lectureId)
                .Where(x => x.Course.Members.Any(y => y.MemberId == GetUserId()))
                .Include(x => x.Questions)
                .ThenInclude(x => x.Answers)
                .SingleOrDefaultAsync();
        }

        // GET: api/Lectures/5
        [HttpGet("ownerLecture/{lectureId}")]
        public async Task<Lecture> GetOwnerLecture(int lectureId)
        {
            return await _context.Lectures
                .Where(x => x.Id == lectureId)
                .Where(x => x.Course.Owner.Id == GetUserId())
                .Include(x => x.Questions)
                .ThenInclude(x => x.Answers)
                .Include(x => x.Pauses)
                .SingleOrDefaultAsync();
        }

        [HttpPost("pause/{lectureId}")]
        public async Task<ActionResult> PostPause(int lectureId)
        {
            Lecture lecture = await GetMemberLecture(lectureId);
            ILAUser user = await _context.Users.FindAsync(GetUserId());

            if (lecture == null)
                throw new UserException("Lecture not found", 404);

            if (user == null)
                throw new UserException("Internal server Error", 500);

            Pause pause = new Pause
            {
                Lecture = lecture,
                TimeStamp = DateTime.Now.ToUniversalTime(),
                User = user,
            };

            await _context.Pauses.AddAsync(pause);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/Lectures/5
        [HttpPut("{id}")]
        public async Task<ActionResult<IEnumerable<Lecture>>> PutLecture(int id, [FromBody] LectureCreateUpdateModel lectureModel)
        {
            Lecture lecture = await _context.Lectures
                .Where(x => x.Course.Owner.Id == GetUserId())
                .Where(x => x.Id == id)
                .Include(x => x.Course)
                .SingleOrDefaultAsync();
            if (lecture == null)
                throw new UserException("Couldn't find a lecture with the id where you are the owner.", 404);

            lecture.Description = lectureModel.Description;
            lecture.Start = lectureModel.Start;
            lecture.Stop = lectureModel.Stop;
            lecture.Title = lectureModel.Title;
            lecture.Visible = lectureModel.Visible;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(GetOwnerLectures), new { courseId = lecture.Course.Id });
        }

        // POST: api/Lectures
        [HttpPost("{courseId}")]
        public async Task<ActionResult<IEnumerable<Lecture>>> PostLecture(int courseId, [FromBody] LectureCreateUpdateModel lectureModel)
        {
            Course course = await _context.Courses
                .Where(x => x.Owner.Id == GetUserId())
                .Where(x => x.Id == courseId)
                .SingleOrDefaultAsync();
            if (course == null)
                throw new UserException("Couldn't find a course with the id where you are the owner.", 404);

            Lecture lecture = new Lecture
            {
                Course = course,
                Description = lectureModel.Description,
                Start = lectureModel.Start,
                Stop = lectureModel.Stop,
                Title = lectureModel.Title,
                Visible = lectureModel.Visible,
            };

            _context.Lectures.Add(lecture);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(GetOwnerLectures), "Lectures", new { @courseId = lecture.Course.Id });
        }

        // DELETE: api/Lectures/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Lecture>> DeleteLecture(int id)
        {
            Lecture lecture = await _context.Lectures
                .Where(x => x.Course.Owner.Id == GetUserId())
                .Where(x => x.Id == id)
                .SingleOrDefaultAsync();

            if (lecture == null)
                throw new UserException("Couldn't find a lecture with the id where you are the owner.", 404);

            _context.Lectures.Remove(lecture);
            await _context.SaveChangesAsync();

            return lecture;
        }

        [HttpPost("questions/{lectureId}")]
        public async Task<ActionResult<Question>> PostQuestion(int lectureId, [FromBody] QuestionCreate model)
        {
            Lecture lecture = await GetMemberLecture(lectureId);
            ILAUser user = await _context.Users.FindAsync(GetUserId());

            Question question = new Question
            {
                PointedQuestion = model.PointedQuestion,
                Lecture = lecture,
                User = user
            };

            await _context.Questions.AddAsync(question);
            await _context.SaveChangesAsync();

            question.User = null;
            question.Lecture = null;
            return Ok(question);
        }

        [HttpGet("questions/{lectureId}")]
        public async Task<IEnumerable<Question>> GetQuestion(int lectureId)
        {
            return await _context.Questions
                .Where(x => x.Lecture.Course.Members.Any(y => y.MemberId == GetUserId()))
                .Include(x => x.Answers)
                .ToListAsync();
        }

        private bool LectureExists(int id)
        {
            return _context.Lectures.Any(e => e.Id == id);
        }

        private string GetUserId()
        {
            string userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                throw new UserException("Internal Error");
            return userId;
        }
    }

    public class LectureCreateUpdateModel
    {
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
    }
}
