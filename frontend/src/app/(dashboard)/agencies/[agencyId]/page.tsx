"use client";

import { useParams } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import { agenciesApi } from "@/lib/api/agencies.api";
import Link from "next/link";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { PenLine, Share2, Mail, BarChart3, Rss, FileText, ArrowRight, Globe } from "lucide-react";

const agents = [
  { type: "content-writer", label: "Content Writer", desc: "Genera articoli blog da fonti reali", icon: PenLine, route: "content", color: "text-blue-600 bg-blue-100 dark:bg-blue-950" },
  { type: "social-manager", label: "Social Manager", desc: "Crea post per tutti i social", icon: Share2, route: "social", color: "text-violet-600 bg-violet-100 dark:bg-violet-950" },
  { type: "newsletter", label: "Newsletter", desc: "Cura e genera newsletter", icon: Mail, route: "newsletter", color: "text-emerald-600 bg-emerald-100 dark:bg-emerald-950" },
  { type: "analytics", label: "Analytics", desc: "Analizza performance e report", icon: BarChart3, route: "analytics", color: "text-amber-600 bg-amber-100 dark:bg-amber-950" },
];

export default function AgencyOverviewPage() {
  const { agencyId } = useParams();
  const { data, isLoading } = useQuery({
    queryKey: ["agency", agencyId],
    queryFn: () => agenciesApi.get(agencyId as string),
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

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">{agency.name}</h1>
        <p className="text-muted-foreground mt-1">{agency.productName}</p>
        {agency.description && <p className="text-sm text-muted-foreground mt-2">{agency.description}</p>}
      </div>

      <div className="grid md:grid-cols-2 gap-6">
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Statistiche</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <div className="flex items-center justify-between text-sm">
              <span className="flex items-center gap-2 text-muted-foreground">
                <Rss className="size-4" /> Fonti contenuto
              </span>
              <span className="font-medium">{agency.contentSourcesCount}</span>
            </div>
            <div className="flex items-center justify-between text-sm">
              <span className="flex items-center gap-2 text-muted-foreground">
                <FileText className="size-4" /> Contenuti generati
              </span>
              <span className="font-medium">{agency.generatedContentsCount}</span>
            </div>
            <div className="flex items-center justify-between text-sm">
              <span className="text-muted-foreground">Approvazione</span>
              <Badge variant="outline">
                {agency.approvalMode === 1 ? "Manuale" :
                 agency.approvalMode === 2 ? "Automatica" :
                 `Auto sopra ${agency.autoApproveMinScore}/10`}
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
              <span className="font-medium capitalize">{agency.brandVoice.tone}</span>
            </div>
            <div className="flex items-center justify-between text-sm">
              <span className="flex items-center gap-2 text-muted-foreground">
                <Globe className="size-4" /> Lingua
              </span>
              <span className="font-medium uppercase">{agency.brandVoice.language}</span>
            </div>
            {agency.brandVoice.keywords.length > 0 && (
              <div className="flex flex-wrap gap-1.5 pt-1">
                {agency.brandVoice.keywords.map((kw) => (
                  <Badge key={kw} variant="secondary" className="text-xs">{kw}</Badge>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      <div>
        <h2 className="text-lg font-semibold tracking-tight mb-4">Lancia un agente AI</h2>
        <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-4">
          {agents.map((agent) => (
            <Link key={agent.type} href={`/agencies/${agencyId}/${agent.route}`}>
              <Card className="hover:shadow-md transition-all duration-200 hover:-translate-y-0.5 h-full group">
                <CardContent className="pt-6">
                  <div className={`size-10 rounded-lg flex items-center justify-center mb-3 ${agent.color}`}>
                    <agent.icon className="size-5" />
                  </div>
                  <h4 className="font-semibold">{agent.label}</h4>
                  <p className="text-muted-foreground text-sm mt-1">{agent.desc}</p>
                  <div className="flex items-center gap-1 text-primary text-sm font-medium mt-3 opacity-0 group-hover:opacity-100 transition-opacity">
                    Apri <ArrowRight className="size-3" />
                  </div>
                </CardContent>
              </Card>
            </Link>
          ))}
        </div>
      </div>
    </div>
  );
}
