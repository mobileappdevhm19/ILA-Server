using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ILA_Server.Areas.Identity.Models;
using ILA_Server.Data;
using ILA_Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ILA_Server.Hubs
{
    [Authorize]
    public class LectureHub : Hub
    {
        private readonly ILADbContext _context;

        public LectureHub(ILADbContext context)
        {
            _context = context;
        }

        public async Task RegisterLecture(int lectureId)
        {
            var lecture = await _context.Lectures.FirstOrDefaultAsync(x => x.Id == lectureId && x.Course.Owner.Id == GetUserId());

            if (lecture != null)
                await Groups.AddToGroupAsync(Context.ConnectionId, lectureId.ToString());
        }

        public async Task<List<Pause>> GetPauses(int lectureId)
        {
            return await _context.Pauses
                .Where(x => x.Lecture.Id == lectureId && x.Lecture.Course.Owner.Id == GetUserId())
                .ToListAsync();
        }

        public async Task<ProfQuestion> GetAnswers(int questionId)
        {
            return await _context.ProfQuestion
                .Where(x => x.Lecture.Course.Owner.Id == GetUserId())
                .Include(x => x.Answers)
                .ThenInclude(x => x.ProfQuestionAnswers)
                .SingleOrDefaultAsync(x => x.Id == questionId);
        }

        private string GetUserId()
        {
            string userId = Context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                throw new UserException("Internal Error");
            return userId;
        }
    }
}
