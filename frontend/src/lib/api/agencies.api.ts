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

  updateApprovalMode: async (id: string, approvalMode: number, autoApproveMinScore: number, autoScheduleOnApproval: boolean = true) => {
    const res = await apiClient.put<ApiResponse<null>>(`/api/v1/agencies/${id}/approval-mode`, { approvalMode, autoApproveMinScore, autoScheduleOnApproval });
    return res;
  },

  updateDefaultLlm: async (id: string, defaultLlmProviderKeyId: string | null, imageLlmProviderKeyId: string | null) => {
    const res = await apiClient.put<ApiResponse<null>>(`/api/v1/agencies/${id}/default-llm`, { defaultLlmProviderKeyId, imageLlmProviderKeyId });
    return res;
  },

  updateImageSettings: async (id: string, enableLogoOverlay: boolean, logoOverlayPosition: number, logoUrl: string | null, logoOverlayMode: number = 0, brandBannerColor: string | null = null) => {
    const res = await apiClient.put<ApiResponse<null>>(`/api/v1/agencies/${id}/image-settings`, { enableLogoOverlay, logoOverlayPosition, logoUrl, logoOverlayMode, brandBannerColor });
    return res;
  },

  uploadLogo: async (id: string, file: File) => {
    const form = new FormData();
    form.append("file", file);
    const res = await apiClient.postForm<ApiResponse<{ url: string }>>(`/api/v1/agencies/${id}/logo-upload`, form);
    return res;
  },
};
