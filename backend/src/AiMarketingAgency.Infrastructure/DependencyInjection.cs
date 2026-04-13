using AiMarketingAgency.Application.Agents;
using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Infrastructure.Ai;
using AiMarketingAgency.Infrastructure.Ai.Agents;
using AiMarketingAgency.Infrastructure.Ai.ImageGeneration;
using AiMarketingAgency.Infrastructure.Ai.Rag;
using AiMarketingAgency.Infrastructure.Ai.VideoGeneration;
using AiMarketingAgency.Infrastructure.Persistence;
using AiMarketingAgency.Infrastructure.Email;
using AiMarketingAgency.Infrastructure.Scheduling;
using AiMarketingAgency.Infrastructure.Security;
using AiMarketingAgency.Infrastructure.Social;
using AiMarketingAgency.Infrastructure.Telegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiMarketingAgency.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITenantContext, TenantContext>();

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                });
        });

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<IUsageGuard, UsageGuard>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISubscriptionService, Billing.StripeSubscriptionService>();
        services.AddScoped<ILlmKeyVault, LlmKeyVault>();

        // AI / LLM services
        services.AddHttpClient();
        services.AddScoped<ILlmKernelFactory, LlmKernelFactory>();
        services.AddScoped<IAgentJobProcessor, AgentJobProcessor>();
        services.AddSingleton<IImageGenerationServiceFactory, ImageGenerationServiceFactory>();
        services.AddSingleton<IVideoGenerationServiceFactory, VideoGenerationServiceFactory>();
        services.AddScoped<IImageOverlayService, ImageOverlayService>();

        // RAG pipeline
        services.AddScoped<RssFeedFetcher>();
        services.AddScoped<WebScraperFetcher>();
        services.AddScoped<EmbeddingService>();
        services.AddScoped<IContentFetcherService, ContentFetcherService>();
        services.AddHostedService<ContentSourceRefreshService>();

        // Social publishing
        services.AddScoped<ISocialPublishingServiceFactory, SocialPublishingServiceFactory>();

        // Email sending (default SMTP, SendGrid resolved at runtime)
        services.AddScoped<IEmailSendingService, SmtpEmailService>();

        // Telegram bot
        services.AddScoped<ITelegramBotService, TelegramBotService>();

        // Scheduler background service
        services.AddHostedService<SchedulerBackgroundService>();

        // Marketing agents
        services.AddScoped<IMarketingAgent, ContentWriterAgent>();
        services.AddScoped<IMarketingAgent, SocialManagerAgent>();
        services.AddScoped<IMarketingAgent, NewsletterAgent>();
        services.AddScoped<IMarketingAgent, AnalyticsAgent>();
        services.AddScoped<IMarketingAgent, ContentStrategistAgent>();
        services.AddScoped<IMarketingAgent, SeoOptimizerAgent>();

        return services;
    }
}
