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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace ILA_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserController : ControllerBase
    {
        private readonly IPushService _pushService;
        private readonly IFireBaseService _fireBaseService;
        private readonly ILADbContext _dbContext;

        public UserController(IPushService pushService, IFireBaseService fireBaseService, ILADbContext dbContext) : base()
        {
            _pushService = pushService;
            _fireBaseService = fireBaseService;
            _dbContext = dbContext;
        }

        [HttpPost("token")]
        public async Task<PushTokens> PostToken([FromBody]SavePushToken model)
        {
            return await _pushService.SaveToken(GetUserId(), model.Token, model.DeviceId);
        }

        [HttpGet("pushTest")]
        public async Task<TestPush> TestPush()
        {
            ILAUser user = await _dbContext.Users
                .Where(x => x.Id == GetUserId())
                .Include(x => x.PushTokens)
                .SingleOrDefaultAsync();
            if (user == null)
                throw new UserException(404);

            _fireBaseService.SendPushNotificationMessageToSingleUser(user, "Test", "Ping", null);

            return new TestPush { Message = "Send Test Notifications" };
        }

        private string GetUserId()
        {
            string userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                throw new UserException("Internal Error");
            return userId;
        }
    }

    public class SavePushToken
    {
        public string DeviceId { get; set; }
        public string Token { get; set; }
    }

    public class TestPush
    {
        public string Message { get; set; }
    }
}