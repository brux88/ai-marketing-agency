import { apiClient } from "./client";
import type { ApiResponse } from "@/types/api";

export interface AdminStats {
  totalTenants: number;
  activeTenants: number;
  totalUsers: number;
  totalAgencies: number;
  totalProjects: number;
  jobsThisMonth: number;
  jobsTotal: number;
  contentsTotal: number;
  contentsThisMonth: number;
  activeSubscriptions: number;
  trialSubscriptions: number;
  planBreakdown: Record<string, number>;
  totalImageCostUsd: number;
  totalTextCostUsd: number;
  totalCostUsd: number;
}

export interface TenantDetail {
  id: string;
  name: string;
  plan: string;
  status: string;
  isActive: boolean;
  usersCount: number;
  agenciesCount: number;
  jobsThisMonth: number;
  totalContents: number;
  totalCostUsd: number;
  createdAt: string;
}

export const adminApi = {
  getStats: () =>
    apiClient.get<ApiResponse<AdminStats>>("/api/v1/admin/stats").then((r) => r.data),
  getTenants: () =>
    apiClient.get<ApiResponse<TenantDetail[]>>("/api/v1/admin/tenants").then((r) => r.data),
};
