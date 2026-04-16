"use client";

import { useParams } from "next/navigation";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import { projectsApi } from "@/lib/api/projects.api";
import type { ApiResponse } from "@/types/api";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import { Send, Plus, Trash2, Loader2, MessageCircle, Bell, CheckCircle2, Zap, Bot, Copy, KeyRound, Webhook } from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";

interface TelegramConnection {
  id: string;
  projectId?: string | null;
  projectName?: string | null;
  chatId: number;
  chatTitle?: string;
  username?: string;
  notifyOnContentGenerated: boolean;
  notifyOnApprovalNeeded: boolean;
  notifyOnPublished: boolean;
  allowCommands: boolean;
  isActive: boolean;
}

interface TelegramBotInfo {
  hasToken: boolean;
  botUsername?: string | null;
  webhookUrl: string;
}

export default function TelegramPage() {
  const { agencyId } = useParams<{ agencyId: string }>();
  const queryClient = useQueryClient();
  const [showForm, setShowForm] = useState(false);
  const [chatId, setChatId] = useState("");
  const [chatTitle, setChatTitle] = useState("");
  const [username, setUsername] = useState("");
  const [formProjectId, setFormProjectId] = useState("");
  const [botToken, setBotToken] = useState("");
  const [botUsernameInput, setBotUsernameInput] = useState("");

  const { data, isLoading } = useQuery({
    queryKey: ["telegram", agencyId],
    queryFn: () => apiClient.get<ApiResponse<TelegramConnection[]>>(`/api/v1/agencies/${agencyId}/telegram`),
  });

  const { data: projects } = useQuery({
    queryKey: ["projects", agencyId],
    queryFn: () => projectsApi.list(agencyId),
  });

  const { data: botInfoRes } = useQuery({
    queryKey: ["telegram-bot", agencyId],
    queryFn: () => apiClient.get<ApiResponse<TelegramBotInfo>>(`/api/v1/agencies/${agencyId}/telegram/bot`),
  });

  const botInfo = botInfoRes?.data;

  const saveBotMutation = useMutation({
    mutationFn: () =>
      apiClient.put<ApiResponse<TelegramBotInfo>>(`/api/v1/agencies/${agencyId}/telegram/bot`, {
        token: botToken || null,
        botUsername: botUsernameInput || null,
      }),
    onSuccess: () => {
      toast.success("Bot salvato");
      setBotToken("");
      queryClient.invalidateQueries({ queryKey: ["telegram-bot", agencyId] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  const registerWebhookMutation = useMutation({
    mutationFn: () =>
      apiClient.post<ApiResponse<{ ok: boolean; description?: string; botUsername?: string; webhookUrl: string }>>(
        `/api/v1/agencies/${agencyId}/telegram/register-webhook`
      ),
    onSuccess: (res) => {
      if (res.data?.ok) {
        toast.success(`Webhook registrato${res.data.botUsername ? ` su @${res.data.botUsername}` : ""}`);
      } else {
        toast.error(res.data?.description || "Registrazione webhook fallita");
      }
      queryClient.invalidateQueries({ queryKey: ["telegram-bot", agencyId] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  const clearBotMutation = useMutation({
    mutationFn: () => apiClient.delete(`/api/v1/agencies/${agencyId}/telegram/bot`),
    onSuccess: () => {
      toast.success("Bot rimosso");
      setBotUsernameInput("");
      queryClient.invalidateQueries({ queryKey: ["telegram-bot", agencyId] });
    },
  });

  const connectMutation = useMutation({
    mutationFn: () =>
      apiClient.post(`/api/v1/agencies/${agencyId}/telegram/connect`, {
        chatId: parseInt(chatId),
        chatTitle: chatTitle || null,
        username: username || null,
        projectId: formProjectId || null,
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
      setFormProjectId("");
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
  const defaultConnections = connections.filter((c) => !c.projectId);
  const projectGroups = connections.reduce<Record<string, { name: string; items: TelegramConnection[] }>>((acc, c) => {
    if (!c.projectId) return acc;
    if (!acc[c.projectId]) acc[c.projectId] = { name: c.projectName || "Progetto", items: [] };
    acc[c.projectId].items.push(c);
    return acc;
  }, {});

  const renderConnectionCard = (conn: TelegramConnection) => (
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
  );

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

      {/* Bot credentials (white-label) */}
      <Card>
        <CardContent className="pt-6 space-y-4">
          <div className="flex items-start gap-3">
            <div className="size-10 rounded-lg bg-violet-500/10 flex items-center justify-center shrink-0">
              <Bot className="size-5 text-violet-500" />
            </div>
            <div className="flex-1">
              <h3 className="font-semibold">Bot dell'agency</h3>
              <p className="text-sm text-muted-foreground">
                Default per tutta l'agenzia. I progetti possono avere un bot dedicato (configurabile dalla pagina del progetto).
              </p>
            </div>
            {botInfo?.hasToken && (
              <Badge variant="default" className="gap-1">
                <CheckCircle2 className="size-3" /> Configurato
              </Badge>
            )}
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Bot token {botInfo?.hasToken && <span className="text-xs text-muted-foreground">(lascia vuoto per mantenere)</span>}</Label>
              <Input
                type="password"
                placeholder="123456:ABC-DEF..."
                value={botToken}
                onChange={(e) => setBotToken(e.target.value)}
              />
            </div>
            <div className="space-y-1.5">
              <Label>Bot username</Label>
              <Input
                placeholder="mio_marketing_bot"
                value={botUsernameInput || botInfo?.botUsername || ""}
                onChange={(e) => setBotUsernameInput(e.target.value)}
              />
            </div>
          </div>

          {botInfo?.webhookUrl && (
            <div className="space-y-1.5">
              <Label className="text-xs flex items-center gap-1">
                <KeyRound className="size-3" /> Webhook URL
              </Label>
              <div className="flex gap-2">
                <Input readOnly value={botInfo.webhookUrl} className="font-mono text-xs" />
                <Button
                  type="button"
                  variant="outline"
                  size="icon"
                  onClick={() => {
                    navigator.clipboard.writeText(botInfo.webhookUrl);
                    toast.success("Copiato!");
                  }}
                >
                  <Copy className="size-4" />
                </Button>
              </div>
              <p className="text-xs text-muted-foreground">
                Clicca "Registra webhook" per configurare automaticamente il bot (richiama Telegram setWebhook).
              </p>
            </div>
          )}

          <div className="flex gap-2">
            <Button
              onClick={() => saveBotMutation.mutate()}
              disabled={saveBotMutation.isPending || (!botToken && !botUsernameInput)}
            >
              {saveBotMutation.isPending ? <Loader2 className="size-4 animate-spin" /> : <Bot className="size-4" />}
              Salva bot
            </Button>
            {botInfo?.hasToken && (
              <Button
                variant="default"
                onClick={() => registerWebhookMutation.mutate()}
                disabled={registerWebhookMutation.isPending}
              >
                {registerWebhookMutation.isPending ? <Loader2 className="size-4 animate-spin" /> : <Webhook className="size-4" />}
                Registra webhook
              </Button>
            )}
            {botInfo?.hasToken && (
              <Button
                variant="outline"
                onClick={() => {
                  if (confirm("Rimuovere il bot dell'agency?")) clearBotMutation.mutate();
                }}
                disabled={clearBotMutation.isPending}
              >
                <Trash2 className="size-4" /> Rimuovi
              </Button>
            )}
          </div>
        </CardContent>
      </Card>

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
              Seleziona l'ambito (agenzia o progetto), cerca il bot {botInfo?.botUsername ? <code>@{botInfo.botUsername}</code> : <code>@{"<bot-username>"}</code>} su Telegram e incolla il Chat ID qui sotto.
            </p>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
              <div className="space-y-1.5">
                <Label>Ambito</Label>
                <select
                  value={formProjectId}
                  onChange={(e) => setFormProjectId(e.target.value)}
                  className="flex h-9 w-full rounded-lg border border-input bg-transparent px-3 py-1 text-sm"
                >
                  <option value="">Tutta l'agenzia (default)</option>
                  {projects?.map((p) => (
                    <option key={p.id} value={p.id}>{p.name}</option>
                  ))}
                </select>
              </div>
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
        <div className="space-y-6">
          {defaultConnections.length > 0 && (
            <div className="space-y-3">
              <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wider">Default agenzia</h3>
              {defaultConnections.map(renderConnectionCard)}
            </div>
          )}
          {Object.entries(projectGroups).map(([pid, group]) => (
            <div key={pid} className="space-y-3">
              <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wider">Progetto: {group.name}</h3>
              {group.items.map(renderConnectionCard)}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
