"use client";

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Users, Building2, CreditCard, Activity, TrendingUp,
  Shield, Server, AlertTriangle,
} from "lucide-react";

const stats = [
  { label: "Tenant totali", value: "0", icon: Building2, color: "text-blue-600" },
  { label: "Utenti attivi", value: "0", icon: Users, color: "text-emerald-600" },
  { label: "Abbonamenti attivi", value: "0", icon: CreditCard, color: "text-violet-600" },
  { label: "Job AI oggi", value: "0", icon: Activity, color: "text-amber-600" },
];

const systemHealth = [
  { name: "API Server", status: "online" },
  { name: "Database", status: "online" },
  { name: "Job Queue", status: "online" },
  { name: "LLM Providers", status: "online" },
];

export default function AdminPage() {
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
        <Badge variant="secondary">SaaS Owner</Badge>
      </div>

      {/* Stats */}
      <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {stats.map((s) => (
          <Card key={s.label}>
            <CardHeader className="pb-2">
              <div className="flex items-center justify-between">
                <CardTitle className="text-sm font-medium text-muted-foreground">{s.label}</CardTitle>
                <s.icon className={`size-4 ${s.color}`} />
              </div>
            </CardHeader>
            <CardContent>
              <p className="text-3xl font-bold">{s.value}</p>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid md:grid-cols-2 gap-6">
        {/* System Health */}
        <Card>
          <CardHeader>
            <CardTitle className="text-base flex items-center gap-2">
              <Server className="size-4" />
              Stato Sistema
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {systemHealth.map((service) => (
              <div key={service.name} className="flex items-center justify-between text-sm">
                <span>{service.name}</span>
                <Badge variant="secondary" className="text-xs">
                  <span className="size-1.5 rounded-full bg-emerald-500 mr-1.5" />
                  {service.status}
                </Badge>
              </div>
            ))}
          </CardContent>
        </Card>

        {/* Revenue */}
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
              <p className="text-4xl font-bold">&euro;0</p>
              <p className="text-muted-foreground text-sm mt-1">MRR</p>
            </div>
            <div className="grid grid-cols-3 gap-4 text-center text-sm">
              <div>
                <p className="font-medium">0</p>
                <p className="text-muted-foreground text-xs">Basic</p>
              </div>
              <div>
                <p className="font-medium">0</p>
                <p className="text-muted-foreground text-xs">Pro</p>
              </div>
              <div>
                <p className="font-medium">0</p>
                <p className="text-muted-foreground text-xs">Enterprise</p>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Recent Activity */}
        <Card className="md:col-span-2">
          <CardHeader>
            <CardTitle className="text-base flex items-center gap-2">
              <Activity className="size-4" />
              Attivita Recente
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-center py-8">
              <AlertTriangle className="size-8 text-muted-foreground/50 mx-auto mb-3" />
              <p className="text-muted-foreground">Nessuna attivita recente</p>
              <p className="text-xs text-muted-foreground/60 mt-1">
                Le registrazioni e gli eventi appariranno qui
              </p>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
