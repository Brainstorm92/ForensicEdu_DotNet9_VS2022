using System;
using System.Collections.Generic;
using ForensicEdu.Domain.Enums;

namespace ForensicEdu.Domain.Entities;

public class Question
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CourseId { get; set; } = string.Empty;
    public Course? Course { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string OptionsJson { get; set; } = "[]";
    public int CorrectAnswerIndex { get; set; }
    public string Explanation { get; set; } = string.Empty;
}
