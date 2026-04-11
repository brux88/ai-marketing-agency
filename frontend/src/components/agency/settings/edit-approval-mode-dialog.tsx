"use client";

import { useState, useEffect } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Slider } from "@/components/ui/slider";
import { Loader2 } from "lucide-react";
import { toast } from "sonner";
import { agenciesApi } from "@/lib/api/agencies.api";
import type { Agency } from "@/types/api";

interface EditApprovalModeDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  agency: Agency;
  onSaved: () => void;
}

const approvalModes = [
  { value: 1, label: "Manuale" },
  { value: 2, label: "Automatica" },
  { value: 3, label: "Ibrida (auto sopra punteggio)" },
];

export function EditApprovalModeDialog({ open, onOpenChange, agency, onSaved }: EditApprovalModeDialogProps) {
  const [saving, setSaving] = useState(false);
  const [approvalMode, setApprovalMode] = useState(agency.approvalMode as number);
  const [minScore, setMinScore] = useState(agency.autoApproveMinScore);

  useEffect(() => {
    if (open) {
      setApprovalMode(agency.approvalMode as number);
      setMinScore(agency.autoApproveMinScore);
    }
  }, [open, agency]);

  const handleSubmit = async () => {
    setSaving(true);
    try {
      await agenciesApi.updateApprovalMode(agency.id, approvalMode, minScore);
      toast.success("Modalita approvazione aggiornata");
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
          <DialogTitle>Modalita Approvazione</DialogTitle>
        </DialogHeader>
        <div className="space-y-4">
          <div className="space-y-2">
            <Label>Modalita</Label>
            <select
              value={approvalMode}
              onChange={(e) => setApprovalMode(Number(e.target.value))}
              className="flex h-9 w-full rounded-lg border border-input bg-transparent px-3 py-1 text-sm shadow-xs transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
            >
              {approvalModes.map((m) => (
                <option key={m.value} value={m.value}>{m.label}</option>
              ))}
            </select>
          </div>
          {approvalMode === 3 && (
            <div className="space-y-3">
              <Label>Punteggio minimo per auto-approvazione: {minScore}/10</Label>
              <Slider
                value={[minScore]}
                onValueChange={(val) => {
                  const v = Array.isArray(val) ? val[0] : val;
                  setMinScore(v);
                }}
                min={1}
                max={10}
              />
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
