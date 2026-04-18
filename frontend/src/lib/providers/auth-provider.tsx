"use client";

import { createContext, useContext, useState, useEffect, useCallback, type ReactNode } from "react";
import { apiClient } from "@/lib/api/client";
import { authApi } from "@/lib/api/auth.api";
import type { UserInfo } from "@/types/api";

interface AuthContextType {
  user: UserInfo | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, fullName: string, companyName?: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

function isTokenExpired(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    // Consider expired if less than 60 seconds remaining
    return payload.exp * 1000 < Date.now() + 60_000;
  } catch {
    return true;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserInfo | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const restoreSession = async () => {
      const accessToken = localStorage.getItem("accessToken");
      const refreshToken = localStorage.getItem("refreshToken");
      const userData = localStorage.getItem("user");

      if (!accessToken || !userData) {
        setIsLoading(false);
        return;
      }

      // If access token is still valid, use it directly
      if (!isTokenExpired(accessToken)) {
        apiClient.setToken(accessToken);
        setUser(JSON.parse(userData));
        setIsLoading(false);
        return;
      }

      // Access token expired - try refresh
      if (refreshToken && !isTokenExpired(refreshToken)) {
        try {
          const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";
          const response = await fetch(`${API_URL}/api/v1/auth/refresh`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ refreshToken }),
          });

          if (response.ok) {
            const data = await response.json();
            const newAccessToken = data.data.accessToken;
            const newRefreshToken = data.data.refreshToken;
            const newUser = data.data.user;

            apiClient.setToken(newAccessToken);
            localStorage.setItem("accessToken", newAccessToken);
            localStorage.setItem("refreshToken", newRefreshToken);
            if (newUser) {
              localStorage.setItem("user", JSON.stringify(newUser));
              setUser(newUser);
            } else {
              setUser(JSON.parse(userData));
            }
            setIsLoading(false);
            return;
          }
        } catch {
          // Refresh failed
        }
      }

      // Both tokens invalid - clear and require login
      localStorage.removeItem("accessToken");
      localStorage.removeItem("refreshToken");
      localStorage.removeItem("user");
      setIsLoading(false);
    };

    restoreSession();
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    const response = await authApi.login({ email, password });
    const { accessToken, refreshToken, user: userInfo } = response.data;
    apiClient.setToken(accessToken);
    localStorage.setItem("accessToken", accessToken);
    localStorage.setItem("refreshToken", refreshToken);
    localStorage.setItem("user", JSON.stringify(userInfo));
    setUser(userInfo);
  }, []);

  const register = useCallback(async (email: string, password: string, fullName: string, companyName?: string) => {
    const response = await authApi.register({ email, password, fullName, companyName });
    const { accessToken, refreshToken, user: userInfo } = response.data;
    apiClient.setToken(accessToken);
    localStorage.setItem("accessToken", accessToken);
    localStorage.setItem("refreshToken", refreshToken);
    localStorage.setItem("user", JSON.stringify(userInfo));
    setUser(userInfo);
  }, []);

  const logout = useCallback(() => {
    apiClient.setToken(null);
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
    localStorage.removeItem("user");
    setUser(null);
    window.location.href = "/login";
  }, []);

  return (
    <AuthContext.Provider value={{ user, isLoading, isAuthenticated: !!user, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth must be used within AuthProvider");
  return context;
}
