import { apiClient } from "./client";
import type { ApiResponse, PendingApproval } from "@/types/api";

const base = (agencyId: string) => `/api/v1/agencies/${agencyId}/approvals`;

export const approvalsApi = {
  list: (agencyId: string) =>
    apiClient.get<ApiResponse<PendingApproval[]>>(base(agencyId)),

  approve: (agencyId: string, contentId: string) =>
    apiClient.put<ApiResponse<object>>(`${base(agencyId)}/${contentId}/approve`),

  reject: (agencyId: string, contentId: string) =>
    apiClient.put<ApiResponse<object>>(`${base(agencyId)}/${contentId}/reject`),
};
