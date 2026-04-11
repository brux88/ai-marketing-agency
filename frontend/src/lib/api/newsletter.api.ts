import { apiClient } from "./client";
import type { ApiResponse, EmailConnectorDto, NewsletterSubscriber, EmailSendResult } from "@/types/api";

const base = (agencyId: string) => `/api/v1/agencies/${agencyId}/newsletter`;

export const newsletterApi = {
  getConfig: (agencyId: string) =>
    apiClient.get<ApiResponse<EmailConnectorDto | null>>(`${base(agencyId)}/config`),

  saveConfig: (agencyId: string, data: {
    providerType: number;
    smtpHost?: string;
    smtpPort?: number;
    smtpUsername?: string;
    smtpPassword?: string;
    apiKey?: string;
    fromEmail: string;
    fromName: string;
  }) =>
    apiClient.post<ApiResponse<EmailConnectorDto>>(`${base(agencyId)}/config`, data),

  getSubscribers: (agencyId: string) =>
    apiClient.get<ApiResponse<NewsletterSubscriber[]>>(`${base(agencyId)}/subscribers`),

  addSubscriber: (agencyId: string, data: { email: string; name?: string }) =>
    apiClient.post<ApiResponse<NewsletterSubscriber>>(`${base(agencyId)}/subscribers`, data),

  removeSubscriber: (agencyId: string, subscriberId: string) =>
    apiClient.delete<ApiResponse<object>>(`${base(agencyId)}/subscribers/${subscriberId}`),

  send: (agencyId: string, contentId: string) =>
    apiClient.post<ApiResponse<EmailSendResult>>(`${base(agencyId)}/send/${contentId}`),
};
