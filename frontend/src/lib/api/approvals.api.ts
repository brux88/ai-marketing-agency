import { apiClient } from "./client";
import type { ApiResponse, PendingApproval } from "@/types/api";

export interface ApprovalHistoryItem {
  id: string;
  title: string;
  body: string;
  contentType: number;
  status: number;
  projectId?: string | null;
  projectName?: string | null;
  overallScore: number;
  autoApproved: boolean;
  imageUrl?: string | null;
  createdAt: string;
  approvedAt?: string | null;
  publishedAt?: string | null;
}

const base = (agencyId: string) => `/api/v1/agencies/${agencyId}/approvals`;

export const approvalsApi = {
  list: (agencyId: string) =>
    apiClient.get<ApiResponse<PendingApproval[]>>(base(agencyId)),

  approve: (agencyId: string, contentId: string) =>
    apiClient.put<ApiResponse<object>>(`${base(agencyId)}/${contentId}/approve`),

  reject: (agencyId: string, contentId: string) =>
    apiClient.put<ApiResponse<object>>(`${base(agencyId)}/${contentId}/reject`),

  history: (agencyId: string, filter: "all" | "approved" | "rejected" = "all", take = 100) => {
    const params = new URLSearchParams();
    if (filter !== "all") params.set("filter", filter);
    params.set("take", String(take));
    return apiClient.get<ApiResponse<ApprovalHistoryItem[]>>(`${base(agencyId)}/history?${params}`);
  },
};
