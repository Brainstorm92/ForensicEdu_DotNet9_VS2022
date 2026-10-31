using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ForensicEdu.Application.Common.Interfaces;

public interface IEvaluationService
{
    Task<ExamResultDto> SubmitExamAttemptAsync(ExamSubmissionDto submission, CancellationToken cancellationToken = default);
}
