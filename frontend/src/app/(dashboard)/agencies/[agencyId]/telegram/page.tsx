"use client";

import { useParams } from "next/navigation";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import type { ApiResponse } from "@/types/api";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Skeleton } from "@/components/ui/skeleton";
import { Send, Plus, Trash2, Loader2, MessageCircle, Bell, CheckCircle2, Zap } from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";

interface TelegramConnection {
  id: string;
  chatId: number;
  chatTitle?: string;
  username?: string;
  notifyOnContentGenerated: boolean;
  notifyOnApprovalNeeded: boolean;
  notifyOnPublished: boolean;
  allowCommands: boolean;
  isActive: boolean;
}

export default function TelegramPage() {
  const { agencyId } = useParams();
  const queryClient = useQueryClient();
  const [showForm, setShowForm] = useState(false);
  const [chatId, setChatId] = useState("");
  const [chatTitle, setChatTitle] = useState("");
  const [username, setUsername] = useState("");

  const { data, isLoading } = useQuery({
    queryKey: ["telegram", agencyId],
    queryFn: () => apiClient.get<ApiResponse<TelegramConnection[]>>(`/api/v1/agencies/${agencyId}/telegram`),
  });

  const connectMutation = useMutation({
    mutationFn: () =>
      apiClient.post(`/api/v1/agencies/${agencyId}/telegram/connect`, {
        chatId: parseInt(chatId),
        chatTitle: chatTitle || null,
        username: username || null,
        notifyOnContentGenerated: true,
        notifyOnApprovalNeeded: true,
        notifyOnPublished: true,
        allowCommands: true,
      }),
    onSuccess: () => {
      toast.success("Telegram connesso!");
      setChatId("");
      setChatTitle("");
      setUsername("");
      setShowForm(false);
      queryClient.invalidateQueries({ queryKey: ["telegram", agencyId] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  const disconnectMutation = useMutation({
    mutationFn: (id: string) =>
      apiClient.delete(`/api/v1/agencies/${agencyId}/telegram/${id}`),
    onSuccess: () => {
      toast.success("Connessione rimossa");
      queryClient.invalidateQueries({ queryKey: ["telegram", agencyId] });
    },
  });

  const testMutation = useMutation({
    mutationFn: () =>
      apiClient.post(`/api/v1/agencies/${agencyId}/telegram/test`),
    onSuccess: () => toast.success("Messaggio di test inviato!"),
    onError: (err: Error) => toast.error(err.message),
  });

  const connections = data?.data || [];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold tracking-tight">Telegram</h2>
          <p className="text-muted-foreground text-sm mt-1">
            Ricevi notifiche e gestisci contenuti direttamente da Telegram
          </p>
        </div>
        <div className="flex gap-2">
          {connections.length > 0 && (
            <Button variant="outline" onClick={() => testMutation.mutate()} disabled={testMutation.isPending}>
              {testMutation.isPending ? <Loader2 className="size-4 animate-spin" /> : <Zap className="size-4" />}
              Test
            </Button>
          )}
          <Button onClick={() => setShowForm(!showForm)}>
            <Plus className="size-4" /> Connetti
          </Button>
        </div>
      </div>

      {/* How it works */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Card>
          <CardContent className="pt-6 text-center">
            <Bell className="size-8 mx-auto mb-2 text-blue-500" />
            <h4 className="font-medium text-sm">Notifiche</h4>
            <p className="text-xs text-muted-foreground mt-1">
              Ricevi avvisi quando nuovi contenuti vengono generati
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6 text-center">
            <CheckCircle2 className="size-8 mx-auto mb-2 text-green-500" />
            <h4 className="font-medium text-sm">Approvazione</h4>
            <p className="text-xs text-muted-foreground mt-1">
              Approva o rifiuta contenuti con i comandi /approve e /reject
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6 text-center">
            <MessageCircle className="size-8 mx-auto mb-2 text-purple-500" />
            <h4 className="font-medium text-sm">Comandi</h4>
            <p className="text-xs text-muted-foreground mt-1">
              Usa /status per vedere lo stato dell&apos;agenzia in tempo reale
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Connect form */}
      {showForm && (
        <Card>
          <CardContent className="pt-6 space-y-4">
            <h3 className="font-semibold">Nuova connessione Telegram</h3>
            <p className="text-sm text-muted-foreground">
              1. Cerca il bot <code>@AiMarketingAgencyBot</code> su Telegram<br/>
              2. Avvia una chat e invia /start<br/>
              3. Il bot ti dara il Chat ID da inserire qui
            </p>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
              <div className="space-y-1.5">
                <Label>Chat ID *</Label>
                <Input
                  type="number"
                  placeholder="-1001234567890"
                  value={chatId}
                  onChange={(e) => setChatId(e.target.value)}
                />
              </div>
              <div className="space-y-1.5">
                <Label>Nome chat</Label>
                <Input
                  placeholder="Marketing Team"
                  value={chatTitle}
                  onChange={(e) => setChatTitle(e.target.value)}
                />
              </div>
              <div className="space-y-1.5">
                <Label>Username</Label>
                <Input
                  placeholder="@username"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                />
              </div>
            </div>
            <div className="flex gap-2">
              <Button onClick={() => connectMutation.mutate()} disabled={!chatId || connectMutation.isPending}>
                {connectMutation.isPending ? <Loader2 className="size-4 animate-spin" /> : <Send className="size-4" />}
                Connetti
              </Button>
              <Button variant="outline" onClick={() => setShowForm(false)}>Annulla</Button>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Connections list */}
      {isLoading ? (
        <div className="space-y-3">
          {[1, 2].map((i) => <Skeleton key={i} className="h-20" />)}
        </div>
      ) : connections.length === 0 ? (
        <Card className="text-center py-16">
          <CardContent className="space-y-4">
            <div className="size-16 rounded-full bg-primary/10 flex items-center justify-center mx-auto">
              <Send className="size-8 text-primary" />
            </div>
            <h3 className="text-lg font-semibold">Nessuna connessione Telegram</h3>
            <p className="text-muted-foreground max-w-sm mx-auto">
              Connetti un gruppo o chat Telegram per ricevere notifiche e gestire i contenuti
            </p>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-3">
          {connections.map((conn) => (
            <Card key={conn.id}>
              <CardContent className="pt-6">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <div className="size-10 rounded-full bg-blue-500/10 flex items-center justify-center">
                      <Send className="size-5 text-blue-500" />
                    </div>
                    <div>
                      <p className="font-medium">{conn.chatTitle || `Chat ${conn.chatId}`}</p>
                      <p className="text-xs text-muted-foreground">
                        {conn.username ? `@${conn.username}` : `ID: ${conn.chatId}`}
                      </p>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <Badge variant={conn.isActive ? "default" : "secondary"}>
                      {conn.isActive ? "Attivo" : "Disattivo"}
                    </Badge>
                    <Button
                      variant="ghost"
                      size="icon-sm"
                      onClick={() => disconnectMutation.mutate(conn.id)}
                      className="text-destructive"
                    >
                      <Trash2 className="size-4" />
                    </Button>
                  </div>
                </div>
                <div className="flex gap-4 mt-3 pt-3 border-t text-xs text-muted-foreground">
                  <span>Contenuti: {conn.notifyOnContentGenerated ? "Si" : "No"}</span>
                  <span>Approvazioni: {conn.notifyOnApprovalNeeded ? "Si" : "No"}</span>
                  <span>Pubblicazioni: {conn.notifyOnPublished ? "Si" : "No"}</span>
                  <span>Comandi: {conn.allowCommands ? "Si" : "No"}</span>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
