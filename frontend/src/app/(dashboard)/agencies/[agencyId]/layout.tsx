"use client";

import Link from "next/link";
import { usePathname, useParams } from "next/navigation";
import { useEffect } from "react";
import { Settings, LayoutDashboard, FolderKanban, Users, Send, BarChart3, Mail, Share2 } from "lucide-react";
import { useNotifications } from "@/lib/providers/notification-provider";

const tabs = [
  { href: "", label: "Overview", icon: LayoutDashboard },
  { href: "/projects", label: "Progetti", icon: FolderKanban },
  { href: "/analytics", label: "Analytics", icon: BarChart3 },
  { href: "/social", label: "Social", icon: Share2 },
  { href: "/telegram", label: "Telegram", icon: Send },
  { href: "/newsletter", label: "Newsletter", icon: Mail },
  { href: "/team", label: "Team", icon: Users },
  { href: "/settings", label: "Impostazioni", icon: Settings },
];

export default function AgencyLayout({ children }: { children: React.ReactNode }) {
  const { agencyId } = useParams();
  const pathname = usePathname();
  const basePath = `/agencies/${agencyId}`;
  const { joinAgency, leaveAgency } = useNotifications();

  useEffect(() => {
    if (!agencyId) return;
    const id = agencyId as string;
    joinAgency(id);
    return () => { leaveAgency(id); };
  }, [agencyId, joinAgency, leaveAgency]);

  return (
    <div>
      <nav className="flex gap-1 border-b mb-6 -mt-2 overflow-x-auto">
        {tabs.map((tab) => {
          const fullPath = `${basePath}${tab.href}`;
          const isActive = tab.href === ""
            ? pathname === basePath
            : pathname.startsWith(fullPath);
          return (
            <Link
              key={tab.href}
              href={fullPath}
              className={`flex items-center gap-1.5 px-4 py-3 text-sm font-medium border-b-2 transition-colors whitespace-nowrap ${
                isActive
                  ? "border-primary text-primary"
                  : "border-transparent text-muted-foreground hover:text-foreground hover:border-muted-foreground/30"
              }`}
            >
              <tab.icon className="size-4" />
              {tab.label}
            </Link>
          );
        })}
      </nav>
      {children}
    </div>
  );
}
