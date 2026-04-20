"use client";

import { useState, useEffect } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Loader2 } from "lucide-react";
import { toast } from "sonner";
import { agenciesApi } from "@/lib/api/agencies.api";
import type { Agency } from "@/types/api";

interface EditNotificationSettingsDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  agency: Agency;
  onSaved: () => void;
}

export function EditNotificationSettingsDialog({ open, onOpenChange, agency, onSaved }: EditNotificationSettingsDialogProps) {
  const [saving, setSaving] = useState(false);
  const [notificationEmail, setNotificationEmail] = useState(agency.notificationEmail ?? "");
  const [telegramEnabled, setTelegramEnabled] = useState(agency.telegramNotificationsEnabled ?? true);
  const [emailOnSubscribed, setEmailOnSubscribed] = useState(agency.notifyEmailOnSubscribed ?? true);
  const [pushOnSubscribed, setPushOnSubscribed] = useState(agency.notifyPushOnSubscribed ?? true);
  const [telegramOnSubscribed, setTelegramOnSubscribed] = useState(agency.notifyTelegramOnSubscribed ?? true);

  useEffect(() => {
    if (open) {
      setNotificationEmail(agency.notificationEmail ?? "");
      setTelegramEnabled(agency.telegramNotificationsEnabled ?? true);
      setEmailOnSubscribed(agency.notifyEmailOnSubscribed ?? true);
      setPushOnSubscribed(agency.notifyPushOnSubscribed ?? true);
      setTelegramOnSubscribed(agency.notifyTelegramOnSubscribed ?? true);
    }
  }, [open, agency]);

  const handleSubmit = async () => {
    setSaving(true);
    try {
      await agenciesApi.updateNotificationSettings(agency.id, {
        notificationEmail: notificationEmail.trim() || null,
        telegramNotificationsEnabled: telegramEnabled,
        notifyEmailOnSubscribed: emailOnSubscribed,
        notifyPushOnSubscribed: pushOnSubscribed,
        notifyTelegramOnSubscribed: telegramOnSubscribed,
      });
      toast.success("Impostazioni notifiche aggiornate");
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
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Notifiche agenzia</DialogTitle>
        </DialogHeader>
        <div className="space-y-5">
          <div className="space-y-2">
            <Label>Email notifiche agenzia</Label>
            <Input
              type="email"
              placeholder="admin@esempio.com"
              value={notificationEmail}
              onChange={(e) => setNotificationEmail(e.target.value)}
            />
            <p className="text-xs text-muted-foreground">
              Destinatario delle notifiche a livello agenzia. Lascia vuoto per non ricevere email.
            </p>
          </div>
          <div className="flex items-center justify-between rounded-md border p-3">
            <div className="space-y-0.5 pr-3">
              <Label className="text-sm">Notifiche Telegram abilitate</Label>
              <p className="text-xs text-muted-foreground">
                Interruttore generale per tutte le notifiche Telegram dell&apos;agenzia.
              </p>
            </div>
            <Switch checked={telegramEnabled} onCheckedChange={setTelegramEnabled} />
          </div>
          <div className="space-y-2 pt-2 border-t">
            <div className="text-xs font-medium uppercase tracking-wide text-muted-foreground">Iscrizione newsletter</div>
            <p className="text-xs text-muted-foreground">
              Notifiche da inviare quando qualcuno si iscrive a una newsletter dell&apos;agenzia.
            </p>
            <div className="flex items-center justify-between">
              <Label className="cursor-pointer">Email</Label>
              <Switch checked={emailOnSubscribed} onCheckedChange={setEmailOnSubscribed} />
            </div>
            <div className="flex items-center justify-between">
              <Label className="cursor-pointer">Push mobile</Label>
              <Switch checked={pushOnSubscribed} onCheckedChange={setPushOnSubscribed} />
            </div>
            <div className="flex items-center justify-between">
              <Label className="cursor-pointer">Telegram</Label>
              <Switch checked={telegramOnSubscribed} onCheckedChange={setTelegramOnSubscribed} />
            </div>
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
