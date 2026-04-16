"use client";

import { useQuery } from "@tanstack/react-query";
import { adminApi, type AdminStats, type TenantDetail } from "@/lib/api/admin.api";
import { useAuth } from "@/lib/providers/auth-provider";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Users, Building2, CreditCard, Activity, TrendingUp,
  Shield, Server, Briefcase, FileText, DollarSign, Loader2,
} from "lucide-react";

const planPrices: Record<string, number> = {
  Basic: 29,
  Pro: 79,
  Enterprise: 199,
};

export default function AdminPage() {
  const { user } = useAuth();

  const { data: statsData, isLoading: statsLoading } = useQuery({
    queryKey: ["admin", "stats"],
    queryFn: () => adminApi.getStats(),
    refetchInterval: 30000,
    enabled: user?.role === "SuperAdmin" || user?.role === "Owner",
  });

  const { data: tenantsData, isLoading: tenantsLoading } = useQuery({
    queryKey: ["admin", "tenants"],
    queryFn: () => adminApi.getTenants(),
    refetchInterval: 60000,
    enabled: user?.role === "SuperAdmin" || user?.role === "Owner",
  });

  const s = statsData;
  const tenants = tenantsData ?? [];

  const mrr = s
    ? Object.entries(s.planBreakdown).reduce(
        (acc, [plan, count]) => acc + (planPrices[plan] ?? 0) * count,
        0
      )
    : 0;

  if (user?.role !== "SuperAdmin" && user?.role !== "Owner") {
    return (
      <div className="flex items-center justify-center min-h-[50vh]">
        <p className="text-muted-foreground">Accesso non autorizzato</p>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight flex items-center gap-2">
            <Shield className="size-6 text-primary" />
            Pannello Admin
          </h1>
          <p className="text-muted-foreground mt-1">Gestione piattaforma SaaS</p>
        </div>
        <Badge variant="secondary">{user?.role === "SuperAdmin" ? "Super Admin" : "SaaS Owner"}</Badge>
      </div>

      <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {[
          { label: "Tenant totali", value: s?.totalTenants, icon: Building2, color: "text-blue-600" },
          { label: "Utenti totali", value: s?.totalUsers, icon: Users, color: "text-emerald-600" },
          { label: "Abbonamenti attivi", value: s?.activeSubscriptions, icon: CreditCard, color: "text-violet-600" },
          { label: "Job AI questo mese", value: s?.jobsThisMonth, icon: Activity, color: "text-amber-600" },
        ].map((stat) => (
          <Card key={stat.label}>
            <CardHeader className="pb-2">
              <div className="flex items-center justify-between">
                <CardTitle className="text-sm font-medium text-muted-foreground">{stat.label}</CardTitle>
                <stat.icon className={`size-4 ${stat.color}`} />
              </div>
            </CardHeader>
            <CardContent>
              {statsLoading ? (
                <Skeleton className="h-9 w-16" />
              ) : (
                <p className="text-3xl font-bold">{stat.value ?? 0}</p>
              )}
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {[
          { label: "Agenzie", value: s?.totalAgencies, icon: Briefcase, color: "text-sky-600" },
          { label: "Progetti", value: s?.totalProjects, icon: FileText, color: "text-pink-600" },
          { label: "Contenuti totali", value: s?.contentsTotal, icon: FileText, color: "text-indigo-600" },
          { label: "Trial attivi", value: s?.trialSubscriptions, icon: CreditCard, color: "text-orange-600" },
        ].map((stat) => (
          <Card key={stat.label}>
            <CardHeader className="pb-2">
              <div className="flex items-center justify-between">
                <CardTitle className="text-sm font-medium text-muted-foreground">{stat.label}</CardTitle>
                <stat.icon className={`size-4 ${stat.color}`} />
              </div>
            </CardHeader>
            <CardContent>
              {statsLoading ? (
                <Skeleton className="h-9 w-16" />
              ) : (
                <p className="text-3xl font-bold">{stat.value ?? 0}</p>
              )}
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid md:grid-cols-2 gap-6">
        <Card>
          <CardHeader>
            <CardTitle className="text-base flex items-center gap-2">
              <TrendingUp className="size-4" />
              Revenue
            </CardTitle>
            <CardDescription>Entrate mensili ricorrenti</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="text-center py-4">
              <p className="text-4xl font-bold">&euro;{mrr}</p>
              <p className="text-muted-foreground text-sm mt-1">MRR</p>
            </div>
            <div className="grid grid-cols-3 gap-4 text-center text-sm">
              <div>
                <p className="font-medium">{s?.planBreakdown?.Basic ?? 0}</p>
                <p className="text-muted-foreground text-xs">Basic</p>
              </div>
              <div>
                <p className="font-medium">{s?.planBreakdown?.Pro ?? 0}</p>
                <p className="text-muted-foreground text-xs">Pro</p>
              </div>
              <div>
                <p className="font-medium">{s?.planBreakdown?.Enterprise ?? 0}</p>
                <p className="text-muted-foreground text-xs">Enterprise</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base flex items-center gap-2">
              <DollarSign className="size-4" />
              Costi AI
            </CardTitle>
            <CardDescription>Spese di generazione contenuti e immagini</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="text-center py-4">
              <p className="text-4xl font-bold">${(s?.totalCostUsd ?? 0).toFixed(2)}</p>
              <p className="text-muted-foreground text-sm mt-1">Costo totale</p>
            </div>
            <div className="grid grid-cols-2 gap-4 text-center text-sm">
              <div>
                <p className="font-medium">${(s?.totalTextCostUsd ?? 0).toFixed(2)}</p>
                <p className="text-muted-foreground text-xs">Testo (LLM)</p>
              </div>
              <div>
                <p className="font-medium">${(s?.totalImageCostUsd ?? 0).toFixed(2)}</p>
                <p className="text-muted-foreground text-xs">Immagini</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="text-base flex items-center gap-2">
            <Building2 className="size-4" />
            Clienti ({tenants.length})
          </CardTitle>
          <CardDescription>Tutti i tenant della piattaforma</CardDescription>
        </CardHeader>
        <CardContent>
          {tenantsLoading ? (
            <div className="flex justify-center py-8">
              <Loader2 className="size-6 animate-spin" />
            </div>
          ) : tenants.length === 0 ? (
            <p className="text-center text-muted-foreground py-8">Nessun tenant registrato</p>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b text-left">
                    <th className="pb-2 font-medium text-muted-foreground">Tenant</th>
                    <th className="pb-2 font-medium text-muted-foreground">Piano</th>
                    <th className="pb-2 font-medium text-muted-foreground">Stato</th>
                    <th className="pb-2 font-medium text-muted-foreground text-right">Utenti</th>
                    <th className="pb-2 font-medium text-muted-foreground text-right">Agenzie</th>
                    <th className="pb-2 font-medium text-muted-foreground text-right">Job/mese</th>
                    <th className="pb-2 font-medium text-muted-foreground text-right">Contenuti</th>
                    <th className="pb-2 font-medium text-muted-foreground text-right">Costi</th>
                    <th className="pb-2 font-medium text-muted-foreground text-right">Registrato</th>
                  </tr>
                </thead>
                <tbody>
                  {tenants.map((t) => (
                    <tr key={t.id} className="border-b last:border-0">
                      <td className="py-2.5 font-medium">{t.name}</td>
                      <td className="py-2.5">
                        <Badge variant="outline" className="text-xs">{t.plan}</Badge>
                      </td>
                      <td className="py-2.5">
                        <Badge
                          variant={t.status === "Active" ? "default" : t.status === "Trialing" ? "secondary" : "destructive"}
                          className="text-xs"
                        >
                          {t.status}
                        </Badge>
                      </td>
                      <td className="py-2.5 text-right">{t.usersCount}</td>
                      <td className="py-2.5 text-right">{t.agenciesCount}</td>
                      <td className="py-2.5 text-right">{t.jobsThisMonth}</td>
                      <td className="py-2.5 text-right">{t.totalContents}</td>
                      <td className="py-2.5 text-right">${t.totalCostUsd.toFixed(2)}</td>
                      <td className="py-2.5 text-right text-muted-foreground">
                        {new Date(t.createdAt).toLocaleDateString("it-IT")}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
