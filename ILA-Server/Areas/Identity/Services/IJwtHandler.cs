using System.Threading.Tasks;
using ILA_Server.Areas.Identity.Models;
using ILA_Server.Models;
using Microsoft.AspNetCore.Identity;

namespace ILA_Server.Areas.Identity.Services
{
    public interface IJwtHandler
    {
        Task<JsonWebToken> Create(ILAUser user, UserManager<ILAUser> userManager);
    }
}
