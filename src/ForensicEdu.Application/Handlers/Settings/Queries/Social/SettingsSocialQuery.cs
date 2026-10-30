using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Settings.Queries.Social;

public sealed record SettingsSocialQuery() : IRequest<ApplicationResult>;
