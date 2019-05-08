using System.Threading.Tasks;
using ILA_Server.Models;

namespace ILA_Server.Areas.Identity.Services
{
    public interface IUserService
    {
        Task<ILAUser> GetUserById(string id);

        Task ChangePassword(ILAUser user, string oldPassword, string newPassword);
        Task EditUser(ILAUser user);

        Task DeleteUser(ILAUser user);
    }
}
