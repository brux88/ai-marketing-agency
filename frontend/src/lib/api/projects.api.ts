import { apiClient } from "./client";
import type { ApiResponse, Project } from "@/types/api";

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

  delete: async (agencyId: string, projectId: string) => {
    await apiClient.delete(`/api/v1/agencies/${agencyId}/projects/${projectId}`);
  },

  extractBrand: async (agencyId: string, projectId: string) => {
    const res = await apiClient.post<ApiResponse<Project>>(
      `/api/v1/agencies/${agencyId}/projects/${projectId}/extract-brand`,
      {}
    );
    return res.data;
  },
};
