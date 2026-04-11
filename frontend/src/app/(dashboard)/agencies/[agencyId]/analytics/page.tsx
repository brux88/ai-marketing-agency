"use client";

import { useParams } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import { agenciesApi } from "@/lib/api/agencies.api";
import { apiClient } from "@/lib/api/client";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { BarChart3, Loader2, FileText, Star, Briefcase, TrendingUp } from "lucide-react";
import { toast } from "sonner";

const metrics = [
  { label: "Contenuti generati", value: "0", trend: "+0%", icon: FileText, color: "text-blue-600" },
  { label: "Score medio", value: "0/10", trend: "N/A", icon: Star, color: "text-amber-600" },
  { label: "Job questo mese", value: "0", trend: "0/20 limite", icon: Briefcase, color: "text-emerald-600" },
];

export default function AnalyticsPage() {
  const { agencyId } = useParams();
  const [generating, setGenerating] = useState(false);

  const { data: agencyData } = useQuery({
    queryKey: ["agency", agencyId],
    queryFn: () => agenciesApi.get(agencyId as string),
  });

  const agency = agencyData?.data;

  const handleGenerate = async () => {
    if (!agency?.defaultLlmProviderKeyId) {
      toast.error("Configura prima una chiave LLM nelle impostazioni dell'agenzia");
      return;
    }
    setGenerating(true);
    try {
      await apiClient.post(`/api/v1/agencies/${agencyId}/agents/analytics/run`, {});
      toast.success("Generazione report avviata!");
    } catch (err: any) {
      const message = err?.message || "Errore durante la generazione";
      toast.error(message);
    } finally {
      setGenerating(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold tracking-tight">Analytics & Reporting</h2>
          <p className="text-muted-foreground text-sm mt-1">
            Monitora le performance dei tuoi contenuti
          </p>
        </div>
        <Button onClick={handleGenerate} disabled={generating}>
          {generating ? (
            <><Loader2 className="size-4 animate-spin" /> Generazione...</>
          ) : (
            <><BarChart3 className="size-4" /> Genera report</>
          )}
        </Button>
      </div>

      <div className="grid sm:grid-cols-3 gap-4">
        {metrics.map((m) => (
          <Card key={m.label}>
            <CardHeader className="pb-2">
              <div className="flex items-center justify-between">
                <CardTitle className="text-sm font-medium text-muted-foreground">{m.label}</CardTitle>
                <m.icon className={`size-4 ${m.color}`} />
              </div>
            </CardHeader>
            <CardContent>
              <p className="text-2xl font-bold">{m.value}</p>
              <p className="text-xs text-muted-foreground mt-1">{m.trend}</p>
            </CardContent>
          </Card>
        ))}
      </div>

      <Card className="text-center py-12">
        <CardContent className="space-y-4">
          <div className="size-16 rounded-full bg-amber-100 dark:bg-amber-950 flex items-center justify-center mx-auto">
            <TrendingUp className="size-8 text-amber-600" />
          </div>
          <h3 className="text-lg font-semibold">Dashboard Analytics</h3>
          <p className="text-muted-foreground max-w-md mx-auto">
            Qui vedrai grafici e insight AI sulle performance dei tuoi contenuti.
            L&apos;agente analizza trend e suggerisce miglioramenti.
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
