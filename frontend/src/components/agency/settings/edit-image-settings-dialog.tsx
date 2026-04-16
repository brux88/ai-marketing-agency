"use client";

import { useState, useEffect, useRef } from "react";
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
  const [uploading, setUploading] = useState(false);
  const [enableLogoOverlay, setEnableLogoOverlay] = useState(agency.enableLogoOverlay ?? false);
  const [logoOverlayPosition, setLogoOverlayPosition] = useState(agency.logoOverlayPosition ?? 3);
  const [logoOverlayMode, setLogoOverlayMode] = useState(agency.logoOverlayMode ?? 0);
  const [brandBannerColor, setBrandBannerColor] = useState(agency.brandBannerColor ?? "#003366");
  const [logoUrl, setLogoUrl] = useState(agency.logoUrl ?? "");
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFileSelected = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    if (file.size > 5 * 1024 * 1024) {
      toast.error("File troppo grande (max 5 MB)");
      return;
    }
    setUploading(true);
    try {
      const res = await agenciesApi.uploadLogo(agency.id, file);
      if (res.data?.url) {
        setLogoUrl(res.data.url);
        toast.success("Logo caricato");
      }
    } catch (err: any) {
      toast.error(err?.message || "Upload fallito");
    } finally {
      setUploading(false);
      if (fileInputRef.current) fileInputRef.current.value = "";
    }
  };

  useEffect(() => {
    if (open) {
      setEnableLogoOverlay(agency.enableLogoOverlay ?? false);
      setLogoOverlayPosition(agency.logoOverlayPosition ?? 3);
      setLogoOverlayMode(agency.logoOverlayMode ?? 0);
      setBrandBannerColor(agency.brandBannerColor ?? "#003366");
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
      await agenciesApi.updateImageSettings(agency.id, enableLogoOverlay, logoOverlayPosition, logoUrl || null, logoOverlayMode, brandBannerColor || null);
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
      <DialogContent className="sm:max-w-md max-w-[calc(100vw-2rem)] overflow-hidden">
        <DialogHeader>
          <DialogTitle>Logo Overlay Immagini</DialogTitle>
        </DialogHeader>
        <div className="space-y-4 min-w-0">
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
            <Label>Logo</Label>
            <div className="flex gap-2 min-w-0">
              <Input
                type="url"
                placeholder="https://example.com/logo.png"
                value={logoUrl}
                onChange={(e) => setLogoUrl(e.target.value)}
                className="flex-1 min-w-0"
              />
              <Button
                type="button"
                variant="outline"
                size="sm"
                disabled={uploading}
                onClick={() => fileInputRef.current?.click()}
              >
                {uploading ? <Loader2 className="size-4 animate-spin" /> : "Carica"}
              </Button>
              <input
                ref={fileInputRef}
                type="file"
                accept="image/png,image/jpeg,image/webp"
                className="hidden"
                onChange={handleFileSelected}
              />
            </div>
            {logoUrl && (
              <div className="flex items-center gap-2 pt-1 min-w-0 max-w-full">
                <img
                  src={logoUrl}
                  alt="Logo preview"
                  className="h-10 max-w-[3rem] rounded border bg-muted object-contain p-1 shrink-0"
                />
                <span className="text-[10px] text-muted-foreground truncate min-w-0 flex-1">{logoUrl}</span>
              </div>
            )}
            <p className="text-xs text-muted-foreground">
              PNG con sfondo trasparente consigliato (max 5 MB)
            </p>
          </div>
          <div className="space-y-2">
            <Label>Modalità</Label>
            <div className="flex gap-2">
              <Button
                type="button"
                variant={logoOverlayMode === 0 ? "default" : "outline"}
                size="sm"
                className="flex-1"
                disabled={!enableLogoOverlay}
                onClick={() => setLogoOverlayMode(0)}
              >
                Overlay sull'immagine
              </Button>
              <Button
                type="button"
                variant={logoOverlayMode === 1 ? "default" : "outline"}
                size="sm"
                className="flex-1"
                disabled={!enableLogoOverlay}
                onClick={() => setLogoOverlayMode(1)}
              >
                Banner brand sotto
              </Button>
            </div>
          </div>
          {logoOverlayMode === 0 && (
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
          )}
          {logoOverlayMode === 1 && (
            <div className="space-y-2">
              <Label>Colore banner brand</Label>
              <div className="flex gap-2 items-center">
                <input
                  type="color"
                  value={brandBannerColor}
                  onChange={(e) => setBrandBannerColor(e.target.value)}
                  disabled={!enableLogoOverlay}
                  className="h-9 w-14 rounded border border-input disabled:opacity-50"
                />
                <Input
                  type="text"
                  value={brandBannerColor}
                  onChange={(e) => setBrandBannerColor(e.target.value)}
                  placeholder="#003366"
                  disabled={!enableLogoOverlay}
                  className="flex-1"
                />
              </div>
              <p className="text-xs text-muted-foreground">
                Banner colorato aggiunto sotto all'immagine con il logo al suo interno.
              </p>
            </div>
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
