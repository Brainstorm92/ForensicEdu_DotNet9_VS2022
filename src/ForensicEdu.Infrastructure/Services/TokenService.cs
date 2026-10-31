using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace ForensicEdu.Infrastructure.Services;

public sealed class TokenService(IConfiguration config) : ITokenService
{
    public string Create(User user)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id), new Claim(ClaimTypes.Role, ((int)user.Role).ToString()) };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:Secret"]!));
        var token = new JwtSecurityToken(issuer: config["JwtSettings:Issuer"], audience: config["JwtSettings:Audience"],
            claims: claims, expires: DateTime.UtcNow.AddHours(8), signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
