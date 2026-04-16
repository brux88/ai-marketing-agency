import { apiClient } from "./client";
import type { ApiResponse } from "@/types/api";

export interface NotificationItem {
  id: string;
  agencyId: string;
  jobId?: string | null;
  projectId?: string | null;
  type: string;
  title: string;
  body?: string | null;
  link?: string | null;
  read: boolean;
  createdAt: string;
  readAt?: string | null;
}

export interface NotificationListResponse {
  items: NotificationItem[];
  unreadCount: number;
}

export const notificationsApi = {
  list: (unreadOnly = false, take = 50) =>
    apiClient.get<ApiResponse<NotificationListResponse>>(
      `/api/v1/notifications?unreadOnly=${unreadOnly}&take=${take}`
    ),
  markRead: (id: string) =>
    apiClient.post<ApiResponse<object>>(`/api/v1/notifications/${id}/read`),
  markAllRead: () =>
    apiClient.post<ApiResponse<object>>(`/api/v1/notifications/read-all`),
};
