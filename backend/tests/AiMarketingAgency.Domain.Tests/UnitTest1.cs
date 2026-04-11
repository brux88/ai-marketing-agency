using AiMarketingAgency.Domain.Entities;
using AiMarketingAgency.Domain.Enums;
using AiMarketingAgency.Domain.ValueObjects;

namespace AiMarketingAgency.Domain.Tests;

public class AgencyTests
{
    [Fact]
    public void Agency_DefaultValues_AreCorrect()
    {
        var agency = new Agency();

        Assert.Equal(ApprovalMode.Manual, agency.ApprovalMode);
        Assert.Equal(7, agency.AutoApproveMinScore);
        Assert.True(agency.IsActive);
        Assert.NotNull(agency.BrandVoice);
        Assert.NotNull(agency.TargetAudience);
        Assert.Empty(agency.ContentSources);
        Assert.Empty(agency.GeneratedContents);
    }

    [Fact]
    public void Agency_CanSetAllProperties()
    {
        var tenantId = Guid.NewGuid();
        var agency = new Agency
        {
            TenantId = tenantId,
            Name = "Test Agency",
            ProductName = "Test Product",
            Description = "A test agency",
            WebsiteUrl = "https://example.com",
            ApprovalMode = ApprovalMode.AutoApproveAboveScore,
            AutoApproveMinScore = 8,
            BrandVoice = new BrandVoice
            {
                Tone = "casual",
                Language = "en",
                Keywords = new List<string> { "innovation", "quality" },
                ForbiddenWords = new List<string> { "spam" }
            }
        };

        Assert.Equal(tenantId, agency.TenantId);
        Assert.Equal("Test Agency", agency.Name);
        Assert.Equal("Test Product", agency.ProductName);
        Assert.Equal(ApprovalMode.AutoApproveAboveScore, agency.ApprovalMode);
        Assert.Equal(8, agency.AutoApproveMinScore);
        Assert.Equal("casual", agency.BrandVoice.Tone);
        Assert.Equal("en", agency.BrandVoice.Language);
        Assert.Equal(2, agency.BrandVoice.Keywords.Count);
    }
}

public class GeneratedContentTests
{
    [Fact]
    public void GeneratedContent_DefaultValues_AreCorrect()
    {
        var content = new GeneratedContent();

        Assert.Equal(ContentStatus.Draft, content.Status);
        Assert.False(content.AutoApproved);
        Assert.Null(content.ApprovedAt);
        Assert.Null(content.ApprovedBy);
        Assert.Equal(0m, content.OverallScore);
    }

    [Fact]
    public void GeneratedContent_Scores_CanBeSet()
    {
        var content = new GeneratedContent
        {
            Title = "Test Article",
            Body = "Article body content",
            QualityScore = 8.5m,
            RelevanceScore = 7.0m,
            SeoScore = 9.0m,
            BrandVoiceScore = 8.0m,
            OverallScore = 8.1m,
            ScoreExplanation = "Good quality content",
            ContentType = ContentType.BlogPost
        };

        Assert.Equal(8.5m, content.QualityScore);
        Assert.Equal(7.0m, content.RelevanceScore);
        Assert.Equal(9.0m, content.SeoScore);
        Assert.Equal(8.0m, content.BrandVoiceScore);
        Assert.Equal(8.1m, content.OverallScore);
        Assert.Equal(ContentType.BlogPost, content.ContentType);
    }

    [Fact]
    public void GeneratedContent_AutoApproval_Tracking()
    {
        var content = new GeneratedContent
        {
            AutoApproved = true,
            Status = ContentStatus.Approved,
            ApprovedAt = DateTime.UtcNow,
        };

        Assert.True(content.AutoApproved);
        Assert.Equal(ContentStatus.Approved, content.Status);
        Assert.NotNull(content.ApprovedAt);
    }
}

public class BrandVoiceTests
{
    [Fact]
    public void BrandVoice_DefaultValues()
    {
        var voice = new BrandVoice();

        Assert.Equal("professional", voice.Tone);
        Assert.Equal("it", voice.Language);
        Assert.Empty(voice.Keywords);
        Assert.Empty(voice.ForbiddenWords);
        Assert.Empty(voice.ExamplePhrases);
    }
}

public class SubscriptionTests
{
    [Fact]
    public void Subscription_DefaultValues()
    {
        var sub = new Subscription();

        Assert.Equal(PlanTier.FreeTrial, sub.PlanTier);
        Assert.Equal(1, sub.MaxAgencies);
        Assert.Equal(20, sub.MaxJobsPerMonth);
    }

    [Fact]
    public void Subscription_PlanLimits()
    {
        var sub = new Subscription
        {
            PlanTier = PlanTier.Pro,
            MaxAgencies = 10,
            MaxJobsPerMonth = 500,
            Status = SubscriptionStatus.Active
        };

        Assert.Equal(PlanTier.Pro, sub.PlanTier);
        Assert.Equal(10, sub.MaxAgencies);
        Assert.Equal(500, sub.MaxJobsPerMonth);
        Assert.Equal(SubscriptionStatus.Active, sub.Status);
    }
}

public class EnumTests
{
    [Fact]
    public void ApprovalMode_HasCorrectValues()
    {
        Assert.Equal(1, (int)ApprovalMode.Manual);
        Assert.Equal(2, (int)ApprovalMode.AutoApprove);
        Assert.Equal(3, (int)ApprovalMode.AutoApproveAboveScore);
    }

    [Fact]
    public void ContentStatus_HasCorrectValues()
    {
        Assert.Equal(1, (int)ContentStatus.Draft);
        Assert.Equal(2, (int)ContentStatus.InReview);
        Assert.Equal(3, (int)ContentStatus.Approved);
        Assert.Equal(4, (int)ContentStatus.Published);
        Assert.Equal(5, (int)ContentStatus.Rejected);
    }

    [Fact]
    public void LlmProviderType_HasAllProviders()
    {
        var values = Enum.GetValues<LlmProviderType>();
        Assert.True(values.Length >= 5);
        Assert.Contains(LlmProviderType.OpenAI, values);
        Assert.Contains(LlmProviderType.Anthropic, values);
        Assert.Contains(LlmProviderType.NanoBanana, values);
    }

    [Fact]
    public void AgentType_HasAllAgents()
    {
        var values = Enum.GetValues<AgentType>();
        Assert.Contains(AgentType.ContentWriter, values);
        Assert.Contains(AgentType.SocialManager, values);
        Assert.Contains(AgentType.Newsletter, values);
        Assert.Contains(AgentType.Analytics, values);
    }
}
