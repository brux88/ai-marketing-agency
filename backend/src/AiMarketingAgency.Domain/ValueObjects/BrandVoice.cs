namespace AiMarketingAgency.Domain.ValueObjects;

public class BrandVoice
{
    public string Tone { get; set; } = "professional";
    public string Style { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
    public List<string> ExamplePhrases { get; set; } = new();
    public List<string> ForbiddenWords { get; set; } = new();
    public string Language { get; set; } = "it";
}
