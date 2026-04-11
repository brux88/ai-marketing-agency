import { apiClient } from "./client";
import type { Agency, ApiResponse, BrandVoice, TargetAudience } from "@/types/api";

export const agenciesApi = {
  list: () => apiClient.get<ApiResponse<Agency[]>>("/api/v1/agencies"),

  get: (id: string) => apiClient.get<ApiResponse<Agency>>(`/api/v1/agencies/${id}`),

  create: (data: Partial<Agency>) =>
    apiClient.post<ApiResponse<Agency>>("/api/v1/agencies", data),

  update: (id: string, data: Partial<Agency>) =>
    apiClient.put<ApiResponse<Agency>>(`/api/v1/agencies/${id}`, data),

  delete: (id: string) => apiClient.delete(`/api/v1/agencies/${id}`),

  updateBrandVoice: async (id: string, brandVoice: BrandVoice) => {
    const res = await apiClient.put<ApiResponse<null>>(`/api/v1/agencies/${id}/brand-voice`, { brandVoice });
    return res;
  },

  updateTargetAudience: async (id: string, targetAudience: TargetAudience) => {
    const res = await apiClient.put<ApiResponse<null>>(`/api/v1/agencies/${id}/target-audience`, { targetAudience });
    return res;
  },

  updateApprovalMode: async (id: string, approvalMode: number, autoApproveMinScore: number) => {
    const res = await apiClient.put<ApiResponse<null>>(`/api/v1/agencies/${id}/approval-mode`, { approvalMode, autoApproveMinScore });
    return res;
  },

  updateDefaultLlm: async (id: string, defaultLlmProviderKeyId: string | null, imageLlmProviderKeyId: string | null) => {
    const res = await apiClient.put<ApiResponse<null>>(`/api/v1/agencies/${id}/default-llm`, { defaultLlmProviderKeyId, imageLlmProviderKeyId });
    return res;
  },
};
