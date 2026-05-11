using Microsoft.Extensions.Logging;

namespace AiMarketingAgency.Infrastructure.Ai;

/// <summary>
/// DelegatingHandler that retries transient LLM provider errors with exponential backoff.
/// Anthropic returns HTTP 529 ("Overloaded") under load; that, plus the usual 429/5xx,
/// are worth retrying a few times instead of failing the whole agent job.
/// </summary>
public class LlmRetryHandler : DelegatingHandler
{
    private static readonly TimeSpan[] Delays =
    {
        TimeSpan.FromSeconds(3),
        TimeSpan.FromSeconds(8),
        TimeSpan.FromSeconds(20),
        TimeSpan.FromSeconds(45),
    };

    private readonly ILogger? _logger;

    public LlmRetryHandler(ILogger? logger = null)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Buffer the request body so we can resend it on retry.
        if (request.Content is not null)
        {
            await request.Content.LoadIntoBufferAsync().ConfigureAwait(false);
        }

        HttpResponseMessage? response = null;
        for (var attempt = 0; ; attempt++)
        {
            response?.Dispose();

            try
            {
                response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException ex) when (attempt < Delays.Length)
            {
                _logger?.LogWarning(
                    ex,
                    "LLM request network error; retrying in {Delay}s (attempt {Attempt}/{Max})",
                    Delays[attempt].TotalSeconds, attempt + 1, Delays.Length);
                await Task.Delay(Delays[attempt], cancellationToken).ConfigureAwait(false);
                continue;
            }

            var status = (int)response.StatusCode;
            if (!IsTransient(status) || attempt >= Delays.Length)
            {
                return response;
            }

            _logger?.LogWarning(
                "LLM provider returned transient HTTP {Status}; retrying in {Delay}s (attempt {Attempt}/{Max})",
                status, Delays[attempt].TotalSeconds, attempt + 1, Delays.Length);
            await Task.Delay(Delays[attempt], cancellationToken).ConfigureAwait(false);
        }
    }

    private static bool IsTransient(int statusCode) => statusCode is 408 or 425 or 429 or 500 or 502 or 503 or 504 or 529;
}
