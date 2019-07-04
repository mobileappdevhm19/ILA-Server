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
using ILA_Server.Hubs;
using ILA_Server.Models;
using ILA_Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ILA_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class LecturesController : ControllerBase
    {
        private readonly ILADbContext _context;
        private readonly IHubContext<LectureHub> _hubContext;
        private readonly IFireBaseService _fireBaseService;

        public LecturesController(ILADbContext context, IHubContext<LectureHub> hubContext, IFireBaseService fireBaseService)
        {
            _context = context;
            _hubContext = hubContext;
            _fireBaseService = fireBaseService;
        }

        [HttpGet("all/{courseId}")]
        public async Task<IEnumerable<Lecture>> GetAll(int courseId)
        {
            return await _context.Lectures
                .Where(x => x.Course.Id == courseId)
                .Where(x => x.Course.Members.Any(y => y.MemberId == GetUserId()))
                .Include(x => x.Questions)
                .ThenInclude(x => x.Answers)
                .ToListAsync();
        }

        [HttpGet("{lectureId}")]
        public async Task<Lecture> Get(int lectureId)
        {
            return await _context.Lectures
                .Where(x => x.Id == lectureId)
                .Where(x => x.Course.Members.Any(y => y.MemberId == GetUserId()))
                .Include(x => x.Questions)
                .ThenInclude(x => x.Answers)
                .SingleOrDefaultAsync();
        }

        [HttpPost("{lectureId}/pause")]
        public async Task<Pause> PostPause(int lectureId)
        {
            Lecture lecture = await _context.Lectures
                .Include(x => x.Pauses)
                .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == lectureId);

            ILAUser user = await _context.Users.FindAsync(GetUserId());

            if (lecture == null)
                throw new UserException("Lecture not found", 404);

            if (user == null)
                throw new UserException("Internal server Error", 500);

            Pause pause = new Pause
            {
                Lecture = lecture,
                TimeStamp = DateTime.Now.ToUniversalTime(),
                User = user
            };

            if (lecture.Pauses.Any(x => x.User.Id == GetUserId() && (pause.TimeStamp - x.TimeStamp).TotalSeconds < 120))
            {
                throw new UserException("You can pause ery 120 seconds only.", 481);
            }

            await _context.Pauses.AddAsync(pause);
            await _context.SaveChangesAsync();

            pause.User = null;
            pause.Lecture = null;

            await _hubContext.Clients.Group(lectureId.ToString()).SendAsync("Pause", pause);
            pause.TimeStamp = DateTime.Parse(DateTime.Now.ToString());

            return pause;
        }

        [HttpPut("{id}")]
        public async Task<Lecture> Put(int id, [FromBody] LectureCreateUpdateModel lectureModel)
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

            lecture.Course = null;
            lecture.Pauses = null;
            lecture.Questions = null;

            return lecture;
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
