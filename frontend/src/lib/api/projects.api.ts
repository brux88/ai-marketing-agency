import { apiClient } from "./client";
import type { ApiResponse, ApprovalMode, Project } from "@/types/api";

export interface ProjectCostStats {
  totalContents: number;
  totalTextCostUsd: number;
  totalImageCostUsd: number;
  totalCostUsd: number;
  last30DaysTextCostUsd: number;
  last30DaysImageCostUsd: number;
  last30DaysCostUsd: number;
}

export interface AgencyCostStats extends ProjectCostStats {
  projects: Array<{
    projectId: string;
    projectName: string;
    contents: number;
    textCostUsd: number;
    imageCostUsd: number;
    totalCostUsd: number;
  }>;
}

export const projectsApi = {
  list: async (agencyId: string) => {
    const res = await apiClient.get<ApiResponse<Project[]>>(`/api/v1/agencies/${agencyId}/projects`);
    return res.data;
  },

  get: async (agencyId: string, projectId: string) => {
    const res = await apiClient.get<ApiResponse<Project>>(`/api/v1/agencies/${agencyId}/projects/${projectId}`);
    return res.data;
  },

  create: async (agencyId: string, data: Partial<Project>) => {
    const res = await apiClient.post<ApiResponse<Project>>(`/api/v1/agencies/${agencyId}/projects`, data);
    return res.data;
  },

  update: async (agencyId: string, projectId: string, data: Partial<Project>) => {
    const res = await apiClient.put<ApiResponse<Project>>(`/api/v1/agencies/${agencyId}/projects/${projectId}`, data);
    return res.data;
  },

  delete: async (agencyId: string, projectId: string, password: string) => {
    await apiClient.delete(`/api/v1/agencies/${agencyId}/projects/${projectId}?password=${encodeURIComponent(password)}`);
  },

  extractBrand: async (agencyId: string, projectId: string) => {
    const res = await apiClient.post<ApiResponse<Project>>(
      `/api/v1/agencies/${agencyId}/projects/${projectId}/extract-brand`,
      {}
    );
    return res.data;
  },

  getPrompts: async (agencyId: string, projectId: string) => {
    const res = await apiClient.get<ApiResponse<ProjectPrompts>>(
      `/api/v1/agencies/${agencyId}/projects/${projectId}/prompts`
    );
    return res.data;
  },

  updateApprovalMode: async (
    agencyId: string,
    projectId: string,
    approvalMode: ApprovalMode | null,
    autoApproveMinScore: number | null,
    autoScheduleOnApproval: boolean | null
  ) =>
    apiClient.put<ApiResponse<null>>(
      `/api/v1/agencies/${agencyId}/projects/${projectId}/approval-mode`,
      { approvalMode, autoApproveMinScore, autoScheduleOnApproval }
    ),

  updateSocialPlatforms: async (
    agencyId: string,
    projectId: string,
    enabledSocialPlatforms: string | null
  ) =>
    apiClient.put<ApiResponse<null>>(
      `/api/v1/agencies/${agencyId}/projects/${projectId}/social-platforms`,
      { enabledSocialPlatforms }
    ),

  updateImageSettings: async (
    agencyId: string,
    projectId: string,
    enableLogoOverlay: boolean | null,
    logoOverlayPosition: number | null,
    logoUrl: string | null,
    logoOverlayMode: number | null = null,
    brandBannerColor: string | null = null
  ) =>
    apiClient.put<ApiResponse<null>>(
      `/api/v1/agencies/${agencyId}/projects/${projectId}/image-settings`,
      { enableLogoOverlay, logoOverlayPosition, logoUrl, logoOverlayMode, brandBannerColor }
    ),

  uploadLogo: async (agencyId: string, projectId: string, file: File) => {
    const form = new FormData();
    form.append("file", file);
    return apiClient.postForm<ApiResponse<{ url: string }>>(
      `/api/v1/agencies/${agencyId}/projects/${projectId}/logo-upload`,
      form
    );
  },

  getCostStats: async (agencyId: string, projectId: string) => {
    const res = await apiClient.get<ApiResponse<ProjectCostStats>>(
      `/api/v1/agencies/${agencyId}/projects/${projectId}/cost-stats`
    );
    return res.data;
  },

  resetAnalytics: (agencyId: string, projectId: string) =>
    apiClient.post<ApiResponse<object>>(
      `/api/v1/agencies/${agencyId}/projects/${projectId}/reset-analytics`
    ),

  getAgencyCostStats: async (agencyId: string) => {
    const res = await apiClient.get<ApiResponse<AgencyCostStats>>(
      `/api/v1/agencies/${agencyId}/cost-stats`
    );
    return res.data;
  },

  deleteContent: async (agencyId: string, contentId: string) =>
    apiClient.delete(`/api/v1/agencies/${agencyId}/content/${contentId}`),

  updatePrompts: async (
    agencyId: string,
    projectId: string,
    data: Partial<ProjectPrompts>
  ) => {
    const res = await apiClient.put<ApiResponse<ProjectPrompts>>(
      `/api/v1/agencies/${agencyId}/projects/${projectId}/prompts`,
      data
    );
    return res.data;
  },
};

export interface ProjectPrompts {
  blogPromptTemplate?: string | null;
  socialPromptTemplate?: string | null;
  newsletterPromptTemplate?: string | null;
  extractedContext?: string | null;
  extractedContextAt?: string | null;
  logoUrl?: string | null;
}
