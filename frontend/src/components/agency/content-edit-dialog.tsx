"use client";

import { useEffect, useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import { resolveImageUrl } from "@/lib/utils";
import { connectorsApi } from "@/lib/api/connectors.api";
import { SocialPlatform, type GeneratedContent, type ApiResponse } from "@/types/api";
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { RichEditor } from "@/components/ui/rich-editor";
import { Loader2, Star, Sparkles, Search, TrendingUp, Save, Trash2, Calendar as CalendarIcon, Send } from "lucide-react";
import { toast } from "sonner";

const PLATFORM_OPTIONS: { value: number; label: string }[] = [
  { value: SocialPlatform.Facebook, label: "Facebook" },
  { value: SocialPlatform.Instagram, label: "Instagram" },
  { value: SocialPlatform.LinkedIn, label: "LinkedIn" },
  { value: SocialPlatform.Twitter, label: "Twitter/X" },
  { value: 5, label: "Telegram" },
];

interface ContentEditDialogProps {
  content: GeneratedContent | null;
  agencyId: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

const statusLabels: Record<number, string> = {
  1: "Bozza", 2: "In revisione", 3: "Approvato", 4: "Pubblicato", 5: "Rifiutato",
};

export function ContentEditDialog({ content, agencyId, open, onOpenChange }: ContentEditDialogProps) {
  const queryClient = useQueryClient();
  const [title, setTitle] = useState(content?.title ?? "");
  const [body, setBody] = useState(content?.body ?? "");
  const [schedulePlatform, setSchedulePlatform] = useState<number | "">("");
  const [scheduleAt, setScheduleAt] = useState<string>("");

  const { data: connectorsRes } = useQuery({
    queryKey: ["connectors", agencyId],
    queryFn: () => connectorsApi.list(agencyId),
    enabled: open,
  });
  const connectedPlatforms = useMemo(() => {
    const list = connectorsRes?.data ?? [];
    const set = new Set<number>();
    for (const c of list) if (c.isActive && (!content?.projectId || !c.projectId || c.projectId === content.projectId)) set.add(c.platform);
    return set;
  }, [connectorsRes, content?.projectId]);
  const availablePlatforms = useMemo(
    () => PLATFORM_OPTIONS.filter((p) => connectedPlatforms.has(p.value)),
    [connectedPlatforms]
  );

  useEffect(() => {
    if (content) {
      setTitle(content.title);
      setBody(content.body);
    }
  }, [content]);

  const mutation = useMutation({
    mutationFn: () =>
      apiClient.put<ApiResponse<GeneratedContent>>(
        `/api/v1/agencies/${agencyId}/content/${content?.id}`,
        { title, body }
      ),
    onSuccess: () => {
      toast.success("Contenuto aggiornato!");
      queryClient.invalidateQueries({ queryKey: ["content", agencyId] });
      onOpenChange(false);
    },
    onError: (err: Error) => {
      toast.error(err.message || "Errore durante il salvataggio");
    },
  });

  const deleteMutation = useMutation({
    mutationFn: () =>
      apiClient.delete<ApiResponse<object>>(
        `/api/v1/agencies/${agencyId}/content/${content?.id}`
      ),
    onSuccess: () => {
      toast.success("Contenuto eliminato");
      queryClient.invalidateQueries({ queryKey: ["content", agencyId] });
      queryClient.invalidateQueries({ queryKey: ["approvals", agencyId] });
      onOpenChange(false);
    },
    onError: (err: Error) => {
      toast.error(err.message || "Errore durante l'eliminazione");
    },
  });

  const scheduleMutation = useMutation({
    mutationFn: () =>
      apiClient.post<ApiResponse<{ id: string }>>(
        `/api/v1/agencies/${agencyId}/content/${content?.id}/schedule`,
        {
          scheduledAt: new Date(scheduleAt).toISOString(),
          platform: schedulePlatform === "" ? null : schedulePlatform,
        }
      ),
    onSuccess: () => {
      toast.success("Contenuto programmato!");
      queryClient.invalidateQueries({ queryKey: ["project-calendar"] });
      queryClient.invalidateQueries({ queryKey: ["content", agencyId] });
      setSchedulePlatform("");
      setScheduleAt("");
    },
    onError: (err: Error) => toast.error(err.message || "Programmazione fallita"),
  });

  const handleDelete = () => {
    if (confirm(`Eliminare definitivamente "${content?.title}"?`)) {
      deleteMutation.mutate();
    }
  };

  if (!content) return null;

  // Convert markdown-ish body to basic HTML for the editor
  const htmlBody = body.includes("<") ? body : body.replace(/\n/g, "<br/>");

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-4xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            Modifica Contenuto
            <Badge variant="outline">{statusLabels[content.status] ?? "Sconosciuto"}</Badge>
          </DialogTitle>
        </DialogHeader>

        <div className="space-y-4">
          {/* Scores */}
          <div className="grid grid-cols-4 gap-3">
            <div className="flex items-center gap-1.5 text-sm">
              <Star className="size-3.5 text-amber-500" />
              <span className="font-medium">{content.overallScore}/10</span>
              <span className="text-muted-foreground text-xs">Overall</span>
            </div>
            <div className="flex items-center gap-1.5 text-sm">
              <Sparkles className="size-3.5 text-blue-500" />
              <span className="font-medium">{content.qualityScore}</span>
              <span className="text-muted-foreground text-xs">Qualita</span>
            </div>
            <div className="flex items-center gap-1.5 text-sm">
              <Search className="size-3.5 text-green-500" />
              <span className="font-medium">{content.seoScore}</span>
              <span className="text-muted-foreground text-xs">SEO</span>
            </div>
            <div className="flex items-center gap-1.5 text-sm">
              <TrendingUp className="size-3.5 text-violet-500" />
              <span className="font-medium">{content.brandVoiceScore}</span>
              <span className="text-muted-foreground text-xs">Brand</span>
            </div>
          </div>

          {/* Image */}
          {content.imageUrl && (
            <div className="w-full rounded-lg bg-muted/30 border flex items-center justify-center overflow-hidden">
              <img
                src={resolveImageUrl(content.imageUrl)}
                alt={content.title}
                className="max-h-[50vh] w-auto max-w-full object-contain"
              />
            </div>
          )}

          {/* Title */}
          <div className="space-y-1.5">
            <Label htmlFor="edit-title">Titolo</Label>
            <Input
              id="edit-title"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="Titolo del contenuto"
            />
          </div>

          {/* Body */}
          <div className="space-y-1.5">
            <Label>Contenuto</Label>
            <RichEditor content={htmlBody} onChange={setBody} />
          </div>

          {content.scoreExplanation && (
            <p className="text-xs text-muted-foreground bg-muted/50 p-2 rounded">
              {content.scoreExplanation}
            </p>
          )}

          {/* Schedule */}
          <div className="space-y-2 p-3 border rounded-md bg-muted/20">
            <Label className="flex items-center gap-2 text-sm">
              <CalendarIcon className="size-4" /> Programma pubblicazione
            </Label>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-2">
              <Input
                type="datetime-local"
                value={scheduleAt}
                onChange={(e) => setScheduleAt(e.target.value)}
                data-testid="schedule-datetime"
              />
              <select
                className="h-9 rounded-md border bg-transparent px-3 text-sm"
                value={schedulePlatform}
                onChange={(e) => setSchedulePlatform(e.target.value ? Number(e.target.value) : "")}
                data-testid="schedule-platform"
              >
                <option value="">Nessuna piattaforma</option>
                {availablePlatforms.length === 0 && (
                  <option disabled>Nessun account social collegato</option>
                )}
                {availablePlatforms.map((p) => (
                  <option key={p.value} value={p.value}>{p.label}</option>
                ))}
              </select>
              <Button
                size="sm"
                onClick={() => scheduleMutation.mutate()}
                disabled={!scheduleAt || scheduleMutation.isPending}
                data-testid="schedule-submit"
              >
                {scheduleMutation.isPending ? (
                  <><Loader2 className="size-4 animate-spin" /> Programmazione...</>
                ) : (
                  <><Send className="size-4" /> Programma</>
                )}
              </Button>
            </div>
            <p className="text-[10px] text-muted-foreground">
              Il contenuto verrà pubblicato automaticamente all&apos;orario indicato. Lascia &quot;Nessuna piattaforma&quot; per una voce di sola bozza.
            </p>
          </div>
        </div>

        <DialogFooter className="sm:justify-between gap-2">
          <Button
            variant="destructive"
            onClick={handleDelete}
            disabled={deleteMutation.isPending || mutation.isPending}
          >
            {deleteMutation.isPending ? (
              <><Loader2 className="size-4 animate-spin" /> Eliminazione...</>
            ) : (
              <><Trash2 className="size-4" /> Elimina</>
            )}
          </Button>
          <div className="flex gap-2">
            <Button variant="outline" onClick={() => onOpenChange(false)}>
              Annulla
            </Button>
            <Button onClick={() => mutation.mutate()} disabled={mutation.isPending || deleteMutation.isPending}>
              {mutation.isPending ? (
                <><Loader2 className="size-4 animate-spin" /> Salvataggio...</>
              ) : (
                <><Save className="size-4" /> Salva modifiche</>
              )}
            </Button>
          </div>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
