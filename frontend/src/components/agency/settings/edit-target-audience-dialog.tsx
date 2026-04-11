"use client";

import { useState, useEffect } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Loader2 } from "lucide-react";
import { toast } from "sonner";
import { agenciesApi } from "@/lib/api/agencies.api";
import type { Agency } from "@/types/api";

interface EditTargetAudienceDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  agency: Agency;
  onSaved: () => void;
}

export function EditTargetAudienceDialog({ open, onOpenChange, agency, onSaved }: EditTargetAudienceDialogProps) {
  const [saving, setSaving] = useState(false);
  const [description, setDescription] = useState(agency.targetAudience.description);
  const [ageRange, setAgeRange] = useState(agency.targetAudience.ageRange || "");
  const [interests, setInterests] = useState(agency.targetAudience.interests.join(", "));
  const [painPoints, setPainPoints] = useState(agency.targetAudience.painPoints.join(", "));

  useEffect(() => {
    if (open) {
      setDescription(agency.targetAudience.description);
      setAgeRange(agency.targetAudience.ageRange || "");
      setInterests(agency.targetAudience.interests.join(", "));
      setPainPoints(agency.targetAudience.painPoints.join(", "));
    }
  }, [open, agency]);

  const handleSubmit = async () => {
    setSaving(true);
    try {
      await agenciesApi.updateTargetAudience(agency.id, {
        description,
        ageRange: ageRange || undefined,
        interests: interests.split(",").map((i) => i.trim()).filter(Boolean),
        painPoints: painPoints.split(",").map((p) => p.trim()).filter(Boolean),
        personas: agency.targetAudience.personas,
      });
      toast.success("Target audience aggiornato");
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
          <DialogTitle>Modifica Target Audience</DialogTitle>
        </DialogHeader>
        <div className="space-y-4">
          <div className="space-y-2">
            <Label>Descrizione</Label>
            <Textarea value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Descrivi il tuo target audience" />
          </div>
          <div className="space-y-2">
            <Label>Fascia di eta</Label>
            <Input value={ageRange} onChange={(e) => setAgeRange(e.target.value)} placeholder="es. 25-45" />
          </div>
          <div className="space-y-2">
            <Label>Interessi (separati da virgola)</Label>
            <Input value={interests} onChange={(e) => setInterests(e.target.value)} placeholder="interesse1, interesse2" />
          </div>
          <div className="space-y-2">
            <Label>Pain Points (separati da virgola)</Label>
            <Input value={painPoints} onChange={(e) => setPainPoints(e.target.value)} placeholder="problema1, problema2" />
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
