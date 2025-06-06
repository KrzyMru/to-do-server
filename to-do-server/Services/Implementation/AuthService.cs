using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using to_do_server.Services.Interface;

namespace to_do_server.Services.Implementation
{
    public class AuthService(IConfiguration config) : IAuthService
    {
        private readonly IConfiguration _config = config;

        public string GenerateToken(int userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var issuer = _config.GetValue<string>("jwtIssuer");
            var audience = _config.GetValue<string>("jwtAudience");
            var key = _config.GetValue<string>("jwtKey");

            var claims = new List<Claim>
            {
                new("id", userId.ToString()),
            };
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var secToken = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddHours(24),
                SigningCredentials = credentials
            };
            var token = tokenHandler.CreateToken(secToken);

            return tokenHandler.WriteToken(token);
        }

        public bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var issuer = _config.GetValue<string>("jwtIssuer");
            var audience = _config.GetValue<string>("jwtAudience");
            var key = _config.GetValue<string>("jwtKey");

            var validationOptions = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
            };

            try
            {
                tokenHandler.ValidateToken(token, validationOptions, out SecurityToken validatedToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string GetUserId(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var decodedToken = tokenHandler.ReadJwtToken(token);
            var userId = decodedToken.Claims.First(claim => claim.Type == "id").Value;

            return userId;
        }
    }
}
