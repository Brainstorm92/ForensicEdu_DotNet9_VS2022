namespace ForensicEdu.Application.Common.Interfaces;

public sealed record PlatformEmailMessage(string Heading, string Body, string? ActionLabel = null,
    string? ActionUrl = null, string? Highlight = null, string? Footnote = null,
    string Category = "TU FORMACIÓN")
{
    public string ToPlainText() => string.Join("\n\n", new[] { Heading, Body,
        ActionUrl == null ? null : ActionLabel + ":\n" + ActionUrl, Highlight, Footnote }
        .Where(x => !string.IsNullOrWhiteSpace(x)));
}
