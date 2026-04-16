import { apiClient } from "./client";
import type { ApiResponse, SocialConnector, PublishResult } from "@/types/api";

const base = (agencyId: string) => `/api/v1/agencies/${agencyId}/connectors`;

export const connectorsApi = {
  list: (agencyId: string) =>
    apiClient.get<ApiResponse<SocialConnector[]>>(base(agencyId)),

  connect: (agencyId: string, data: {
    platform: number;
    projectId?: string | null;
    accessToken: string;
    refreshToken?: string;
    accountId?: string;
    accountName?: string;
    profileImageUrl?: string;
    tokenExpiresAt?: string;
  }) =>
    apiClient.post<ApiResponse<SocialConnector>>(`${base(agencyId)}/connect`, data),

  disconnect: (agencyId: string, connectorId: string) =>
    apiClient.delete<ApiResponse<object>>(`${base(agencyId)}/${connectorId}`),

  publish: (agencyId: string, contentId: string, platform: number) =>
    apiClient.post<ApiResponse<PublishResult>>(
      `${base(agencyId)}/publish/${contentId}/${platform}`
    ),
};
