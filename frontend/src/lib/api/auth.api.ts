import { apiClient } from "./client";
import type { ApiResponse, AuthResponse } from "@/types/api";

export const authApi = {
  register: (data: { email: string; password: string; fullName: string; companyName?: string }) =>
    apiClient.post<ApiResponse<AuthResponse>>("/api/v1/auth/register", data),

  login: (data: { email: string; password: string }) =>
    apiClient.post<ApiResponse<AuthResponse>>("/api/v1/auth/login", data),

  refresh: (refreshToken: string) =>
    apiClient.post<ApiResponse<AuthResponse>>("/api/v1/auth/refresh", { refreshToken }),
};
