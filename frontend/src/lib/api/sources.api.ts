import { apiClient } from "./client";
import type { ApiResponse, ContentSource } from "@/types/api";

export const sourcesApi = {
  list: (agencyId: string) =>
    apiClient.get<ApiResponse<ContentSource[]>>(
      `/api/v1/agencies/${agencyId}/sources`
    ),

  create: (
    agencyId: string,
    data: { type: number; url: string; name?: string }
  ) =>
    apiClient.post<ApiResponse<ContentSource>>(
      `/api/v1/agencies/${agencyId}/sources`,
      data
    ),

  delete: (agencyId: string, sourceId: string) =>
    apiClient.delete(`/api/v1/agencies/${agencyId}/sources/${sourceId}`),

  discover: (agencyId: string, projectId?: string) => {
    const params = projectId ? `?projectId=${projectId}` : "";
    return apiClient.post<ApiResponse<{ url: string; name: string; description: string; type: number }[]>>(
      `/api/v1/agencies/${agencyId}/sources/discover${params}`
    );
  },
};
