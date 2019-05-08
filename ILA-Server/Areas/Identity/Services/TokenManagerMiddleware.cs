using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ILA_Server.Areas.Identity.Services
{
    public class TokenManagerMiddleware : IMiddleware
    {
        private readonly ITokenManager _tokenManager;

        public TokenManagerMiddleware(ITokenManager tokenManager)
        {
            _tokenManager = tokenManager;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {

            if (await _tokenManager.IsCurrentActiveToken() == TokenState.InActive)
            {
                // custom HTTP-Code 498 for an revoked token.
                context.Response.StatusCode = 498;
            }
            else
            {
                await next(context);
            }
        }
    }
}
