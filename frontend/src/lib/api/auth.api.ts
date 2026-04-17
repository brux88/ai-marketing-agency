import { apiClient } from "./client";
import type { ApiResponse, AuthResponse } from "@/types/api";

export const authApi = {
  register: (data: { email: string; password: string; fullName: string; companyName?: string }) =>
    apiClient.post<ApiResponse<AuthResponse>>("/api/v1/auth/register", data),

  login: (data: { email: string; password: string }) =>
    apiClient.post<ApiResponse<AuthResponse>>("/api/v1/auth/login", data),

  refresh: (refreshToken: string) =>
    apiClient.post<ApiResponse<AuthResponse>>("/api/v1/auth/refresh", { refreshToken }),

  confirmEmail: (token: string) =>
    apiClient.post<ApiResponse<string>>("/api/v1/auth/confirm-email", { token }),

  forgotPassword: (email: string) =>
    apiClient.post<ApiResponse<string>>("/api/v1/auth/forgot-password", { email }),

  resetPassword: (token: string, newPassword: string) =>
    apiClient.post<ApiResponse<string>>("/api/v1/auth/reset-password", { token, newPassword }),

  resendConfirmation: (email: string) =>
    apiClient.post<ApiResponse<string>>("/api/v1/auth/resend-confirmation", { email }),

  requestAccountDeletion: () =>
    apiClient.post<ApiResponse<string>>("/api/v1/auth/request-account-deletion", {}),

  confirmAccountDeletion: (token: string) =>
    apiClient.post<ApiResponse<string>>("/api/v1/auth/confirm-account-deletion", { token }),
};
