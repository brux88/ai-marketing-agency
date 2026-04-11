"use client";

import { useState, useEffect } from "react";
import { useQuery } from "@tanstack/react-query";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Loader2 } from "lucide-react";
import { toast } from "sonner";
import { agenciesApi } from "@/lib/api/agencies.api";
import { apiClient } from "@/lib/api/client";
import type { Agency, ApiResponse, LlmKey, LlmProviderCategory } from "@/types/api";

interface EditDefaultLlmDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  agency: Agency;
  onSaved: () => void;
}

export function EditDefaultLlmDialog({ open, onOpenChange, agency, onSaved }: EditDefaultLlmDialogProps) {
  const [saving, setSaving] = useState(false);
  const [textKeyId, setTextKeyId] = useState(agency.defaultLlmProviderKeyId || "");
  const [imageKeyId, setImageKeyId] = useState(agency.imageLlmProviderKeyId || "");

  const { data: keysData } = useQuery({
    queryKey: ["llm-keys"],
    queryFn: () => apiClient.get<ApiResponse<LlmKey[]>>("/api/v1/llmkeys"),
    enabled: open,
  });

  const keys = keysData?.data || [];
  const textKeys = keys.filter((k) => k.isActive && (k.category === undefined || k.category === 0));
  const imageKeys = keys.filter((k) => k.isActive && k.category === 1);
  const allActiveKeys = keys.filter((k) => k.isActive);

  useEffect(() => {
    if (open) {
      setTextKeyId(agency.defaultLlmProviderKeyId || "");
      setImageKeyId(agency.imageLlmProviderKeyId || "");
    }
  }, [open, agency]);

  const handleSubmit = async () => {
    setSaving(true);
    try {
      await agenciesApi.updateDefaultLlm(
        agency.id,
        textKeyId || null,
        imageKeyId || null,
      );
      toast.success("Provider LLM aggiornati");
      onSaved();
      onOpenChange(false);
    } catch (err: any) {
      const message = err?.message || "Errore durante il salvataggio";
      toast.error(message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Provider LLM</DialogTitle>
        </DialogHeader>
        <div className="space-y-4">
          <div className="space-y-2">
            <Label>Provider Testo</Label>
            <select
              value={textKeyId}
              onChange={(e) => setTextKeyId(e.target.value)}
              className="flex h-9 w-full rounded-lg border border-input bg-transparent px-3 py-1 text-sm shadow-xs transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
            >
              <option value="">Nessuno</option>
              {(textKeys.length > 0 ? textKeys : allActiveKeys).map((k) => (
                <option key={k.id} value={k.id}>{k.displayName}{k.modelName ? ` (${k.modelName})` : ""}</option>
              ))}
            </select>
          </div>
          <div className="space-y-2">
            <Label>Provider Immagini</Label>
            <select
              value={imageKeyId}
              onChange={(e) => setImageKeyId(e.target.value)}
              className="flex h-9 w-full rounded-lg border border-input bg-transparent px-3 py-1 text-sm shadow-xs transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
            >
              <option value="">Nessuno</option>
              {(imageKeys.length > 0 ? imageKeys : allActiveKeys).map((k) => (
                <option key={k.id} value={k.id}>{k.displayName}{k.modelName ? ` (${k.modelName})` : ""}</option>
              ))}
            </select>
          </div>
          {allActiveKeys.length === 0 && (
            <p className="text-sm text-muted-foreground">
              Nessuna chiave API disponibile. Aggiungine una nelle impostazioni.
            </p>
          )}
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Annulla</Button>
          <Button onClick={handleSubmit} disabled={saving}>
            {saving ? <><Loader2 className="size-4 animate-spin" /> Salvataggio...</> : "Salva"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
