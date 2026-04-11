namespace AiMarketingAgency.Infrastructure.Ai.ImageGeneration;

public static class InfographicPromptBuilder
{
    public static string BuildPrompt(string title, string[] dataPoints, string? brandColor = null)
    {
        var color = brandColor ?? "#2563EB";
        var dataSection = string.Join("\n", dataPoints.Select((d, i) => $"- Point {i + 1}: {d}"));

        return $"""
            Create a clean, modern infographic with the following specifications:

            Title: {title}

            Data Points:
            {dataSection}

            Style:
            - Clean, modern design with a white background
            - Primary accent color: {color}
            - Use icons and simple charts/graphs to visualize data
            - Professional business presentation style
            - Include a clear visual hierarchy
            - Minimal text, maximum visual impact
            - 1080x1350 portrait orientation (Instagram-optimized)
            """;
    }
}
