using System.Threading.Tasks;

namespace ILA_Server.Areas.Identity.Services
{
    public interface ITokenManager
    {
        Task<TokenState> IsCurrentActiveToken();
        Task DeactivateCurrentAsync();
        Task<TokenState> IsActiveAsync(string token);
        Task ActivateAsync(string guid);
        Task DeactivateAsync(string token);
    }

    public enum TokenState
    {
        Active,
        InActive,
        Unknown
    }
}
