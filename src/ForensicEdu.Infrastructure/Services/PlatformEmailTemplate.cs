using System.Net;
using ForensicEdu.Application.Common.Interfaces;
namespace ForensicEdu.Infrastructure.Services;

// Inline styles and presentation tables also work when the mail client removes media queries.
public static class PlatformEmailTemplate
{
    public static string Render(PlatformEmailMessage message)
    {
        static string E(string? value) => WebUtility.HtmlEncode(value ?? "");
        static string P(string value) => E(value).Replace("\n", "<br>");
        var action = "";
        if (Uri.TryCreate(message.ActionUrl, UriKind.Absolute, out var uri) &&
            (uri.Scheme == "https" || (uri.Scheme == "http" && uri.IsLoopback)) && string.IsNullOrEmpty(uri.UserInfo))
            action = $"<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"margin:26px 0\"><tr><td><a class=\"action\" href=\"{E(uri.AbsoluteUri)}\" style=\"display:inline-block;background:#00796d;border:1px solid #00796d;border-radius:10px;padding:17px 25px;font-size:14px;line-height:20px;font-weight:700;color:#ffffff;text-decoration:none\">{E(message.ActionLabel)} &rarr;</a></td></tr></table>";
        var highlight = string.IsNullOrEmpty(message.Highlight) ? "" : $"<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"margin-top:26px;border:1px solid #d6e9e3;border-radius:10px;background:#f1f8f5\"><tr><td style=\"padding:16px 18px;font-size:13px;line-height:1.7;color:#007165;font-weight:600\">{P(message.Highlight)}</td></tr></table>";
        var footnote = string.IsNullOrEmpty(message.Footnote) ? "" : $"<p style=\"font-size:13px;line-height:1.7;margin:24px 0 0;color:#52616c\">{P(message.Footnote)}</p>";
        return $$$"""
        <!doctype html><html lang="es"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>{{{E(message.Heading)}}}</title>
        <style>@media(max-width:620px){.outer{padding:16px 12px!important}.body{padding:30px 24px!important}.brand{padding:24px!important}.title{font-size:28px!important}.action{display:block!important;text-align:center!important}}</style></head>
        <body style="margin:0;background:#f1f5f4;color:#172434;font-family:Arial,Helvetica,sans-serif;-webkit-text-size-adjust:100%">
        <div style="display:none;max-height:0;overflow:hidden;opacity:0">{{{E(message.Heading)}}}</div>
        <table role="presentation" cellpadding="0" cellspacing="0" width="100%"><tr><td class="outer" align="center" style="padding:40px 20px">
        <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="max-width:600px;border:1px solid #dce6e3;border-radius:20px;overflow:hidden;background:#ffffff">
        <tr><td class="brand" style="padding:28px 40px;background:#111c2d;border-bottom:4px solid #3ddac0"><table role="presentation" width="100%" cellpadding="0" cellspacing="0"><tr><td><div style="font-size:18px;font-weight:700;letter-spacing:1px;color:#ffffff">ACADEMIA <span style="color:#62e7d0">FORENSE</span></div><div style="color:#bbc9d1;font-size:11px;letter-spacing:1.7px;margin-top:8px">CIENCIA · FORMACIÓN · PRECISIÓN</div></td><td align="right" style="color:#62e7d0;font-size:10px;letter-spacing:1px">TU CUENTA</td></tr></table></td></tr>
        <tr><td class="body" style="padding:38px 40px 32px"><div style="font-size:11px;color:#00796d;font-weight:700;letter-spacing:1.6px;margin-bottom:16px">{{{E(message.Category)}}}</div>
        <h1 class="title" style="font-size:32px;line-height:1.2;letter-spacing:-1px;margin:0 0 20px;color:#111c2d">{{{E(message.Heading)}}}</h1>
        <p style="font-size:15px;line-height:1.7;margin:0 0 12px">Hola,</p><p style="font-size:15px;line-height:1.7;margin:0;color:#52616c">{{{P(message.Body)}}}</p>
        {{{action}}}{{{highlight}}}{{{footnote}}}
        </td></tr></table></td></tr></table></body></html>
        """;
    }
}
