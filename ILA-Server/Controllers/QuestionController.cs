using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ILA_Server.Areas.Identity.Models;
using ILA_Server.Data;
using ILA_Server.Models;
using ILA_Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ILA_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class QuestionController : ControllerBase
    {
        private readonly ILADbContext _context;
        private readonly IFireBaseService _fireBaseService;

        public QuestionController(ILADbContext context, IFireBaseService fireBaseService)
        {
            _context = context;
            _fireBaseService = fireBaseService;
        }

        [HttpGet]
        public async Task<List<Question>> Get()
        {
            var questions = await _context.Questions
                .Where(x => x.User.Id == GetUserId())
                .Include(x => x.Answers)
                .Include(x => x.Lecture)
                .ThenInclude(x => x.Course)
                .ToListAsync();

            foreach (Question question in questions)
            {
                question.User = null;
                foreach (Answer answer in question.Answers)
                {
                    answer.User = null;
                }
            }

            return questions;
        }

        [HttpGet("{id}")]
        public async Task<Question> Get(int id)
        {
            Question question = await _context.Questions
                .Where(x => x.User.Id == GetUserId())
                .Where(x => x.Id == id)
                .Include(x => x.Answers)
                .SingleOrDefaultAsync();

            if (question == null)
                throw new UserException("Couldn't find a question with the id where you are the owner.", 404);

            question.User = null;

            return question;
        }

        [HttpGet("lecture/{id}")]
        public async Task<IEnumerable<Question>> GetLecture(int id)
        {
            return await _context.Questions
                .Where(x => x.Lecture.Course.Members.Any(y => y.MemberId == GetUserId()))
                .Where(x => x.LectureId == id)
                .Include(x => x.Answers)
                .ToListAsync();
        }

        [HttpPost("questions/{lectureId}")]
        public async Task<Question> PostQuestion(int lectureId, [FromBody] QuestionCreate model)
        {
            Lecture lecture = await _context.Lectures
                .Where(x => x.Id == lectureId)
                .Where(x => x.Course.Members.Any(y => y.MemberId == GetUserId()))
                .Include(x => x.Questions)
                .ThenInclude(x => x.Answers)
                .SingleOrDefaultAsync();
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
            return question;
        }

        [HttpPut("{questionId}")]
        public async Task<Question> PutQuestion(int questionId, [FromBody] QuestionCreate model)
        {
            Question question = await _context.Questions
                .Where(x => x.User.Id == GetUserId())
                .Where(x => x.Id == questionId)
                .SingleOrDefaultAsync();

            if (question == null)
                throw new UserException("Couldn't find a question with the id where you are the owner.", 404);

            question.PointedQuestion = model.PointedQuestion;

            await _context.SaveChangesAsync();

            question.User = null;
            question.Answers = null;

            return question;
        }

        [HttpDelete("{questionId}")]
        public async Task<Question> DeleteQuestion(int questionId)
        {
            Question question = await _context.Questions
                .Where(x => x.User.Id == GetUserId())
                .Where(x => x.Id == questionId)
                .SingleOrDefaultAsync();

            if (question == null)
                throw new UserException("Couldn't find a question with the id where you are the owner.", 404);

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            question.User = null;
            question.Answers = null;

            return question;
        }

        [HttpGet("{questionId}/answers")]
        public async Task<IEnumerable<Answer>> GetAnswers(int questionId)
        {
            return await _context.Answers
                .Where(x => x.Question.Lecture.Course.Members.Any(y => y.MemberId == GetUserId()))
                .Include(x => x.Question)
                .ToListAsync();
        }

        [HttpPost("{questionId}/answer")]
        public async Task<Answer> PostAnswer(int questionId, [FromBody] AnswerCreate model)
        {
            Question question = await _context.Questions
                .Where(x => x.Lecture.Course.Members.Any(y => y.MemberId == GetUserId()))
                .Where(x => x.Id == questionId)
                .Include(x => x.User)
                .ThenInclude(x => x.PushTokens)
                .SingleOrDefaultAsync();
            ILAUser user = await _context.Users.FindAsync(GetUserId());

            if (question == null || user == null)
            {
                throw new UserException(404);
            }

            Answer answer = new Answer
            {
                Question = question,
                User = user,
                Comment = model.Comment,
            };

            await _context.Answers.AddAsync(answer);
            await _context.SaveChangesAsync();

            _fireBaseService.SendPushNotificationMessageToSingleUser(question.User, "New Answer",
                "Someone answerd your question",
                new Dictionary<string, string>
                {
                    {"questionId", question.Id.ToString()},
                    {"answerId", answer.Id.ToString()}
                });

            question.User = null;
            question.Lecture = null;

            return answer;
        }

        [HttpPut("answers/{answerId}")]
        public async Task<Answer> PutAnswer(int answerId, [FromBody] AnswerCreate model)
        {
            Answer answer = await _context.Answers
                .Where(x => x.User.Id == GetUserId())
                .Where(x => x.Id == answerId)
                .SingleOrDefaultAsync();

            if (answer == null)
                throw new UserException("Couldn't find a question with the id where you are the owner.", 404);

            answer.Comment = model.Comment;

            await _context.SaveChangesAsync();

            answer.Question = null;
            answer.User = null;

            return answer;
        }

        [HttpDelete("answers/{answerId}")]
        public async Task<Answer> DeleteAnswer(int answerId)
        {
            Answer answer = await _context.Answers
                .Where(x => x.User.Id == GetUserId())
                .Where(x => x.Id == answerId)
                .SingleOrDefaultAsync();

            if (answer == null)
                throw new UserException("Couldn't find a answer with the id where you are the owner.", 404);

            _context.Answers.Remove(answer);
            await _context.SaveChangesAsync();

            answer.Question = null;
            answer.User = null;

            return answer;
        }

        private string GetUserId()
        {
            string userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                throw new UserException("Internal Error");
            return userId;
        }
    }
}