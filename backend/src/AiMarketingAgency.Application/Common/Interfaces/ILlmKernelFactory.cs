using Microsoft.SemanticKernel;

namespace AiMarketingAgency.Application.Common.Interfaces;

public interface ILlmKernelFactory
{
    Task<Kernel> CreateKernelAsync(Guid agencyId, CancellationToken ct = default);
}
