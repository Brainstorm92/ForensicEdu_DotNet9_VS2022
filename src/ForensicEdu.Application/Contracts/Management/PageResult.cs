using ForensicEdu.Domain.Enums;
namespace ForensicEdu.Application.Contracts.Management;

public record PageResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
