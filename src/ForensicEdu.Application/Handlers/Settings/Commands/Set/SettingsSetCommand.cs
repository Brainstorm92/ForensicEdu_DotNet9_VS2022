using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Settings;
namespace ForensicEdu.Application.Handlers.Settings.Commands.Set;

public sealed record SettingsSetCommand(string Key, SettingRequest Request) : IRequest<ApplicationResult>;
