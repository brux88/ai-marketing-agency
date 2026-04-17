import { apiClient } from "./client";
import type { ApiResponse } from "@/types/api";

export interface BillingUsage {
  plan: string;
  status: string;
  agenciesUsed: number;
  maxAgencies: number;
  jobsUsed: number;
  maxJobs: number;
  currentPeriodEnd?: string | null;
  trialEndsAt?: string | null;
}

export const billingApi = {
  getUsage: () =>
    apiClient.get<ApiResponse<BillingUsage>>("/api/v1/billing/usage").then((r) => r.data),

  createCheckoutSession: (priceId: string, successUrl: string, cancelUrl: string) =>
    apiClient
      .post<ApiResponse<{ url: string }>>("/api/v1/billing/checkout-session", {
        priceId,
        successUrl,
        cancelUrl,
      })
      .then((r) => r.data),

  createPortalSession: (returnUrl: string) =>
    apiClient
      .post<ApiResponse<{ url: string }>>("/api/v1/billing/portal-session", {
        returnUrl,
      })
      .then((r) => r.data),

  getPrices: () =>
    apiClient
      .get<ApiResponse<{ basic: string; pro: string; enterprise: string }>>("/api/v1/billing/prices")
      .then((r) => r.data),
};
