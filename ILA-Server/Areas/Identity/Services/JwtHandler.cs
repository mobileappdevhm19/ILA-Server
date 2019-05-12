using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;
using ILA_Server.Areas.Identity.Models;
using ILA_Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ILA_Server.Areas.Identity.Services
{
    public class JwtHandler : IJwtHandler
    {
        private readonly JwtOptions _options;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        private readonly JwtHeader _jwtHeader;
        private readonly ITokenManager _tokenManager;

        public JwtHandler(IOptions<JwtOptions> options, ITokenManager tokenManager)
        {
            _tokenManager = tokenManager;
            _options = options.Value;
            SecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            _jwtHeader = new JwtHeader(signingCredentials);
        }

        public async Task<JsonWebToken> Create(ILAUser user, UserManager<ILAUser> userManager)
        {
            var nowUtc = DateTime.UtcNow;
            var expires = nowUtc.AddMinutes(_options.ExpiryMinutes);
            var exp = ((DateTimeOffset)expires).ToUnixTimeSeconds();
            var iat = ((DateTimeOffset)nowUtc).ToUnixTimeSeconds();
            var guid = Guid.NewGuid().ToString();

            var payload = new JwtPayload
            {
                {JwtRegisteredClaimNames.Sub, user.Id},
                {JwtRegisteredClaimNames.Iss, _options.Issuer},
                {JwtRegisteredClaimNames.Iat, iat},
                {JwtRegisteredClaimNames.Exp, exp},
                {JwtRegisteredClaimNames.Jti, guid}
            };

            await _tokenManager.ActivateAsync(guid);
            return new JsonWebToken
            {
                AccessToken = _jwtSecurityTokenHandler.WriteToken(new JwtSecurityToken(_jwtHeader, payload)),
                Expires = exp
            };
        }
    }
}
