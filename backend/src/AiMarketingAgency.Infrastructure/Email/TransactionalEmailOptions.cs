namespace AiMarketingAgency.Infrastructure.Email;

public class TransactionalEmailOptions
{
    public const string SectionName = "TransactionalEmail";

    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 465;
    public bool UseSsl { get; set; } = true;
    public string NoReplyEmail { get; set; } = string.Empty;
    public string NoReplyPassword { get; set; } = string.Empty;
    public string SupportEmail { get; set; } = string.Empty;
    public string SupportPassword { get; set; } = string.Empty;
    public string SenderName { get; set; } = "wepostai.com";
    public string FrontendBaseUrl { get; set; } = string.Empty;
}
