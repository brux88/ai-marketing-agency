"use client";

import { useState } from "react";
import { useAuth } from "@/lib/providers/auth-provider";
import { apiClient } from "@/lib/api/client";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";
import { User, Lock, Loader2, CheckCircle2, Trash2, AlertTriangle } from "lucide-react";
import { authApi } from "@/lib/api/auth.api";
import type { ApiResponse } from "@/types/api";

export default function ProfilePage() {
  const { user } = useAuth();
  const [fullName, setFullName] = useState(user?.fullName ?? "");
  const [profileLoading, setProfileLoading] = useState(false);
  const [profileMsg, setProfileMsg] = useState("");
  const [profileError, setProfileError] = useState("");

  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [pwLoading, setPwLoading] = useState(false);
  const [pwMsg, setPwMsg] = useState("");
  const [pwError, setPwError] = useState("");

  const [deleteLoading, setDeleteLoading] = useState(false);
  const [deleteMsg, setDeleteMsg] = useState("");
  const [deleteError, setDeleteError] = useState("");
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

  const handleProfileUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    setProfileError("");
    setProfileMsg("");
    setProfileLoading(true);
    try {
      await apiClient.put<ApiResponse<string>>("/api/v1/auth/profile", { fullName });
      setProfileMsg("Profilo aggiornato con successo.");
      const userData = localStorage.getItem("user");
      if (userData) {
        const parsed = JSON.parse(userData);
        parsed.fullName = fullName;
        localStorage.setItem("user", JSON.stringify(parsed));
      }
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error;
      setProfileError(msg || "Errore nell'aggiornamento del profilo.");
    } finally {
      setProfileLoading(false);
    }
  };

  const handlePasswordChange = async (e: React.FormEvent) => {
    e.preventDefault();
    setPwError("");
    setPwMsg("");

    if (newPassword.length < 6) {
      setPwError("La nuova password deve essere di almeno 6 caratteri.");
      return;
    }
    if (newPassword !== confirmPassword) {
      setPwError("Le password non corrispondono.");
      return;
    }

    setPwLoading(true);
    try {
      await apiClient.post<ApiResponse<string>>("/api/v1/auth/change-password", {
        currentPassword,
        newPassword,
      });
      setPwMsg("Password aggiornata con successo.");
      setCurrentPassword("");
      setNewPassword("");
      setConfirmPassword("");
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error;
      setPwError(msg || "Errore nel cambio password.");
    } finally {
      setPwLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Profilo</h1>
        <p className="text-muted-foreground mt-1">Gestisci le informazioni del tuo account</p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <User className="size-5" />
            Informazioni personali
          </CardTitle>
          <CardDescription>Aggiorna il tuo nome e le informazioni del profilo</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleProfileUpdate} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="email">Email</Label>
              <Input id="email" value={user?.email ?? ""} disabled className="bg-muted" />
              <p className="text-xs text-muted-foreground">L&apos;email non può essere modificata.</p>
            </div>
            <div className="space-y-2">
              <Label htmlFor="fullName">Nome completo</Label>
              <Input
                id="fullName"
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                required
              />
            </div>
            {profileError && (
              <div className="bg-destructive/10 text-destructive text-sm p-3 rounded-lg">{profileError}</div>
            )}
            {profileMsg && (
              <div className="bg-green-500/10 text-green-600 text-sm p-3 rounded-lg flex items-center gap-2">
                <CheckCircle2 className="size-4" /> {profileMsg}
              </div>
            )}
            <Button type="submit" disabled={profileLoading}>
              {profileLoading ? <Loader2 className="size-4 animate-spin" /> : null}
              Salva modifiche
            </Button>
          </form>
        </CardContent>
      </Card>

      <Separator />

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Lock className="size-5" />
            Cambia password
          </CardTitle>
          <CardDescription>Aggiorna la password del tuo account</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handlePasswordChange} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="currentPassword">Password attuale</Label>
              <Input
                id="currentPassword"
                type="password"
                required
                value={currentPassword}
                onChange={(e) => setCurrentPassword(e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="newPassword">Nuova password</Label>
              <Input
                id="newPassword"
                type="password"
                required
                minLength={6}
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                placeholder="Minimo 6 caratteri"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="confirmNewPassword">Conferma nuova password</Label>
              <Input
                id="confirmNewPassword"
                type="password"
                required
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
              />
            </div>
            {pwError && (
              <div className="bg-destructive/10 text-destructive text-sm p-3 rounded-lg">{pwError}</div>
            )}
            {pwMsg && (
              <div className="bg-green-500/10 text-green-600 text-sm p-3 rounded-lg flex items-center gap-2">
                <CheckCircle2 className="size-4" /> {pwMsg}
              </div>
            )}
            <Button type="submit" disabled={pwLoading}>
              {pwLoading ? <Loader2 className="size-4 animate-spin" /> : null}
              Aggiorna password
            </Button>
          </form>
        </CardContent>
      </Card>

      <Separator />

      <Card className="border-destructive/30">
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-destructive">
            <Trash2 className="size-5" />
            Elimina account
          </CardTitle>
          <CardDescription>Elimina definitivamente il tuo account e tutti i dati associati</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-start gap-3 p-4 bg-destructive/5 rounded-lg border border-destructive/20">
            <AlertTriangle className="size-5 text-destructive shrink-0 mt-0.5" />
            <div className="text-sm text-muted-foreground">
              <p className="font-medium text-destructive">Questa azione è irreversibile.</p>
              <p className="mt-1">Tutti i tuoi dati, agenzie, progetti, contenuti e connessioni social verranno eliminati definitivamente.</p>
            </div>
          </div>
          {deleteError && (
            <div className="bg-destructive/10 text-destructive text-sm p-3 rounded-lg">{deleteError}</div>
          )}
          {deleteMsg && (
            <div className="bg-green-500/10 text-green-600 text-sm p-3 rounded-lg flex items-center gap-2">
              <CheckCircle2 className="size-4" /> {deleteMsg}
            </div>
          )}
          {!showDeleteConfirm ? (
            <Button variant="destructive" onClick={() => setShowDeleteConfirm(true)}>
              <Trash2 className="size-4" />
              Elimina il mio account
            </Button>
          ) : (
            <div className="space-y-3">
              <p className="text-sm font-medium">Sei sicuro? Riceverai un&apos;email di conferma.</p>
              <div className="flex gap-2">
                <Button
                  variant="destructive"
                  disabled={deleteLoading}
                  onClick={async () => {
                    setDeleteError("");
                    setDeleteMsg("");
                    setDeleteLoading(true);
                    try {
                      await authApi.requestAccountDeletion();
                      setDeleteMsg("Ti abbiamo inviato un'email di conferma. Controlla la tua casella di posta.");
                      setShowDeleteConfirm(false);
                    } catch (err: unknown) {
                      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error;
                      setDeleteError(msg || "Errore nella richiesta di eliminazione.");
                    } finally {
                      setDeleteLoading(false);
                    }
                  }}
                >
                  {deleteLoading ? <Loader2 className="size-4 animate-spin" /> : null}
                  Conferma eliminazione
                </Button>
                <Button variant="outline" onClick={() => setShowDeleteConfirm(false)}>
                  Annulla
                </Button>
              </div>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
