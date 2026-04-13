"use client";

import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import type { GeneratedContent, ApiResponse } from "@/types/api";
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { RichEditor } from "@/components/ui/rich-editor";
import { Loader2, Star, Sparkles, Search, TrendingUp, Save } from "lucide-react";
import { toast } from "sonner";

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

  // Reset when content changes
  if (content && title !== content.title && body !== content.body) {
    setTitle(content.title);
    setBody(content.body);
  }

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
            <img
              src={content.imageUrl}
              alt={content.title}
              className="w-full max-h-48 object-cover rounded-lg"
            />
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
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Annulla
          </Button>
          <Button onClick={() => mutation.mutate()} disabled={mutation.isPending}>
            {mutation.isPending ? (
              <><Loader2 className="size-4 animate-spin" /> Salvataggio...</>
            ) : (
              <><Save className="size-4" /> Salva modifiche</>
            )}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
