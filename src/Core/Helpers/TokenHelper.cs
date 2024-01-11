using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Core.Helpers
{
    public interface ITokenHelper
    {
        TokenModel GenerateToken(Guid userId, string userName);
        string TokenEncode(Guid token);
        Guid TokenDecode(string encodedToken);
    }

    public class TokenHelper : ITokenHelper
    {
        private readonly CoreSettings _coreSettings;

        public TokenHelper(IOptions<CoreSettings> coreOptions)
        {
            _coreSettings = coreOptions.Value;
        }

        public TokenModel GenerateToken(Guid userId, string userName)
        {
            var key = Encoding.ASCII.GetBytes(_coreSettings.Auth.JwtSecret);
            
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new ClaimsIdentity(new[]
            {
                new Claim("System", _coreSettings.Auth.System),
                new Claim("IsAuthenticated", "True"),
                new Claim("UserId", $"{userId}"),
                new Claim("UserName", $"{userName}"),
                new Claim("SessionId", Guid.NewGuid().ToString()),
                new Claim("Provider", Environment.MachineName),
            });

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddMinutes(_coreSettings.Auth.TokenDuration),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var response = new TokenModel("Bearer", tokenHandler.WriteToken(token), _coreSettings.Auth.TokenDuration);
            
            return response;
        }
        
        public string TokenEncode(Guid token)
        {
            string encoded = Convert.ToBase64String(token.ToByteArray());
            encoded = encoded.Replace("/", "_").Replace("+", "-");
            return encoded.Substring(0, 22);
        }

        public Guid TokenDecode(string encodedToken)
        {
            encodedToken = encodedToken.Replace("_", "/").Replace("-", "+");
            byte[] buffer = Convert.FromBase64String(encodedToken + "==");
            return new Guid(buffer);
        }
    }
}

public record TokenModel(string Type,string Token, int ExpireIn);