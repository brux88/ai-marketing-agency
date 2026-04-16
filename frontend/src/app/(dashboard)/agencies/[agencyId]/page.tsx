"use client";

import { useParams } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import { agenciesApi } from "@/lib/api/agencies.api";
import { projectsApi } from "@/lib/api/projects.api";
import Link from "next/link";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { Button } from "@/components/ui/button";
import { Rss, FileText, Globe, FolderKanban, Plus, ArrowRight } from "lucide-react";
import type { Project } from "@/types/api";

export default function AgencyOverviewPage() {
  const { agencyId } = useParams();
  const { data, isLoading } = useQuery({
    queryKey: ["agency", agencyId],
    queryFn: () => agenciesApi.get(agencyId as string),
  });

  const { data: projects, isLoading: projectsLoading } = useQuery({
    queryKey: ["projects", agencyId],
    queryFn: () => projectsApi.list(agencyId as string),
    enabled: !!agencyId,
  });

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-64" />
        <div className="grid md:grid-cols-2 gap-6">
          <Skeleton className="h-48" />
          <Skeleton className="h-48" />
        </div>
      </div>
    );
  }

  const agency = data?.data;
  if (!agency) {
    return (
      <Card className="text-center py-12">
        <CardContent>
          <p className="text-destructive font-medium">Agenzia non trovata</p>
        </CardContent>
      </Card>
    );
  }

  const projectList: Project[] = projects || [];

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">{agency.name}</h1>
        <p className="text-muted-foreground mt-1">{agency.productName}</p>
        {agency.description && <p className="text-sm text-muted-foreground mt-2">{agency.description}</p>}
      </div>

      <div className="grid md:grid-cols-3 gap-4">
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-2 text-muted-foreground text-xs mb-1">
              <FolderKanban className="size-3.5" /> Progetti
            </div>
            <div className="text-2xl font-semibold">{projectList.length}</div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-2 text-muted-foreground text-xs mb-1">
              <Rss className="size-3.5" /> Fonti totali
            </div>
            <div className="text-2xl font-semibold">{agency.contentSourcesCount}</div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-2 text-muted-foreground text-xs mb-1">
              <FileText className="size-3.5" /> Contenuti generati
            </div>
            <div className="text-2xl font-semibold">{agency.generatedContentsCount}</div>
          </CardContent>
        </Card>
      </div>

      <div className="grid md:grid-cols-2 gap-6">
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Impostazioni globali</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <div className="flex items-center justify-between text-sm">
              <span className="text-muted-foreground">Approvazione default</span>
              <Badge variant="outline">
                {agency.approvalMode === 1 ? "Manuale" :
                 agency.approvalMode === 2 ? "Automatica" :
                 `Auto sopra ${agency.autoApproveMinScore}/10`}
              </Badge>
            </div>
            <div className="flex items-center justify-between text-sm">
              <span className="text-muted-foreground">Brand Voice</span>
              <span className="font-medium capitalize">{agency.brandVoice.tone} · {agency.brandVoice.language.toUpperCase()}</span>
            </div>
            {agency.websiteUrl && (
              <div className="flex items-center justify-between text-sm">
                <span className="flex items-center gap-2 text-muted-foreground">
                  <Globe className="size-4" /> Sito web
                </span>
                <a href={agency.websiteUrl} target="_blank" rel="noopener noreferrer" className="text-primary hover:underline text-xs truncate max-w-[200px]">
                  {agency.websiteUrl}
                </a>
              </div>
            )}
            {agency.brandVoice.keywords.length > 0 && (
              <div className="flex flex-wrap gap-1.5 pt-1">
                {agency.brandVoice.keywords.slice(0, 8).map((kw) => (
                  <Badge key={kw} variant="secondary" className="text-xs">{kw}</Badge>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      <div>
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold tracking-tight">Progetti</h2>
          <Link href={`/agencies/${agencyId}/projects/new`}>
            <Button size="sm"><Plus className="size-4" /> Nuovo progetto</Button>
          </Link>
        </div>

        {projectsLoading ? (
          <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
            <Skeleton className="h-32" />
            <Skeleton className="h-32" />
          </div>
        ) : projectList.length === 0 ? (
          <Card className="text-center py-12">
            <CardContent className="space-y-3">
              <FolderKanban className="size-10 text-muted-foreground mx-auto" />
              <p className="text-muted-foreground">Nessun progetto ancora.</p>
              <p className="text-sm text-muted-foreground">Crea il tuo primo progetto per iniziare a generare contenuti.</p>
              <Link href={`/agencies/${agencyId}/projects/new`}>
                <Button><Plus className="size-4" /> Crea progetto</Button>
              </Link>
            </CardContent>
          </Card>
        ) : (
          <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {projectList.map((p) => (
              <Link key={p.id} href={`/agencies/${agencyId}/projects/${p.id}`}>
                <Card className="hover:shadow-md transition-all duration-200 hover:-translate-y-0.5 h-full group">
                  <CardContent className="pt-6">
                    <div className="flex items-center gap-3 mb-3">
                      {p.logoUrl ? (
                        <img src={p.logoUrl} alt={p.name} className="size-10 rounded-lg object-cover ring-1 ring-border" />
                      ) : (
                        <div className="size-10 rounded-lg bg-primary/10 flex items-center justify-center">
                          <FolderKanban className="size-5 text-primary" />
                        </div>
                      )}
                      <div className="flex-1 min-w-0">
                        <h4 className="font-semibold truncate">{p.name}</h4>
                        {p.description && <p className="text-muted-foreground text-xs line-clamp-1">{p.description}</p>}
                      </div>
                    </div>
                    <div className="flex items-center gap-3 text-xs text-muted-foreground">
                      <span>{p.contentSourcesCount} fonti</span>
                      <span>{p.generatedContentsCount} contenuti</span>
                      <Badge variant={p.isActive ? "default" : "secondary"} className="text-[10px] ml-auto">
                        {p.isActive ? "Attivo" : "Inattivo"}
                      </Badge>
                    </div>
                    <div className="flex items-center gap-1 text-primary text-sm font-medium mt-3 opacity-0 group-hover:opacity-100 transition-opacity">
                      Apri <ArrowRight className="size-3" />
                    </div>
                  </CardContent>
                </Card>
              </Link>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
