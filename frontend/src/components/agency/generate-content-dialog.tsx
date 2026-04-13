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
  const [starting, setStarting] = useState(false);
  const [input, setInput] = useState("");
  const [imageMode, setImageMode] = useState<ImageGenerationMode>(ImageGenerationMode.Single);
  const [imageCount, setImageCount] = useState(4);

  useEffect(() => {
    if (open) {
      setInput("");
      setImageMode(ImageGenerationMode.Single);
      setImageCount(4);
    }
  }, [open]);

  const handleStart = async () => {
    setStarting(true);
    try {
      await apiClient.post(`/api/v1/agencies/${agencyId}/agents/${agentType}/run`, {
        input: input || null,
        projectId: projectId || null,
        imageMode,
        imageCount,
      });
      toast.success(`${agentLabel} avviato!`);
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
