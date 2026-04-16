"use client";

import Link from "next/link";
import { Bell } from "lucide-react";
import { useQuery } from "@tanstack/react-query";
import { notificationsApi } from "@/lib/api/notifications.api";

export function NotificationBell() {
  const { data } = useQuery({
    queryKey: ["notifications", "unread-count"],
    queryFn: async () => (await notificationsApi.list(true, 1)).data,
    refetchInterval: 15000,
  });

  const count = data?.unreadCount ?? 0;

  return (
    <Link
      href="/jobs"
      className="relative flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium text-sidebar-foreground/70 hover:text-sidebar-foreground hover:bg-sidebar-accent/50 transition-colors"
      data-testid="notification-bell"
    >
      <Bell className="size-4 shrink-0" />
      <span className="flex-1">Notifiche</span>
      {count > 0 && (
        <span
          className="inline-flex items-center justify-center rounded-full bg-primary text-primary-foreground text-[10px] font-semibold min-w-5 h-5 px-1"
          data-testid="notification-count"
        >
          {count > 99 ? "99+" : count}
        </span>
      )}
    </Link>
  );
}
