using System.Threading.Tasks;
using ILA_Server.Areas.Identity.Models;

namespace ILA_Server.Areas.Identity.Services
{
    public interface IAccountService
    {
        Task SignUp(string email, string password, string firstName, string lastName);
        Task<JsonWebToken> SignIn(string username, string password);
    }
}
