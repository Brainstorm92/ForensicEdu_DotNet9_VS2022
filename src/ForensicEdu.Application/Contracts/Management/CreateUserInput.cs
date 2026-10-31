using ForensicEdu.Domain.Enums;
namespace ForensicEdu.Application.Contracts.Management;
public record CreateUserInput(string Nombres,string Apellidos,string Email,string Password,UserRole Role,int ValidDays=0,string Dni="",string Phone="");
