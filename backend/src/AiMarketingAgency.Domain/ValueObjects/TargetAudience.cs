namespace AiMarketingAgency.Domain.ValueObjects;

public class TargetAudience
{
    public string Description { get; set; } = string.Empty;
    public string? AgeRange { get; set; }
    public List<string> Interests { get; set; } = new();
    public List<string> PainPoints { get; set; } = new();
    public List<PersonaProfile> Personas { get; set; } = new();
}

public class PersonaProfile
{
    public string Name { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? Description { get; set; }
}
