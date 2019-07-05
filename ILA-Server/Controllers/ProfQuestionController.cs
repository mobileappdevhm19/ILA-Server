using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ILA_Server.Areas.Identity.Models;
using ILA_Server.Data;
using ILA_Server.Hubs;
using ILA_Server.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ILA_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProfQuestionController : ControllerBase
    {
        private readonly ILADbContext _context;
        private readonly IHubContext<LectureHub> _hubContext;

        public ProfQuestionController(ILADbContext context, IHubContext<LectureHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet("{lectureId}")]
        public async Task<List<ProfQuestion>> Get(int lectureId)
        {
            var questions = await _context.ProfQuestion
                .Where(x => x.Lecture.Course.Members.Any(y => y.MemberId == GetUserId()))
                .Include(x => x.Answers)
                .ToListAsync();

            foreach (ProfQuestion question in questions)
            {
                question.Lecture = null;
            }

            return questions;
        }

        [HttpPost("{answerId}")]
        public async Task<ProfQuestionAnswer> Answer(int answerId)
        {
            ProfAnswer answer = await _context.ProfAnswer
                .Include(x => x.ProfQuestionAnswers)
                .ThenInclude(x => x.User)
                .Include(x => x.ProfQuestion)
                .ThenInclude(x => x.Lecture)
                .Include(x => x.ProfQuestion)
                .ThenInclude(x => x.Answers)
                .ThenInclude(x => x.ProfQuestionAnswers)
                .ThenInclude(x => x.User)
                .Where(x => x.ProfQuestion.Lecture.Course.Members.Any(y => y.MemberId == GetUserId()))
                .SingleOrDefaultAsync(x => x.Id == answerId);

            ILAUser user = await _context.Users.FindAsync(GetUserId());

            if (answer == null)
                throw new UserException("Answer not found", 404);

            if (user == null)
                throw new UserException("Internal server Error", 500);

            ProfQuestionAnswer questionAnswer = answer.ProfQuestion.Answers
                .FirstOrDefault(x => x.ProfQuestionAnswers.FirstOrDefault(y => y.User.Id == user.Id) != null)?.ProfQuestionAnswers.FirstOrDefault(x => x.User.Id == user.Id);

            if (questionAnswer == null)
            {
                questionAnswer = new ProfQuestionAnswer
                {
                    ProfAnswer = answer,
                    ProfAnswerId = answer.Id,
                    User = user,
                };

                _context.ProfQuestionAnswer.Add(questionAnswer);
                await _context.SaveChangesAsync();
            }
            else
            {
                questionAnswer.ProfAnswer = answer;
                questionAnswer.ProfAnswerId = answer.Id;

                _context.Update(questionAnswer);
                await _context.SaveChangesAsync();
            }

            int lectureId = questionAnswer.ProfAnswer.ProfQuestion.LectureId;

            questionAnswer.User = null;
            questionAnswer.ProfAnswer.ProfQuestion.Lecture = null;

            await _hubContext.Clients.Group(lectureId.ToString())
                .SendAsync("QuestionChanged", questionAnswer.ProfAnswer.ProfQuestionId);

            return questionAnswer;
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
