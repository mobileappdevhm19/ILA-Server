using System.Linq;
using System.Threading.Tasks;
using ILA_Server.Areas.Identity.Models;
using ILA_Server.Models;
using Microsoft.AspNetCore.Identity;

namespace ILA_Server.Areas.Identity.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ILAUser> _userManager;

        public UserService(UserManager<ILAUser> userManager)
        {
            _userManager = userManager;
        }
        
        public Task<ILAUser> GetUserById(string id) => _userManager.FindByIdAsync(id);

        public async Task ChangePassword(ILAUser user, string oldPassword, string newPassword)
        {
            var identityResult = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (!identityResult.Succeeded)
            {
                throw new UserException(identityResult.Errors.Select(x => x.Description).ToList());
            }
        }

        public async Task EditUser(ILAUser user)
        {
            var identityResult = await _userManager.UpdateAsync(user);
            if (!identityResult.Succeeded)
            {
                throw new UserException(identityResult.Errors.Select(x => x.Description).ToList());
            }
        }

        public async Task DeleteUser(ILAUser user)
        {
            var identityResult = await _userManager.DeleteAsync(user);
            if (!identityResult.Succeeded)
            {
                throw new UserException(identityResult.Errors.Select(x => x.Description).ToList());
            }
        }
    }
}
