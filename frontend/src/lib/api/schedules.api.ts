import { apiClient } from "./client";
import type { ApiResponse, ContentSchedule } from "@/types/api";

const base = (agencyId: string) => `/api/v1/agencies/${agencyId}/schedules`;

export const schedulesApi = {
  list: (agencyId: string) =>
    apiClient.get<ApiResponse<ContentSchedule[]>>(base(agencyId)),

  create: (agencyId: string, data: {
    name: string;
    days: number;
    timeOfDay: string;
    timeZone: string;
    scheduleType?: number;
    agentType: number;
    publishContentType?: number | null;
    maxPostsPerPlatform?: number | null;
    projectId?: string | null;
    input?: string | null;
    enabledSocialPlatforms?: string | null;
    approvalMode?: number | null;
    autoApproveMinScore?: number | null;
    autoScheduleOnApproval?: boolean | null;
  }) => apiClient.post<ApiResponse<ContentSchedule>>(base(agencyId), data),

  update: (agencyId: string, id: string, data: {
    name: string;
    days: number;
    timeOfDay: string;
    timeZone: string;
    scheduleType?: number;
    agentType: number;
    publishContentType?: number | null;
    maxPostsPerPlatform?: number | null;
    projectId?: string | null;
    input?: string | null;
    isActive: boolean;
    enabledSocialPlatforms?: string | null;
    approvalMode?: number | null;
    autoApproveMinScore?: number | null;
    autoScheduleOnApproval?: boolean | null;
  }) => apiClient.put<ApiResponse<ContentSchedule>>(`${base(agencyId)}/${id}`, data),

  delete: (agencyId: string, id: string) =>
    apiClient.delete<ApiResponse<object>>(`${base(agencyId)}/${id}`),
};
