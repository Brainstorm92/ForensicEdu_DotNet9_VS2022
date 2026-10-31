using ForensicEdu.Domain.Entities;
namespace ForensicEdu.Application.Common.Interfaces;

public interface IPersistenceErrors { bool IsUniqueViolation(Exception exception); }
