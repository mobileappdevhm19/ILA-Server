using System.Linq;
using System.Threading.Tasks;
using ILA_Server.Areas.Identity.Models;
using ILA_Server.Models;
using Microsoft.AspNetCore.Identity;

namespace ILA_Server.Areas.Identity.Services
{
    public class AccountService : IAccountService
    {
        private readonly IJwtHandler _jwtHandler;
        private readonly UserManager<ILAUser> _userManager;

        public AccountService(IJwtHandler jwtHandler,
            UserManager<ILAUser> userManager)
        {
            _jwtHandler = jwtHandler;
            _userManager = userManager;
        }

        public async Task SignUp(string email, string password, string firstName, string lastName)
        {
            ILAUser user = new ILAUser
            {
                UserName = email,
                Email = email,
                LastName = lastName,
                FirstName = firstName,
            };

            var identityResult = await _userManager.CreateAsync(user, password);
            if (!identityResult.Succeeded)
            {
                throw new UserException(identityResult.Errors.Select(x => x.Description).ToList());
            }
        }

        
        public async Task<JsonWebToken> SignIn(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user != null)
            {
                if (await _userManager.CheckPasswordAsync(user, password))
                    return await _jwtHandler.Create(user, _userManager);
            }
            
            throw new UserException("Username or password not correct.");
        }
    }
}
