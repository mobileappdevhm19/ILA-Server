using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using ILA_Server.Areas.Identity.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace ILA_Server.Areas.Identity.Services
{
    public class TokenManager : ITokenManager
    {
        private readonly IDistributedCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<JwtOptions> _jwtOptions;

        public TokenManager(IDistributedCache cache,
            IHttpContextAccessor httpContextAccessor,
            IOptions<JwtOptions> jwtOptions
        )
        {
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
            _jwtOptions = jwtOptions;
        }

        public async Task<TokenState> IsCurrentActiveToken()
            => await IsActiveAsync(GetCurrentToken());

        public async Task<TokenState> IsActiveAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return TokenState.Unknown;

            var jwtToken = new JwtSecurityToken(token);
            var guid = jwtToken.Claims?.First(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrEmpty(guid))
                return TokenState.Unknown;

            return await _cache.GetStringAsync(GetKey(guid)) == "active" 
                ? TokenState.Active
                : TokenState.InActive;
        }

        public async Task ActivateAsync(string guid)
            => await SetState(guid, "active");

        public async Task DeactivateCurrentAsync()
            => await DeactivateAsync(GetCurrentToken());


        public async Task DeactivateAsync(string token)
        {
            var jwtToken = new JwtSecurityToken(token);
            var guid = jwtToken.Claims?.First(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;

            if (!string.IsNullOrEmpty(guid))
                await SetState(guid, "inactive");
        }

        private async Task SetState(string guid, string state)
        {
            await _cache.SetStringAsync(GetKey(guid),
                state,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_jwtOptions.Value.ExpiryMinutes)
                });
        }

        private static string GetKey(string guid) => $"token:{guid}";

        private string GetCurrentToken()
        {
            var authorizationHeader = _httpContextAccessor
                .HttpContext.Request.Headers["authorization"];

            return authorizationHeader == StringValues.Empty
                ? string.Empty
                : authorizationHeader.Single().Split(" ").Last();
        }
    }
}
