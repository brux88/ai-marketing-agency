"use client";

import { useParams } from "next/navigation";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { agenciesApi } from "@/lib/api/agencies.api";
import { projectsApi } from "@/lib/api/projects.api";
import { apiClient } from "@/lib/api/client";
import type { ApiResponse, GeneratedContent } from "@/types/api";
import { useMemo, useState } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { BarChart3, Loader2, FileText, Star, Briefcase, TrendingUp, ImageIcon, CheckCircle2, DollarSign, Trash2 } from "lucide-react";
import { toast } from "sonner";

const contentTypeLabels: Record<number, string> = {
  1: "Blog",
  2: "Social",
  3: "Newsletter",
  4: "Report",
};

export default function AnalyticsPage() {
  const { agencyId } = useParams();
  const [generating, setGenerating] = useState(false);
  const [resetPassword, setResetPassword] = useState("");
  const [showResetDialog, setShowResetDialog] = useState(false);
  const queryClient = useQueryClient();

  const resetMutation = useMutation({
    mutationFn: () => agenciesApi.resetAnalytics(agencyId as string, resetPassword),
    onSuccess: (data) => {
      toast.success(`Reset completato: ${data?.reset ?? 0} contenuti eliminati`);
      setShowResetDialog(false);
      setResetPassword("");
      queryClient.invalidateQueries({ queryKey: ["content"] });
      queryClient.invalidateQueries({ queryKey: ["agency-cost-stats"] });
      queryClient.invalidateQueries({ queryKey: ["notifications"] });
      queryClient.invalidateQueries({ queryKey: ["jobs"] });
    },
    onError: (err: any) => {
      toast.error(err?.response?.data?.error || err?.message || "Password non valida");
    },
  });

  const { data: agencyData } = useQuery({
    queryKey: ["agency", agencyId],
    queryFn: () => agenciesApi.get(agencyId as string),
  });

  const { data: contentData, isLoading: contentLoading } = useQuery({
    queryKey: ["content", agencyId],
    queryFn: () =>
      apiClient.get<ApiResponse<GeneratedContent[]>>(`/api/v1/agencies/${agencyId}/content`),
  });

  const { data: costStats } = useQuery({
    queryKey: ["agency-cost-stats", agencyId],
    queryFn: () => projectsApi.getAgencyCostStats(agencyId as string),
    refetchInterval: 60000,
  });

  const agency = agencyData?.data;
  const allContents = contentData?.data || [];

  const stats = useMemo(() => {
    const total = allContents.length;
    const avgScore = total > 0
      ? (allContents.reduce((sum, c) => sum + c.overallScore, 0) / total).toFixed(1)
      : "0";
    const approved = allContents.filter((c) => c.status === 3).length;
    const withImages = allContents.filter((c) => c.imageUrl).length;
    const autoApproved = allContents.filter((c) => c.autoApproved).length;

    const byType: Record<number, number> = {};
    allContents.forEach((c) => {
      byType[c.contentType] = (byType[c.contentType] || 0) + 1;
    });

    return { total, avgScore, approved, withImages, autoApproved, byType };
  }, [allContents]);

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

  const metrics = [
    { label: "Contenuti generati", value: String(stats.total), trend: `${stats.approved} approvati`, icon: FileText, color: "text-blue-600" },
    { label: "Score medio", value: `${stats.avgScore}/10`, trend: `${stats.autoApproved} auto-approvati`, icon: Star, color: "text-amber-600" },
    { label: "Con immagini", value: String(stats.withImages), trend: `su ${stats.total} totali`, icon: ImageIcon, color: "text-emerald-600" },
  ];

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
              <p className="text-2xl font-bold">{contentLoading ? "..." : m.value}</p>
              <p className="text-xs text-muted-foreground mt-1">{contentLoading ? "" : m.trend}</p>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Costi AI */}
      <Card>
        <CardHeader>
          <CardTitle className="text-base flex items-center gap-2">
            <DollarSign className="size-4" /> Costi AI agenzia
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          {!costStats ? (
            <p className="text-sm text-muted-foreground">Caricamento...</p>
          ) : (
            <>
              <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                <div>
                  <p className="text-[10px] uppercase text-muted-foreground">Totale contenuti</p>
                  <p className="text-2xl font-bold">{costStats.totalContents}</p>
                </div>
                <div>
                  <p className="text-[10px] uppercase text-muted-foreground">Costo totale</p>
                  <p className="text-2xl font-bold">${costStats.totalCostUsd.toFixed(4)}</p>
                </div>
                <div>
                  <p className="text-[10px] uppercase text-muted-foreground">Testo / Immagini</p>
                  <p className="text-sm mt-2">
                    ${costStats.totalTextCostUsd.toFixed(4)} / ${costStats.totalImageCostUsd.toFixed(4)}
                  </p>
                </div>
                <div>
                  <p className="text-[10px] uppercase text-muted-foreground">Ultimi 30 giorni</p>
                  <p className="text-2xl font-bold">${costStats.last30DaysCostUsd.toFixed(4)}</p>
                </div>
              </div>
              {costStats.projects.length > 0 && (
                <div className="space-y-1.5 pt-2 border-t">
                  <p className="text-xs font-semibold text-muted-foreground">Per progetto</p>
                  {costStats.projects.map((p) => (
                    <div key={p.projectId} className="flex items-center justify-between text-sm py-1">
                      <span className="truncate flex-1 min-w-0">{p.projectName}</span>
                      <span className="text-muted-foreground text-xs shrink-0 mx-2">
                        {p.contents} contenuti
                      </span>
                      <span className="font-semibold shrink-0">${p.totalCostUsd.toFixed(4)}</span>
                    </div>
                  ))}
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>

      {/* Breakdown per tipo di contenuto */}
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Contenuti per tipo</CardTitle>
        </CardHeader>
        <CardContent>
          {contentLoading ? (
            <p className="text-muted-foreground text-sm">Caricamento...</p>
          ) : Object.keys(stats.byType).length === 0 ? (
            <p className="text-muted-foreground text-sm">Nessun contenuto generato</p>
          ) : (
            <div className="grid sm:grid-cols-2 gap-3">
              {Object.entries(stats.byType).map(([type, count]) => (
                <div key={type} className="flex items-center justify-between p-3 rounded-lg bg-muted/50">
                  <div className="flex items-center gap-2">
                    <Badge variant="outline">{contentTypeLabels[Number(type)] || "Altro"}</Badge>
                  </div>
                  <span className="text-lg font-semibold">{count}</span>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Top contenuti per score */}
      {allContents.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Top contenuti per score</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {[...allContents]
              .sort((a, b) => b.overallScore - a.overallScore)
              .slice(0, 5)
              .map((c) => (
                <div key={c.id} className="flex items-center justify-between gap-4 py-2 border-b last:border-0">
                  <div className="flex items-center gap-3 min-w-0 flex-1">
                    <Badge variant="outline" className="shrink-0">
                      {contentTypeLabels[c.contentType] || "Altro"}
                    </Badge>
                    <span className="text-sm truncate">{c.title}</span>
                  </div>
                  <div className="flex items-center gap-2 shrink-0">
                    {c.imageUrl && <ImageIcon className="size-3.5 text-muted-foreground" />}
                    {c.autoApproved && <CheckCircle2 className="size-3.5 text-green-500" />}
                    <div className="flex items-center gap-1">
                      <Star className="size-3.5 text-amber-500" />
                      <span className="text-sm font-semibold">{c.overallScore}/10</span>
                    </div>
                  </div>
                </div>
              ))}
          </CardContent>
        </Card>
      )}

      <Card className="text-center py-12">
        <CardContent className="space-y-4">
          <div className="size-16 rounded-full bg-amber-100 dark:bg-amber-950 flex items-center justify-center mx-auto">
            <TrendingUp className="size-8 text-amber-600" />
          </div>
          <h3 className="text-lg font-semibold">AI Analytics Agent</h3>
          <p className="text-muted-foreground max-w-md mx-auto">
            Clicca &quot;Genera report&quot; per far analizzare i trend e suggerire miglioramenti
            dall&apos;agente AI.
          </p>
        </CardContent>
      </Card>

      {/* Reset analytics */}
      <Card className="border-destructive/50">
        <CardHeader>
          <CardTitle className="text-base flex items-center gap-2 text-destructive">
            <Trash2 className="size-4" /> Reset Analytics Agenzia
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          <p className="text-sm text-muted-foreground">
            Elimina tutti i contenuti, calendar entries, notifiche e jobs dell&apos;agenzia. Richiede la password admin.
          </p>
          {!showResetDialog ? (
            <Button variant="destructive" size="sm" onClick={() => setShowResetDialog(true)}>
              <Trash2 className="size-4 mr-1" /> Reset completo
            </Button>
          ) : (
            <div className="flex items-center gap-2">
              <input
                type="password"
                placeholder="Password admin"
                className="h-9 rounded-md border border-input bg-background px-3 py-1 text-sm shadow-sm"
                value={resetPassword}
                onChange={(e) => setResetPassword(e.target.value)}
              />
              <Button
                variant="destructive"
                size="sm"
                disabled={!resetPassword || resetMutation.isPending}
                onClick={() => resetMutation.mutate()}
              >
                {resetMutation.isPending ? <Loader2 className="size-4 animate-spin" /> : "Conferma"}
              </Button>
              <Button variant="outline" size="sm" onClick={() => { setShowResetDialog(false); setResetPassword(""); }}>
                Annulla
              </Button>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
