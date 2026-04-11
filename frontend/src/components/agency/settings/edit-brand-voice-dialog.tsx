"use client";

import { useState, useEffect } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Loader2 } from "lucide-react";
import { toast } from "sonner";
import { agenciesApi } from "@/lib/api/agencies.api";
import type { Agency } from "@/types/api";

interface EditBrandVoiceDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  agency: Agency;
  onSaved: () => void;
}

const toneOptions = ["professional", "casual", "playful", "formal", "friendly"];
const languageOptions = [
  { value: "it", label: "Italiano" },
  { value: "en", label: "English" },
  { value: "es", label: "Espanol" },
  { value: "fr", label: "Francais" },
  { value: "de", label: "Deutsch" },
];

export function EditBrandVoiceDialog({ open, onOpenChange, agency, onSaved }: EditBrandVoiceDialogProps) {
  const [saving, setSaving] = useState(false);
  const [tone, setTone] = useState(agency.brandVoice.tone);
  const [style, setStyle] = useState(agency.brandVoice.style);
  const [keywords, setKeywords] = useState(agency.brandVoice.keywords.join(", "));
  const [forbiddenWords, setForbiddenWords] = useState(agency.brandVoice.forbiddenWords.join(", "));
  const [language, setLanguage] = useState(agency.brandVoice.language);

  useEffect(() => {
    if (open) {
      setTone(agency.brandVoice.tone);
      setStyle(agency.brandVoice.style);
      setKeywords(agency.brandVoice.keywords.join(", "));
      setForbiddenWords(agency.brandVoice.forbiddenWords.join(", "));
      setLanguage(agency.brandVoice.language);
    }
  }, [open, agency]);

  const handleSubmit = async () => {
    setSaving(true);
    try {
      await agenciesApi.updateBrandVoice(agency.id, {
        tone,
        style,
        keywords: keywords.split(",").map((k) => k.trim()).filter(Boolean),
        examplePhrases: agency.brandVoice.examplePhrases,
        forbiddenWords: forbiddenWords.split(",").map((w) => w.trim()).filter(Boolean),
        language,
      });
      toast.success("Brand voice aggiornata");
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
          <DialogTitle>Modifica Brand Voice</DialogTitle>
        </DialogHeader>
        <div className="space-y-4">
          <div className="space-y-2">
            <Label>Tono</Label>
            <select
              value={tone}
              onChange={(e) => setTone(e.target.value)}
              className="flex h-9 w-full rounded-lg border border-input bg-transparent px-3 py-1 text-sm shadow-xs transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
            >
              {toneOptions.map((t) => (
                <option key={t} value={t}>{t.charAt(0).toUpperCase() + t.slice(1)}</option>
              ))}
            </select>
          </div>
          <div className="space-y-2">
            <Label>Stile</Label>
            <Input value={style} onChange={(e) => setStyle(e.target.value)} placeholder="es. conciso, tecnico" />
          </div>
          <div className="space-y-2">
            <Label>Keywords (separati da virgola)</Label>
            <Input value={keywords} onChange={(e) => setKeywords(e.target.value)} placeholder="keyword1, keyword2" />
          </div>
          <div className="space-y-2">
            <Label>Parole vietate (separati da virgola)</Label>
            <Input value={forbiddenWords} onChange={(e) => setForbiddenWords(e.target.value)} placeholder="parola1, parola2" />
          </div>
          <div className="space-y-2">
            <Label>Lingua</Label>
            <select
              value={language}
              onChange={(e) => setLanguage(e.target.value)}
              className="flex h-9 w-full rounded-lg border border-input bg-transparent px-3 py-1 text-sm shadow-xs transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
            >
              {languageOptions.map((l) => (
                <option key={l.value} value={l.value}>{l.label}</option>
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
