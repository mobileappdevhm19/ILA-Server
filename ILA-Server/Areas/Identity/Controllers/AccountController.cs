using System.Threading.Tasks;
using ILA_Server.Areas.Identity.Models;
using ILA_Server.Areas.Identity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ILA_Server.Areas.Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ITokenManager _tokenManager;

        public AccountController(IAccountService accountService,
            ITokenManager tokenManager)
        {
            _accountService = accountService;
            _tokenManager = tokenManager;
        }

        [HttpPost("signup")]
        [AllowAnonymous]
        public async Task<IActionResult> SignUp([FromBody] SignUp request)
        {
            if (ModelState.IsValid)
            {
                await _accountService.SignUp(request.Username, request.Password, request.FirstName, request.LastName);
                return Ok();
            }

            throw new UserException(ModelState);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<JsonWebToken> SignIn([FromBody] SignIn request)
            => await _accountService.SignIn(request.Username, request.Password);


        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _tokenManager.DeactivateCurrentAsync();

            return Ok();
        }
    }
}