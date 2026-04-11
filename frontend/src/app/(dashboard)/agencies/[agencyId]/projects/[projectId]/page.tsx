"use client";

import { useParams } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import { projectsApi } from "@/lib/api/projects.api";
import { agenciesApi } from "@/lib/api/agencies.api";
import { apiClient } from "@/lib/api/client";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { PenLine, Share2, Mail, BarChart3, Loader2, FileText, Rss, Globe, FolderKanban } from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";

const agents = [
  { type: "content-writer", label: "Content Writer", desc: "Genera articoli blog", icon: PenLine, color: "text-blue-600 bg-blue-100 dark:bg-blue-950" },
  { type: "social-manager", label: "Social Manager", desc: "Crea post social", icon: Share2, color: "text-violet-600 bg-violet-100 dark:bg-violet-950" },
  { type: "newsletter", label: "Newsletter", desc: "Genera newsletter", icon: Mail, color: "text-emerald-600 bg-emerald-100 dark:bg-emerald-950" },
  { type: "analytics", label: "Analytics", desc: "Analizza performance", icon: BarChart3, color: "text-amber-600 bg-amber-100 dark:bg-amber-950" },
];

export default function ProjectDetailPage() {
  const { agencyId, projectId } = useParams();
  const [runningAgent, setRunningAgent] = useState<string | null>(null);

  const { data: agencyData } = useQuery({
    queryKey: ["agency", agencyId],
    queryFn: () => agenciesApi.get(agencyId as string),
  });

  const { data: project, isLoading } = useQuery({
    queryKey: ["project", agencyId, projectId],
    queryFn: () => projectsApi.get(agencyId as string, projectId as string),
  });

  const agency = agencyData?.data;

  const handleRunAgent = async (agentType: string) => {
    if (!agency?.defaultLlmProviderKeyId) {
      toast.error("Configura prima una chiave LLM nelle impostazioni dell'agenzia");
      return;
    }
    setRunningAgent(agentType);
    try {
      await apiClient.post(`/api/v1/agencies/${agencyId}/agents/${agentType}/run`, {
        projectId,
      });
      toast.success("Agente avviato con successo!");
    } catch (err: any) {
      const message = err?.message || "Errore durante l'avvio dell'agente";
      toast.error(message);
    } finally {
      setRunningAgent(null);
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-48" />
      </div>
    );
  }

  if (!project) {
    return (
      <Card className="text-center py-12">
        <CardContent>
          <p className="text-destructive font-medium">Progetto non trovato</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-8">
      <div>
        <div className="flex items-center gap-3">
          <div className="size-10 rounded-lg bg-primary/10 flex items-center justify-center">
            <FolderKanban className="size-5 text-primary" />
          </div>
          <div>
            <h1 className="text-2xl font-bold tracking-tight">{project.name}</h1>
            {project.description && (
              <p className="text-muted-foreground mt-1">{project.description}</p>
            )}
          </div>
        </div>
      </div>

      <div className="grid md:grid-cols-2 gap-6">
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Statistiche Progetto</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <div className="flex items-center justify-between text-sm">
              <span className="flex items-center gap-2 text-muted-foreground">
                <Rss className="size-4" /> Fonti contenuto
              </span>
              <span className="font-medium">{project.contentSourcesCount}</span>
            </div>
            <div className="flex items-center justify-between text-sm">
              <span className="flex items-center gap-2 text-muted-foreground">
                <FileText className="size-4" /> Contenuti generati
              </span>
              <span className="font-medium">{project.generatedContentsCount}</span>
            </div>
            {project.websiteUrl && (
              <div className="flex items-center justify-between text-sm">
                <span className="flex items-center gap-2 text-muted-foreground">
                  <Globe className="size-4" /> Sito web
                </span>
                <a href={project.websiteUrl} target="_blank" rel="noopener noreferrer" className="text-primary hover:underline text-xs truncate max-w-48">
                  {project.websiteUrl}
                </a>
              </div>
            )}
            <div className="flex items-center justify-between text-sm">
              <span className="text-muted-foreground">Stato</span>
              <Badge variant={project.isActive ? "default" : "secondary"}>
                {project.isActive ? "Attivo" : "Inattivo"}
              </Badge>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">Brand Voice</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <div className="flex items-center justify-between text-sm">
              <span className="text-muted-foreground">Tono</span>
              <span className="font-medium capitalize">{project.brandVoice.tone}</span>
            </div>
            {project.brandVoice.style && (
              <div className="flex items-center justify-between text-sm">
                <span className="text-muted-foreground">Stile</span>
                <span className="font-medium">{project.brandVoice.style}</span>
              </div>
            )}
            {project.brandVoice.keywords.length > 0 && (
              <div className="flex flex-wrap gap-1.5 pt-1">
                {project.brandVoice.keywords.map((kw) => (
                  <Badge key={kw} variant="secondary" className="text-xs">{kw}</Badge>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      <div>
        <h2 className="text-lg font-semibold tracking-tight mb-4">Lancia un agente AI per questo progetto</h2>
        <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-4">
          {agents.map((agent) => (
            <Card key={agent.type} className="h-full">
              <CardContent className="pt-6">
                <div className={`size-10 rounded-lg flex items-center justify-center mb-3 ${agent.color}`}>
                  <agent.icon className="size-5" />
                </div>
                <h4 className="font-semibold">{agent.label}</h4>
                <p className="text-muted-foreground text-sm mt-1">{agent.desc}</p>
                <Button
                  className="mt-4 w-full"
                  size="sm"
                  onClick={() => handleRunAgent(agent.type)}
                  disabled={runningAgent === agent.type}
                >
                  {runningAgent === agent.type ? (
                    <><Loader2 className="size-4 animate-spin" /> Avvio...</>
                  ) : (
                    <><agent.icon className="size-4" /> Avvia</>
                  )}
                </Button>
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    </div>
  );
}
