import { apiClient } from "./client";
import type { ApiResponse } from "@/types/api";

export interface CalendarEntry {
  id: string;
  contentId: string;
  contentTitle: string;
  contentType: string;
  platform?: string | null;
  scheduledAt: string;
  publishedAt?: string | null;
  status: string;
  errorMessage?: string | null;
  postUrl?: string | null;
}

export const calendarApi = {
  listForProject: (agencyId: string, projectId: string, from?: string, to?: string) => {
    const params = new URLSearchParams();
    if (from) params.set("from", from);
    if (to) params.set("to", to);
    const qs = params.toString();
    const url = `/api/v1/agencies/${agencyId}/projects/${projectId}/calendar${qs ? `?${qs}` : ""}`;
    return apiClient.get<ApiResponse<CalendarEntry[]>>(url);
  },

  schedule: (
    agencyId: string,
    contentId: string,
    data: { scheduledAt: string; platform?: number | null }
  ) =>
    apiClient.post<ApiResponse<CalendarEntry>>(
      `/api/v1/agencies/${agencyId}/content/${contentId}/schedule`,
      data
    ),

  publishNow: (agencyId: string, entryId: string) =>
    apiClient.post<ApiResponse<{ success: boolean; message?: string; postUrl?: string }>>(
      `/api/v1/agencies/${agencyId}/calendar/${entryId}/publish-now`,
      {}
    ),

  delete: (agencyId: string, entryId: string) =>
    apiClient.delete<ApiResponse<object>>(`/api/v1/agencies/${agencyId}/calendar/${entryId}`),
};
