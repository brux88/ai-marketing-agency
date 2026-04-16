"use client";

import { useState, useEffect } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Slider } from "@/components/ui/slider";
import { Loader2, Image as ImageIcon, Images, Ban } from "lucide-react";
import { toast } from "sonner";
import { apiClient } from "@/lib/api/client";
import { ImageGenerationMode } from "@/types/api";
import { useQueryClient } from "@tanstack/react-query";
import { projectsApi } from "@/lib/api/projects.api";

interface GenerateContentDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  agencyId: string;
  agentType: string;
  agentLabel: string;
  projectId?: string | null;
  showInput?: boolean;
  onStarted?: () => void;
}

export function GenerateContentDialog({
  open,
  onOpenChange,
  agencyId,
  agentType,
  agentLabel,
  projectId,
  showInput = true,
  onStarted,
}: GenerateContentDialogProps) {
  const queryClient = useQueryClient();
  const [starting, setStarting] = useState(false);
  const [input, setInput] = useState("");
  const [imageMode, setImageMode] = useState<ImageGenerationMode>(ImageGenerationMode.Single);
  const [imageCount, setImageCount] = useState(4);
  const isSocial = agentType === "social-manager";
  const ALL_PLATFORMS = ["TWITTER", "LINKEDIN", "INSTAGRAM", "FACEBOOK"] as const;
  const PLATFORM_LABELS: Record<string, string> = {
    TWITTER: "Twitter/X",
    LINKEDIN: "LinkedIn",
    INSTAGRAM: "Instagram",
    FACEBOOK: "Facebook",
  };
  const [selectedPlatforms, setSelectedPlatforms] = useState<string[]>([...ALL_PLATFORMS]);

  useEffect(() => {
    if (!open) return;
    setInput("");
    setImageMode(ImageGenerationMode.Single);
    setImageCount(4);
    setSelectedPlatforms([...ALL_PLATFORMS]);
    if (isSocial && projectId) {
      projectsApi.get(agencyId, projectId).then((res: any) => {
        const project = res?.data ?? res;
        const pref = project?.enabledSocialPlatforms;
        if (pref && typeof pref === "string" && pref.trim()) {
          const list = pref.split(",").map((p: string) => p.trim().toUpperCase()).filter(Boolean);
          if (list.length > 0) setSelectedPlatforms(list);
        }
      }).catch(() => {});
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, isSocial, projectId, agencyId]);

  const togglePlatform = (p: string) => {
    setSelectedPlatforms((cur) =>
      cur.includes(p) ? cur.filter((x) => x !== p) : [...cur, p]
    );
  };

  const handleStart = async () => {
    if (isSocial && selectedPlatforms.length === 0) {
      toast.error("Seleziona almeno una piattaforma social");
      return;
    }
    setStarting(true);
    try {
      if (isSocial && projectId) {
        await projectsApi.updateSocialPlatforms(
          agencyId,
          projectId,
          selectedPlatforms.join(",")
        );
      }
      await apiClient.post(`/api/v1/agencies/${agencyId}/agents/${agentType}/run`, {
        input: input || null,
        projectId: projectId || null,
        imageMode,
        imageCount,
      });
      toast.success(`${agentLabel} avviato!`);
      queryClient.invalidateQueries({ queryKey: ["jobs"] });
      queryClient.invalidateQueries({ queryKey: ["notifications"] });
      onStarted?.();
      onOpenChange(false);
    } catch (err: any) {
      const message = err?.message || "Errore durante l'avvio";
      toast.error(message);
    } finally {
      setStarting(false);
    }
  };

  const modes = [
    { value: ImageGenerationMode.None, label: "Nessuna", icon: Ban, desc: "Solo testo" },
    { value: ImageGenerationMode.Single, label: "Singola", icon: ImageIcon, desc: "1 immagine" },
    { value: ImageGenerationMode.Carousel, label: "Carosello", icon: Images, desc: "Piu immagini coerenti" },
  ];

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Avvia {agentLabel}</DialogTitle>
        </DialogHeader>
        <div className="space-y-4">
          {isSocial && projectId && (
            <div className="space-y-2">
              <Label>Piattaforme da generare</Label>
              <div className="grid grid-cols-2 gap-2">
                {ALL_PLATFORMS.map((p) => {
                  const active = selectedPlatforms.includes(p);
                  return (
                    <button
                      key={p}
                      type="button"
                      onClick={() => togglePlatform(p)}
                      className={`rounded-lg border p-2 text-left text-sm transition-colors ${
                        active
                          ? "border-primary bg-primary/5 ring-1 ring-primary"
                          : "border-input hover:bg-accent"
                      }`}
                    >
                      <span className="font-medium">{PLATFORM_LABELS[p]}</span>
                    </button>
                  );
                })}
              </div>
              <p className="text-xs text-muted-foreground">
                La selezione viene salvata come preferenza del progetto.
              </p>
            </div>
          )}

          {showInput && (
            <div className="space-y-2">
              <Label>Input (opzionale)</Label>
              <Input
                placeholder="Argomento o istruzione..."
                value={input}
                onChange={(e) => setInput(e.target.value)}
              />
            </div>
          )}

          <div className="space-y-2">
            <Label>Modalita immagine</Label>
            <div className="grid grid-cols-3 gap-2">
              {modes.map((m) => (
                <button
                  key={m.value}
                  type="button"
                  onClick={() => setImageMode(m.value)}
                  className={`rounded-lg border p-3 text-left transition-colors ${
                    imageMode === m.value
                      ? "border-primary bg-primary/5 ring-1 ring-primary"
                      : "border-input hover:bg-accent"
                  }`}
                >
                  <m.icon className="size-4 mb-1" />
                  <div className="text-sm font-medium">{m.label}</div>
                  <div className="text-xs text-muted-foreground">{m.desc}</div>
                </button>
              ))}
            </div>
          </div>

          {imageMode === ImageGenerationMode.Carousel && (
            <div className="space-y-3">
              <Label>Numero di slide: {imageCount}</Label>
              <Slider
                value={[imageCount]}
                onValueChange={(val) => setImageCount(Array.isArray(val) ? val[0] : val)}
                min={2}
                max={10}
              />
              <p className="text-xs text-muted-foreground">
                Le slide conderanno stile, palette e composizione coerenti
              </p>
            </div>
          )}
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Annulla</Button>
          <Button onClick={handleStart} disabled={starting}>
            {starting ? <><Loader2 className="size-4 animate-spin" /> Avvio...</> : "Avvia"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
