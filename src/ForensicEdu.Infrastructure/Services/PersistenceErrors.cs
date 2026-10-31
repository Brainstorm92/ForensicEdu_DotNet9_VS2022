using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace ForensicEdu.Infrastructure.Services;

public sealed class PersistenceErrors : IPersistenceErrors
{
    public bool IsUniqueViolation(Exception exception) => exception.InnerException is Npgsql.PostgresException { SqlState: "23505" };
}
