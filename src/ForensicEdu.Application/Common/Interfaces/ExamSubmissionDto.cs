using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ForensicEdu.Application.Common.Interfaces;

public record ExamSubmissionDto(
    string EnrollmentId,
    string UserId,
    Dictionary<string, int> SelectedAnswers, // QuestionId -> SelectedOptionIndex
    int DurationSeconds
);
