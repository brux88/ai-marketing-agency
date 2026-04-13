"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useAuth } from "@/lib/providers/auth-provider";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import {
  LayoutDashboard,
  PlusCircle,
  Key,
  CreditCard,
  Sparkles,
  LogOut,
  ChevronRight,
  Shield,
} from "lucide-react";
import { ThemeToggle } from "@/components/layout/theme-toggle";

const navItems = [
  { href: "/dashboard", label: "Dashboard", icon: LayoutDashboard },
  { href: "/agencies/new", label: "Nuova Agenzia", icon: PlusCircle },
  { href: "/settings/api-keys", label: "Chiavi API", icon: Key },
  { href: "/settings/billing", label: "Abbonamento", icon: CreditCard },
  { href: "/admin", label: "Admin", icon: Shield, ownerOnly: true },
];

export function AppSidebar() {
  const pathname = usePathname();
  const { user, logout } = useAuth();

  return (
    <aside className="w-64 bg-sidebar text-sidebar-foreground flex flex-col min-h-screen border-r">
      <div className="p-4 h-16 flex items-center border-b border-sidebar-border">
        <Link href="/dashboard" className="flex items-center gap-2 text-lg font-bold">
          <Sparkles className="size-5 text-primary" />
          <span>AI Marketing</span>
        </Link>
      </div>

      <nav className="flex-1 p-3 space-y-1">
        {navItems
          .filter((item) => !item.ownerOnly || user?.role === "Owner")
          .map((item) => {
          const isActive = pathname === item.href || pathname.startsWith(item.href + "/");
          return (
            <Link
              key={item.href}
              href={item.href}
              className={`flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors ${
                isActive
                  ? "bg-sidebar-accent text-sidebar-accent-foreground"
                  : "text-sidebar-foreground/70 hover:text-sidebar-foreground hover:bg-sidebar-accent/50"
              }`}
            >
              <item.icon className="size-4 shrink-0" />
              <span className="flex-1">{item.label}</span>
              {isActive && <ChevronRight className="size-3 opacity-50" />}
            </Link>
          );
        })}
      </nav>

      <Separator className="bg-sidebar-border" />

      <div className="p-4 space-y-3">
        <div className="flex items-center justify-between">
          <div className="text-sm min-w-0">
            <p className="font-medium truncate">{user?.fullName}</p>
            <p className="text-sidebar-foreground/60 text-xs truncate">{user?.email}</p>
          </div>
          <ThemeToggle />
        </div>
        <Button
          variant="ghost"
          size="sm"
          onClick={logout}
          className="w-full justify-start text-destructive hover:text-destructive hover:bg-destructive/10"
        >
          <LogOut className="size-4" />
          Esci
        </Button>
      </div>
    </aside>
  );
}
