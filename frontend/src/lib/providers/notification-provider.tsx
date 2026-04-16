"use client";

import { createContext, useContext, useEffect, useCallback } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useAuth } from "@/lib/providers/auth-provider";
import { notificationClient, type NotificationEvent } from "@/lib/signalr/notification-client";
import { toast } from "sonner";

interface NotificationContextType {
  joinAgency: (agencyId: string) => Promise<void>;
  leaveAgency: (agencyId: string) => Promise<void>;
}

const NotificationContext = createContext<NotificationContextType>({
  joinAgency: async () => {},
  leaveAgency: async () => {},
});

export function NotificationProvider({ children }: { children: React.ReactNode }) {
  const { user } = useAuth();
  const queryClient = useQueryClient();

  const handleNotification = useCallback(
    (event: NotificationEvent) => {
      switch (event.type) {
        case "JobStatusChanged":
          if (event.data.status === "Completed") {
            toast.success(`Job ${event.data.agentType ?? ""} completato!`);
          } else if (event.data.status === "Failed") {
            toast.error(`Job ${event.data.agentType ?? ""} fallito`);
          }
          queryClient.invalidateQueries({ queryKey: ["content", event.data.agencyId] });
          queryClient.invalidateQueries({ queryKey: ["jobs"] });
          queryClient.invalidateQueries({ queryKey: ["notifications"] });
          break;
        case "ContentGenerated":
          toast.success(`Nuovo contenuto: ${event.data.title}`);
          queryClient.invalidateQueries({ queryKey: ["content", event.data.agencyId] });
          queryClient.invalidateQueries({ queryKey: ["approvals", event.data.agencyId] });
          queryClient.invalidateQueries({ queryKey: ["notifications"] });
          break;
        case "ContentApproved":
          toast.success(`Contenuto approvato: ${event.data.title}`);
          queryClient.invalidateQueries({ queryKey: ["content", event.data.agencyId] });
          queryClient.invalidateQueries({ queryKey: ["approvals", event.data.agencyId] });
          break;
        case "ContentRejected":
          toast.info(`Contenuto rifiutato: ${event.data.title}`);
          queryClient.invalidateQueries({ queryKey: ["content", event.data.agencyId] });
          queryClient.invalidateQueries({ queryKey: ["approvals", event.data.agencyId] });
          break;
        case "PublishResult":
          if (event.data.success) {
            toast.success(`Pubblicato su ${event.data.platform}!`);
          } else {
            toast.error(`Pubblicazione su ${event.data.platform} fallita`);
          }
          queryClient.invalidateQueries({ queryKey: ["project-calendar"] });
          queryClient.invalidateQueries({ queryKey: ["notifications"] });
          break;
      }
    },
    [queryClient]
  );

  useEffect(() => {
    const token = typeof window !== "undefined" ? localStorage.getItem("accessToken") : null;
    if (!user || !token) return;

    notificationClient.connect(token);
    const unsub = notificationClient.onNotification(handleNotification);

    return () => {
      unsub();
      notificationClient.disconnect();
    };
  }, [user, handleNotification]);

  const joinAgency = async (agencyId: string) => {
    await notificationClient.joinAgency(agencyId);
  };

  const leaveAgency = async (agencyId: string) => {
    await notificationClient.leaveAgency(agencyId);
  };

  return (
    <NotificationContext value={{ joinAgency, leaveAgency }}>
      {children}
    </NotificationContext>
  );
}

export const useNotifications = () => useContext(NotificationContext);
