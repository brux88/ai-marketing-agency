"use client";

import { useParams } from "next/navigation";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { agenciesApi } from "@/lib/api/agencies.api";
import { apiClient } from "@/lib/api/client";
import type { ApiResponse, GeneratedContent } from "@/types/api";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { PenLine, Loader2, FileText, Star, Search, TrendingUp, Sparkles, CheckCircle2, ImageIcon } from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";

const statusConfig: Record<number, { label: string; variant: "default" | "secondary" | "outline" | "destructive" }> = {
  1: { label: "Bozza", variant: "outline" },
  2: { label: "In revisione", variant: "secondary" },
  3: { label: "Approvato", variant: "default" },
  4: { label: "Pubblicato", variant: "default" },
  5: { label: "Rifiutato", variant: "destructive" },
};

export default function ContentPage() {
  const { agencyId } = useParams();
  const queryClient = useQueryClient();
  const [generating, setGenerating] = useState(false);

  const { data: agencyData } = useQuery({
    queryKey: ["agency", agencyId],
    queryFn: () => agenciesApi.get(agencyId as string),
  });

  const { data, isLoading, refetch } = useQuery({
    queryKey: ["content", agencyId],
    queryFn: () =>
      apiClient.get<ApiResponse<GeneratedContent[]>>(`/api/v1/agencies/${agencyId}/content`),
  });

  const agency = agencyData?.data;
  const contents = data?.data || [];

  const handleGenerate = async () => {
    if (!agency?.defaultLlmProviderKeyId) {
      toast.error("Configura prima una chiave LLM nelle impostazioni dell'agenzia");
      return;
    }
    setGenerating(true);
    try {
      await apiClient.post(`/api/v1/agencies/${agencyId}/agents/content-writer/run`, {});
      toast.success("Generazione articolo avviata!");
      refetch();
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
          <h2 className="text-xl font-bold tracking-tight">Content & Blog</h2>
          <p className="text-muted-foreground text-sm mt-1">
            {contents.length} contenuti generati
          </p>
        </div>
        <Button onClick={handleGenerate} disabled={generating}>
          {generating ? (
            <><Loader2 className="size-4 animate-spin" /> Generazione...</>
          ) : (
            <><PenLine className="size-4" /> Genera nuovo articolo</>
          )}
        </Button>
      </div>

      {isLoading ? (
        <div className="space-y-4">
          {[1, 2, 3].map((i) => (
            <Card key={i}>
              <CardContent className="pt-6">
                <Skeleton className="h-5 w-3/4 mb-3" />
                <Skeleton className="h-4 w-full" />
                <Skeleton className="h-4 w-2/3 mt-2" />
              </CardContent>
            </Card>
          ))}
        </div>
      ) : contents.length === 0 ? (
        <Card className="text-center py-16">
          <CardContent className="space-y-4">
            <div className="size-16 rounded-full bg-primary/10 flex items-center justify-center mx-auto">
              <FileText className="size-8 text-primary" />
            </div>
            <h3 className="text-lg font-semibold">Nessun contenuto</h3>
            <p className="text-muted-foreground max-w-sm mx-auto">
              Clicca &quot;Genera nuovo articolo&quot; per far lavorare il Content Writer AI
            </p>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-4">
          {contents.map((content) => {
            const status = statusConfig[content.status] || statusConfig[1];
            return (
              <Card key={content.id} className="hover:shadow-sm transition-shadow">
                <CardContent className="pt-6">
                  <div className="flex items-start justify-between gap-4">
                    <div className="flex items-start gap-4 flex-1 min-w-0">
                      {content.imageUrl && (
                        <img
                          src={content.imageUrl}
                          alt={content.title}
                          className="size-16 rounded-lg object-cover shrink-0"
                        />
                      )}
                      <div className="flex-1 min-w-0">
                        <h3 className="font-semibold truncate">{content.title}</h3>
                        <p className="text-muted-foreground text-sm mt-1 line-clamp-2">
                          {content.body.slice(0, 200)}...
                        </p>
                      </div>
                    </div>
                    <div className="flex items-center gap-2 shrink-0">
                      {content.imageUrl && (
                        <Badge variant="secondary">
                          <ImageIcon className="size-3" /> Img
                        </Badge>
                      )}
                      {content.autoApproved && (
                        <Badge variant="secondary">
                          <CheckCircle2 className="size-3" /> Auto
                        </Badge>
                      )}
                      <Badge variant={status.variant}>{status.label}</Badge>
                    </div>
                  </div>

                  <div className="grid grid-cols-4 gap-4 mt-4 pt-4 border-t">
                    <div className="text-center">
                      <div className="flex items-center justify-center gap-1 text-sm font-medium">
                        <Star className="size-3.5 text-amber-500" />
                        {content.overallScore}/10
                      </div>
                      <p className="text-xs text-muted-foreground">Overall</p>
                    </div>
                    <div className="text-center">
                      <div className="flex items-center justify-center gap-1 text-sm font-medium">
                        <Sparkles className="size-3.5 text-blue-500" />
                        {content.qualityScore}
                      </div>
                      <p className="text-xs text-muted-foreground">Qualita</p>
                    </div>
                    <div className="text-center">
                      <div className="flex items-center justify-center gap-1 text-sm font-medium">
                        <Search className="size-3.5 text-green-500" />
                        {content.seoScore}
                      </div>
                      <p className="text-xs text-muted-foreground">SEO</p>
                    </div>
                    <div className="text-center">
                      <div className="flex items-center justify-center gap-1 text-sm font-medium">
                        <TrendingUp className="size-3.5 text-violet-500" />
                        {content.brandVoiceScore}
                      </div>
                      <p className="text-xs text-muted-foreground">Brand</p>
                    </div>
                  </div>
                </CardContent>
              </Card>
            );
          })}
        </div>
      )}
    </div>
  );
}
