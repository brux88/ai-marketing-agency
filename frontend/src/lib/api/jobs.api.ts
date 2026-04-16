import { apiClient } from "./client";
import type { ApiResponse } from "@/types/api";

export interface JobListItem {
  id: string;
  agencyId: string;
  projectId?: string | null;
  agentType: string;
  status: string;
  input?: string | null;
  output?: string | null;
  errorMessage?: string | null;
  createdAt: string;
  startedAt?: string | null;
  completedAt?: string | null;
}

export const jobsApi = {
  list: (agencyId?: string, take = 50) => {
    const params = new URLSearchParams();
    if (agencyId) params.set("agencyId", agencyId);
    params.set("take", String(take));
    return apiClient.get<ApiResponse<JobListItem[]>>(`/api/v1/jobs?${params}`);
  },
};
