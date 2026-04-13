"use client";

import { useState, useEffect } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Switch } from "@/components/ui/switch";
import { Loader2 } from "lucide-react";
import { toast } from "sonner";
import { agenciesApi } from "@/lib/api/agencies.api";
import type { Agency } from "@/types/api";

interface EditImageSettingsDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  agency: Agency;
  onSaved: () => void;
}

const positions = [
  { value: 0, label: "In alto a sinistra" },
  { value: 1, label: "In alto a destra" },
  { value: 2, label: "In basso a sinistra" },
  { value: 3, label: "In basso a destra" },
];

export function EditImageSettingsDialog({ open, onOpenChange, agency, onSaved }: EditImageSettingsDialogProps) {
  const [saving, setSaving] = useState(false);
  const [enableLogoOverlay, setEnableLogoOverlay] = useState(agency.enableLogoOverlay ?? false);
  const [logoOverlayPosition, setLogoOverlayPosition] = useState(agency.logoOverlayPosition ?? 3);
  const [logoUrl, setLogoUrl] = useState(agency.logoUrl ?? "");

  useEffect(() => {
    if (open) {
      setEnableLogoOverlay(agency.enableLogoOverlay ?? false);
      setLogoOverlayPosition(agency.logoOverlayPosition ?? 3);
      setLogoUrl(agency.logoUrl ?? "");
    }
  }, [open, agency]);

  const handleSubmit = async () => {
    if (enableLogoOverlay && !logoUrl.trim()) {
      toast.error("Inserisci l'URL del logo per abilitare l'overlay");
      return;
    }
    setSaving(true);
    try {
      await agenciesApi.updateImageSettings(agency.id, enableLogoOverlay, logoOverlayPosition, logoUrl || null);
      toast.success("Impostazioni immagine aggiornate");
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
          <DialogTitle>Logo Overlay Immagini</DialogTitle>
        </DialogHeader>
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <Label htmlFor="enable-overlay" className="flex-1">
              Applica logo alle immagini generate
            </Label>
            <Switch
              id="enable-overlay"
              checked={enableLogoOverlay}
              onCheckedChange={setEnableLogoOverlay}
            />
          </div>
          <div className="space-y-2">
            <Label>URL del logo</Label>
            <Input
              type="url"
              placeholder="https://example.com/logo.png"
              value={logoUrl}
              onChange={(e) => setLogoUrl(e.target.value)}
            />
            <p className="text-xs text-muted-foreground">
              PNG con sfondo trasparente consigliato
            </p>
          </div>
          <div className="space-y-2">
            <Label>Posizione overlay</Label>
            <select
              value={logoOverlayPosition}
              onChange={(e) => setLogoOverlayPosition(Number(e.target.value))}
              disabled={!enableLogoOverlay}
              className="flex h-9 w-full rounded-lg border border-input bg-transparent px-3 py-1 text-sm shadow-xs transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:opacity-50"
            >
              {positions.map((p) => (
                <option key={p.value} value={p.value}>{p.label}</option>
              ))}
            </select>
          </div>
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
