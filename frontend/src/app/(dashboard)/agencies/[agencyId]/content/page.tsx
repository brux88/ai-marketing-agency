"use client";

import { useParams } from "next/navigation";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { agenciesApi } from "@/lib/api/agencies.api";
import { apiClient } from "@/lib/api/client";
import { resolveImageUrl } from "@/lib/utils";
import type { ApiResponse, GeneratedContent } from "@/types/api";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { PenLine, Loader2, FileText, Star, Search, TrendingUp, Sparkles, CheckCircle2, ImageIcon, X, Edit3, Calendar, Images } from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";
import { ContentEditDialog } from "@/components/agency/content-edit-dialog";
import { GenerateContentDialog } from "@/components/agency/generate-content-dialog";

const statusConfig: Record<number, { label: string; variant: "default" | "secondary" | "outline" | "destructive" }> = {
  1: { label: "Bozza", variant: "outline" },
  2: { label: "In revisione", variant: "secondary" },
  3: { label: "Approvato", variant: "default" },
  4: { label: "Pubblicato", variant: "default" },
  5: { label: "Rifiutato", variant: "destructive" },
};

const contentTypeLabels: Record<number, string> = {
  1: "Blog",
  2: "Social",
  3: "Newsletter",
  4: "Report",
};

type ContentFilter = "all" | 1 | 2 | 3 | 4;

export default function ContentPage() {
  const { agencyId } = useParams();
  const queryClient = useQueryClient();
  const [generateOpen, setGenerateOpen] = useState(false);
  const [filter, setFilter] = useState<ContentFilter>("all");
  const [lightboxUrl, setLightboxUrl] = useState<string | null>(null);
  const [editingContent, setEditingContent] = useState<GeneratedContent | null>(null);

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
  const allContents = [...(data?.data || [])].sort(
    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
  );
  const contents = filter === "all" ? allContents : allContents.filter((c) => c.contentType === filter);

  const handleOpenGenerate = () => {
    if (!agency?.defaultLlmProviderKeyId) {
      toast.error("Configura prima una chiave LLM nelle impostazioni dell'agenzia");
      return;
    }
    setGenerateOpen(true);
  };

  const formatDate = (iso: string) => {
    const d = new Date(iso);
    return d.toLocaleString("it-IT", { day: "2-digit", month: "short", year: "numeric", hour: "2-digit", minute: "2-digit" });
  };

  const filterButtons: { value: ContentFilter; label: string }[] = [
    { value: "all", label: "Tutti" },
    { value: 1, label: "Blog" },
    { value: 2, label: "Social" },
    { value: 3, label: "Newsletter" },
    { value: 4, label: "Report" },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold tracking-tight">Content & Blog</h2>
          <p className="text-muted-foreground text-sm mt-1">
            {allContents.length} contenuti generati
          </p>
        </div>
        <Button onClick={handleOpenGenerate}>
          <PenLine className="size-4" /> Genera nuovo articolo
        </Button>
      </div>

      {/* Filtri per tipo */}
      <div className="flex gap-2 flex-wrap">
        {filterButtons.map((fb) => (
          <Button
            key={String(fb.value)}
            variant={filter === fb.value ? "default" : "outline"}
            size="sm"
            onClick={() => setFilter(fb.value)}
          >
            {fb.label}
            {fb.value !== "all" && (
              <span className="ml-1 text-xs opacity-70">
                ({allContents.filter((c) => c.contentType === fb.value).length})
              </span>
            )}
          </Button>
        ))}
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
              <Card key={content.id} className="hover:shadow-sm transition-shadow cursor-pointer" onClick={() => setEditingContent(content)}>
                <CardContent className="pt-6">
                  <div className="flex items-start justify-between gap-4">
                    <div className="flex items-start gap-4 flex-1 min-w-0">
                      {content.imageUrl && (
                        <img
                          src={resolveImageUrl(content.imageUrl)}
                          alt={content.title}
                          className="w-24 h-24 rounded-lg object-cover shrink-0 cursor-pointer hover:opacity-80 transition-opacity ring-1 ring-border"
                          onClick={(e) => { e.stopPropagation(); setLightboxUrl(resolveImageUrl(content.imageUrl)!); }}
                        />
                      )}
                      <div className="flex-1 min-w-0">
                        <h3 className="font-semibold truncate">{content.title}</h3>
                        <p className="text-muted-foreground text-sm mt-1 line-clamp-2">
                          {content.body.slice(0, 200)}...
                        </p>
                        <div className="flex items-center gap-1.5 mt-2 text-xs text-muted-foreground">
                          <Calendar className="size-3" />
                          <span>{formatDate(content.createdAt)}</span>
                        </div>
                      </div>
                    </div>
                    <div className="flex items-center gap-2 shrink-0">
                      <Badge variant="outline">{contentTypeLabels[content.contentType] || "Altro"}</Badge>
                      {content.imageUrls && content.imageUrls.length > 1 ? (
                        <Badge variant="secondary">
                          <Images className="size-3" /> {content.imageUrls.length} slide
                        </Badge>
                      ) : content.imageUrl && (
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

      {/* Generate Dialog */}
      <GenerateContentDialog
        open={generateOpen}
        onOpenChange={setGenerateOpen}
        agencyId={agencyId as string}
        agentType="content-writer"
        agentLabel="Content Writer"
        onStarted={() => refetch()}
      />

      {/* Content Edit Dialog */}
      <ContentEditDialog
        content={editingContent}
        agencyId={agencyId as string}
        open={!!editingContent}
        onOpenChange={(open) => { if (!open) setEditingContent(null); }}
      />

      {/* Lightbox per ingrandire immagini */}
      {lightboxUrl && (
        <div
          className="fixed inset-0 z-50 bg-black/80 flex items-center justify-center p-4"
          onClick={() => setLightboxUrl(null)}
        >
          <button
            onClick={() => setLightboxUrl(null)}
            className="absolute top-4 right-4 text-white hover:text-gray-300"
          >
            <X className="size-8" />
          </button>
          <img
            src={lightboxUrl}
            alt="Immagine ingrandita"
            className="max-w-full max-h-[90vh] rounded-lg object-contain"
            onClick={(e) => e.stopPropagation()}
          />
        </div>
      )}
    </div>
  );
}
