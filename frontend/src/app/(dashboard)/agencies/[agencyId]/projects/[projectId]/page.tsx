"use client";

import { useParams } from "next/navigation";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { projectsApi } from "@/lib/api/projects.api";
import { agenciesApi } from "@/lib/api/agencies.api";
import { apiClient } from "@/lib/api/client";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { PenLine, Share2, Mail, BarChart3, Loader2, FileText, Rss, Globe, FolderKanban, Calendar, Images, ImageIcon, Sparkles } from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";
import { GenerateContentDialog } from "@/components/agency/generate-content-dialog";
import { ContentEditDialog } from "@/components/agency/content-edit-dialog";
import type { ApiResponse, GeneratedContent } from "@/types/api";

const agents = [
  { type: "content-writer", label: "Content Writer", desc: "Genera articoli blog", icon: PenLine, color: "text-blue-600 bg-blue-100 dark:bg-blue-950" },
  { type: "social-manager", label: "Social Manager", desc: "Crea post social", icon: Share2, color: "text-violet-600 bg-violet-100 dark:bg-violet-950" },
  { type: "newsletter", label: "Newsletter", desc: "Genera newsletter", icon: Mail, color: "text-emerald-600 bg-emerald-100 dark:bg-emerald-950" },
  { type: "analytics", label: "Analytics", desc: "Analizza performance", icon: BarChart3, color: "text-amber-600 bg-amber-100 dark:bg-amber-950" },
];

export default function ProjectDetailPage() {
  const { agencyId, projectId } = useParams();
  const queryClient = useQueryClient();
  const [generateFor, setGenerateFor] = useState<{ type: string; label: string } | null>(null);
  const [editingContent, setEditingContent] = useState<GeneratedContent | null>(null);
  const [brandExtracting, setBrandExtracting] = useState(false);

  const handleExtractBrand = async () => {
    if (!project?.websiteUrl) {
      toast.error("Imposta prima un URL del sito web per il progetto");
      return;
    }
    setBrandExtracting(true);
    try {
      await projectsApi.extractBrand(agencyId as string, projectId as string);
      toast.success("Brand voice estratto dal sito!");
      queryClient.invalidateQueries({ queryKey: ["project", agencyId, projectId] });
    } catch (err: any) {
      toast.error(err?.response?.data?.message ?? "Estrazione brand fallita");
    } finally {
      setBrandExtracting(false);
    }
  };

  const { data: agencyData } = useQuery({
    queryKey: ["agency", agencyId],
    queryFn: () => agenciesApi.get(agencyId as string),
  });

  const { data: project, isLoading } = useQuery({
    queryKey: ["project", agencyId, projectId],
    queryFn: () => projectsApi.get(agencyId as string, projectId as string),
  });

  const { data: contentData, refetch: refetchContent } = useQuery({
    queryKey: ["content", agencyId, "project", projectId],
    queryFn: () =>
      apiClient.get<ApiResponse<GeneratedContent[]>>(
        `/api/v1/agencies/${agencyId}/content?projectId=${projectId}`
      ),
  });

  const agency = agencyData?.data;
  const allContents = [...(contentData?.data || [])].sort(
    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
  );
  const blogs = allContents.filter((c) => c.contentType === 1);
  const socials = allContents.filter((c) => c.contentType === 2);
  const newsletters = allContents.filter((c) => c.contentType === 3);

  const handleOpenGenerate = (agentType: string, label: string) => {
    if (!agency?.defaultLlmProviderKeyId) {
      toast.error("Configura prima una chiave LLM nelle impostazioni dell'agenzia");
      return;
    }
    setGenerateFor({ type: agentType, label });
  };

  const formatDate = (iso: string) =>
    new Date(iso).toLocaleString("it-IT", { day: "2-digit", month: "short", hour: "2-digit", minute: "2-digit" });

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
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="text-base">Brand Voice</CardTitle>
            {project.websiteUrl && (
              <Button
                size="sm"
                variant="outline"
                onClick={handleExtractBrand}
                disabled={brandExtracting}
              >
                {brandExtracting ? <Loader2 className="size-4 animate-spin" /> : <Sparkles className="size-4" />}
                Estrai dal sito
              </Button>
            )}
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
                  onClick={() => handleOpenGenerate(agent.type, agent.label)}
                >
                  <agent.icon className="size-4" /> Avvia
                </Button>
              </CardContent>
            </Card>
          ))}
        </div>
      </div>

      <ContentSection title="Blog" icon={PenLine} items={blogs} formatDate={formatDate} onClick={setEditingContent} />
      <ContentSection title="Social" icon={Share2} items={socials} formatDate={formatDate} onClick={setEditingContent} />
      <ContentSection title="Newsletter" icon={Mail} items={newsletters} formatDate={formatDate} onClick={setEditingContent} />

      {generateFor && (
        <GenerateContentDialog
          open={!!generateFor}
          onOpenChange={(open) => !open && setGenerateFor(null)}
          agencyId={agencyId as string}
          agentType={generateFor.type}
          agentLabel={generateFor.label}
          projectId={projectId as string}
          onStarted={() => refetchContent()}
        />
      )}

      <ContentEditDialog
        content={editingContent}
        agencyId={agencyId as string}
        open={!!editingContent}
        onOpenChange={(open) => { if (!open) setEditingContent(null); }}
      />
    </div>
  );
}

function ContentSection({
  title,
  icon: Icon,
  items,
  formatDate,
  onClick,
}: {
  title: string;
  icon: any;
  items: GeneratedContent[];
  formatDate: (iso: string) => string;
  onClick: (c: GeneratedContent) => void;
}) {
  return (
    <div>
      <h2 className="text-lg font-semibold tracking-tight mb-4 flex items-center gap-2">
        <Icon className="size-5" />
        {title}
        <Badge variant="secondary" className="ml-1">{items.length}</Badge>
      </h2>
      {items.length === 0 ? (
        <Card className="text-center py-8">
          <CardContent>
            <p className="text-muted-foreground text-sm">Nessun contenuto {title.toLowerCase()} generato per questo progetto</p>
          </CardContent>
        </Card>
      ) : (
        <div className="grid md:grid-cols-2 gap-3">
          {items.slice(0, 6).map((c) => (
            <Card key={c.id} className="hover:shadow-md transition-shadow cursor-pointer" onClick={() => onClick(c)}>
              <CardContent className="pt-4">
                <div className="flex items-start gap-3">
                  {c.imageUrl && (
                    <img src={c.imageUrl} alt={c.title} className="w-16 h-16 rounded object-cover shrink-0 ring-1 ring-border" />
                  )}
                  <div className="flex-1 min-w-0">
                    <h4 className="font-medium text-sm truncate">{c.title}</h4>
                    <p className="text-xs text-muted-foreground line-clamp-2 mt-1">{c.body.slice(0, 120)}</p>
                    <div className="flex items-center gap-2 mt-2 text-xs text-muted-foreground">
                      <Calendar className="size-3" />
                      <span>{formatDate(c.createdAt)}</span>
                      {c.imageUrls && c.imageUrls.length > 1 && (
                        <Badge variant="outline" className="ml-auto">
                          <Images className="size-3" /> {c.imageUrls.length}
                        </Badge>
                      )}
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
