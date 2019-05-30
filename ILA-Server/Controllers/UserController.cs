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

namespace ILA_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserController : ControllerBase
    {
        private readonly IPushService _pushService;

        public UserController(IPushService pushService) : base()
        {
            _pushService = pushService;
        }

        [HttpPost("token")]
        public async Task<IActionResult> PostToken([FromBody]SavePushToken model)
        {
            await _pushService.SaveToken(GetUserId(), model.Token, model.DeviceId);

            return Ok();
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
}