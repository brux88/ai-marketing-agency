"use client";

import { useParams } from "next/navigation";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { projectsApi } from "@/lib/api/projects.api";
import { agenciesApi } from "@/lib/api/agencies.api";
import { schedulesApi } from "@/lib/api/schedules.api";
import { notificationsApi, type NotificationItem } from "@/lib/api/notifications.api";
import { jobsApi, type JobListItem } from "@/lib/api/jobs.api";
import { apiClient } from "@/lib/api/client";
import { resolveImageUrl } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { PenLine, Share2, Mail, BarChart3, Loader2, FileText, Rss, Globe, FolderKanban, Calendar, Images, ImageIcon, Sparkles, Bot, Trash2, Webhook, Plus, X, Pause, Play, Pencil, Zap, Bell, CheckCheck } from "lucide-react";
import { useEffect, useMemo, useRef, useState } from "react";
import { toast } from "sonner";
import { GenerateContentDialog } from "@/components/agency/generate-content-dialog";
import { ContentEditDialog } from "@/components/agency/content-edit-dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Switch } from "@/components/ui/switch";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { useMutation } from "@tanstack/react-query";
import type { ApiResponse, GeneratedContent, ContentSchedule } from "@/types/api";
import { ApprovalMode, ContentStatus, DayOfWeekFlag } from "@/types/api";

interface TelegramBotInfo {
  hasToken: boolean;
  botUsername?: string | null;
  webhookUrl: string;
}

function ProjectEmailNotificationsCard({
  agencyId,
  projectId,
  project,
}: {
  agencyId: string;
  projectId: string;
  project: any;
}) {
  const qc = useQueryClient();
  const [email, setEmail] = useState(project?.notificationEmail ?? "");
  const [onGeneration, setOnGeneration] = useState(project?.notifyEmailOnGeneration ?? false);
  const [onPublication, setOnPublication] = useState(project?.notifyEmailOnPublication ?? false);
  const [onApproval, setOnApproval] = useState(project?.notifyEmailOnApprovalNeeded ?? false);

  useEffect(() => {
    setEmail(project?.notificationEmail ?? "");
    setOnGeneration(project?.notifyEmailOnGeneration ?? false);
    setOnPublication(project?.notifyEmailOnPublication ?? false);
    setOnApproval(project?.notifyEmailOnApprovalNeeded ?? false);
  }, [project]);

  const save = useMutation({
    mutationFn: () =>
      apiClient.put(`/api/v1/agencies/${agencyId}/projects/${projectId}/email-notifications`, {
        notifyEmailOnGeneration: onGeneration,
        notifyEmailOnPublication: onPublication,
        notifyEmailOnApprovalNeeded: onApproval,
        notificationEmail: email || null,
      }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["project", agencyId, projectId] });
      toast.success("Impostazioni email salvate");
    },
    onError: () => toast.error("Errore nel salvataggio"),
  });

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base flex items-center gap-2">
          <Mail className="size-4" />
          Notifiche Email
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-2">
          <Label>Email destinatario</Label>
          <Input
            type="email"
            placeholder="nome@esempio.com"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
          <p className="text-xs text-muted-foreground">
            Lascia vuoto per non ricevere notifiche email
          </p>
        </div>
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <Label className="cursor-pointer">Generazione contenuti</Label>
            <Switch checked={onGeneration} onCheckedChange={setOnGeneration} />
          </div>
          <div className="flex items-center justify-between">
            <Label className="cursor-pointer">Pubblicazione</Label>
            <Switch checked={onPublication} onCheckedChange={setOnPublication} />
          </div>
          <div className="flex items-center justify-between">
            <Label className="cursor-pointer">Contenuti da approvare</Label>
            <Switch checked={onApproval} onCheckedChange={setOnApproval} />
          </div>
        </div>
        <Button size="sm" onClick={() => save.mutate()} disabled={save.isPending}>
          {save.isPending && <Loader2 className="size-4 animate-spin mr-1" />}
          Salva
        </Button>
      </CardContent>
    </Card>
  );
}

function ProjectDeleteCard({ agencyId, projectId }: { agencyId: string; projectId: string }) {
  const [password, setPassword] = useState("");
  const [showConfirm, setShowConfirm] = useState(false);

  const deleteMut = useMutation({
    mutationFn: () => projectsApi.delete(agencyId, projectId, password),
    onSuccess: () => {
      toast.success("Progetto eliminato");
      window.location.href = `/agencies/${agencyId}`;
    },
    onError: (e: any) => toast.error(e?.response?.data?.message || e?.message || "Password errata o errore"),
  });

  return (
    <Card className="border-destructive/50">
      <CardHeader>
        <CardTitle className="text-base flex items-center gap-2 text-destructive">
          <Trash2 className="size-4" />
          Elimina progetto
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-3">
        <p className="text-sm text-muted-foreground">
          Questa azione e irreversibile. Tutti i contenuti, il calendario e le configurazioni verranno eliminati.
        </p>
        {!showConfirm ? (
          <Button variant="destructive" size="sm" onClick={() => setShowConfirm(true)}>
            Elimina progetto
          </Button>
        ) : (
          <div className="space-y-2">
            <Label>Password admin</Label>
            <Input
              type="password"
              placeholder="Inserisci la password admin"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
            <div className="flex gap-2">
              <Button
                variant="destructive"
                size="sm"
                onClick={() => deleteMut.mutate()}
                disabled={!password || deleteMut.isPending}
              >
                {deleteMut.isPending && <Loader2 className="size-4 animate-spin mr-1" />}
                Conferma eliminazione
              </Button>
              <Button variant="outline" size="sm" onClick={() => { setShowConfirm(false); setPassword(""); }}>
                Annulla
              </Button>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
}

function ProjectTelegramBot({ agencyId, projectId }: { agencyId: string; projectId: string }) {
  const queryClient = useQueryClient();
  const [token, setToken] = useState("");
  const [botUsername, setBotUsername] = useState("");

  const { data: res } = useQuery({
    queryKey: ["project-telegram-bot", agencyId, projectId],
    queryFn: () =>
      apiClient.get<ApiResponse<TelegramBotInfo>>(
        `/api/v1/agencies/${agencyId}/telegram/project/${projectId}/bot`
      ),
  });
  const info = res?.data;

  const saveMutation = useMutation({
    mutationFn: () =>
      apiClient.put<ApiResponse<TelegramBotInfo>>(
        `/api/v1/agencies/${agencyId}/telegram/project/${projectId}/bot`,
        { token: token || null, botUsername: botUsername || null }
      ),
    onSuccess: () => {
      toast.success("Bot progetto salvato");
      setToken("");
      queryClient.invalidateQueries({ queryKey: ["project-telegram-bot", agencyId, projectId] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  const clearMutation = useMutation({
    mutationFn: () =>
      apiClient.delete(`/api/v1/agencies/${agencyId}/telegram/project/${projectId}/bot`),
    onSuccess: () => {
      toast.success("Bot progetto rimosso");
      setBotUsername("");
      queryClient.invalidateQueries({ queryKey: ["project-telegram-bot", agencyId, projectId] });
    },
  });

  const registerMutation = useMutation({
    mutationFn: () =>
      apiClient.post<ApiResponse<{ ok: boolean; description?: string; botUsername?: string }>>(
        `/api/v1/agencies/${agencyId}/telegram/project/${projectId}/register-webhook`
      ),
    onSuccess: (res) => {
      if (res.data?.ok) {
        toast.success(`Webhook registrato${res.data.botUsername ? ` su @${res.data.botUsername}` : ""}`);
      } else {
        toast.error(res.data?.description || "Registrazione fallita");
      }
      queryClient.invalidateQueries({ queryKey: ["project-telegram-bot", agencyId, projectId] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <div className="flex items-center gap-2">
          <Bot className="size-4 text-violet-500" />
          <CardTitle className="text-base">Bot Telegram del progetto (opzionale)</CardTitle>
        </div>
        {info?.hasToken && <Badge variant="default">Override attivo</Badge>}
      </CardHeader>
      <CardContent className="space-y-3">
        <p className="text-xs text-muted-foreground">
          Se impostato, questo bot sostituisce quello dell&apos;agenzia per notifiche e comandi riguardanti questo progetto.
        </p>
        <p className="text-xs text-amber-600 bg-amber-50 dark:bg-amber-950/30 p-2 rounded border border-amber-200 dark:border-amber-900">
          <strong>Nota:</strong> Telegram richiede <strong>HTTPS pubblico</strong> per il webhook. Se stai lavorando in locale, esponi
          l&apos;API con un tunnel come <code>ngrok http 5000</code> o <code>cloudflared tunnel --url http://localhost:5000</code>,
          poi imposta la variabile <code>Api__PublicBaseUrl</code> sull&apos;URL HTTPS del tunnel prima di registrare il webhook.
        </p>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
          <div className="space-y-1.5">
            <Label className="text-xs">Bot token {info?.hasToken && <span className="text-muted-foreground">(vuoto = mantieni)</span>}</Label>
            <Input
              type="password"
              placeholder="123456:ABC-DEF..."
              value={token}
              onChange={(e) => setToken(e.target.value)}
            />
          </div>
          <div className="space-y-1.5">
            <Label className="text-xs">Bot username</Label>
            <Input
              placeholder="progetto_bot"
              value={botUsername || info?.botUsername || ""}
              onChange={(e) => setBotUsername(e.target.value)}
            />
          </div>
        </div>
        <div className="flex gap-2">
          <Button
            size="sm"
            onClick={() => saveMutation.mutate()}
            disabled={saveMutation.isPending || (!token && !botUsername)}
          >
            {saveMutation.isPending ? <Loader2 className="size-4 animate-spin" /> : <Bot className="size-4" />}
            Salva
          </Button>
          {info?.hasToken && (
            <Button
              size="sm"
              variant="default"
              onClick={() => registerMutation.mutate()}
              disabled={registerMutation.isPending}
            >
              {registerMutation.isPending ? <Loader2 className="size-4 animate-spin" /> : <Webhook className="size-4" />}
              Registra webhook
            </Button>
          )}
          {info?.hasToken && (
            <Button
              size="sm"
              variant="outline"
              onClick={() => { if (confirm("Rimuovere il bot del progetto?")) clearMutation.mutate(); }}
              disabled={clearMutation.isPending}
            >
              <Trash2 className="size-4" /> Rimuovi
            </Button>
          )}
        </div>
      </CardContent>
    </Card>
  );
}

const agents = [
  { type: "content-writer", label: "Content Writer", desc: "Genera articoli blog", icon: PenLine, color: "text-blue-600 bg-blue-100 dark:bg-blue-950" },
  { type: "social-manager", label: "Social Manager", desc: "Crea post social", icon: Share2, color: "text-violet-600 bg-violet-100 dark:bg-violet-950" },
  { type: "newsletter", label: "Newsletter", desc: "Genera newsletter", icon: Mail, color: "text-emerald-600 bg-emerald-100 dark:bg-emerald-950" },
  { type: "analytics", label: "Analytics", desc: "Analizza performance", icon: BarChart3, color: "text-amber-600 bg-amber-100 dark:bg-amber-950" },
];

export default function ProjectDetailPage() {
  const { agencyId, projectId } = useParams();
  const queryClient = useQueryClient();
  const [generateFor, setGenerateFor] = useState<{ type: string; label: string } | null>(null);
  const [editingContent, setEditingContent] = useState<GeneratedContent | null>(null);
  const [brandExtracting, setBrandExtracting] = useState(false);

  const { data: keysData } = useQuery({
    queryKey: ["llm-keys"],
    queryFn: () => apiClient.get<ApiResponse<{ category: number; isActive: boolean }[]>>("/api/v1/llmkeys"),
  });
  const hasTextKeys = (keysData?.data ?? []).some((k) => k.category === 0 && k.isActive);

  const handleExtractBrand = async () => {
    if (!project?.websiteUrl) {
      toast.error("Imposta prima un URL del sito web per il progetto");
      return;
    }
    if (!hasTextKeys) {
      toast.error("Configura prima una chiave API nella sezione Chiavi API");
      return;
    }
    setBrandExtracting(true);
    try {
      await projectsApi.extractBrand(agencyId as string, projectId as string);
      toast.success("Brand voice estratto dal sito!");
      queryClient.invalidateQueries({ queryKey: ["project", agencyId, projectId] });
      queryClient.invalidateQueries({ queryKey: ["project-prompts", agencyId, projectId] });
    } catch (err: any) {
      toast.error(err?.response?.data?.message ?? "Estrazione brand fallita");
    } finally {
      setBrandExtracting(false);
    }
  };

  const { data: agencyData } = useQuery({
    queryKey: ["agency", agencyId],
    queryFn: () => agenciesApi.get(agencyId as string),
  });

  const { data: project, isLoading } = useQuery({
    queryKey: ["project", agencyId, projectId],
    queryFn: () => projectsApi.get(agencyId as string, projectId as string),
  });

  const { data: contentData, refetch: refetchContent } = useQuery({
    queryKey: ["content", agencyId, "project", projectId],
    queryFn: () =>
      apiClient.get<ApiResponse<GeneratedContent[]>>(
        `/api/v1/agencies/${agencyId}/content?projectId=${projectId}`
      ),
    refetchInterval: 10000,
  });

  const { data: unreadNotifCount } = useQuery({
    queryKey: ["notifications", "project-unread", projectId],
    queryFn: async () => {
      const res = await notificationsApi.list(true, 200);
      return (res.data?.items ?? []).filter((n: NotificationItem) => n.projectId === projectId).length;
    },
    refetchInterval: 15000,
  });

  const deleteContent = useMutation({
    mutationFn: (contentId: string) =>
      projectsApi.deleteContent(agencyId as string, contentId),
    onSuccess: () => {
      toast.success("Contenuto eliminato");
      refetchContent();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const handleDeleteContent = (c: GeneratedContent) => {
    if (confirm(`Eliminare definitivamente "${c.title}"?`)) {
      deleteContent.mutate(c.id);
    }
  };

  const agency = agencyData?.data;
  const allContents = [...(contentData?.data || [])].sort(
    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
  );
  const blogs = allContents.filter((c) => c.contentType === 1);
  const socials = allContents.filter((c) => c.contentType === 2);
  const newsletters = allContents.filter((c) => c.contentType === 3);

  const handleOpenGenerate = (agentType: string, label: string) => {
    if (!agency?.defaultLlmProviderKeyId) {
      toast.error("Configura prima una chiave LLM nelle impostazioni dell'agenzia");
      return;
    }
    setGenerateFor({ type: agentType, label });
  };

  const formatDate = (iso: string) =>
    new Date(iso).toLocaleString("it-IT", { day: "2-digit", month: "short", hour: "2-digit", minute: "2-digit" });

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-48" />
      </div>
    );
  }

  if (!project) {
    return (
      <Card className="text-center py-12">
        <CardContent>
          <p className="text-destructive font-medium">Progetto non trovato</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <div className="size-10 rounded-lg bg-primary/10 flex items-center justify-center">
          <FolderKanban className="size-5 text-primary" />
        </div>
        <div className="flex-1 min-w-0">
          <h1 className="text-2xl font-bold tracking-tight truncate">{project.name}</h1>
          {project.description && (
            <p className="text-muted-foreground mt-0.5 text-sm line-clamp-1">{project.description}</p>
          )}
        </div>
        <Badge variant={project.isActive ? "default" : "secondary"}>
          {project.isActive ? "Attivo" : "Inattivo"}
        </Badge>
      </div>

      <Tabs defaultValue="overview">
        <TabsList className="w-full h-auto flex-wrap justify-start">
          <TabsTrigger value="overview">Panoramica</TabsTrigger>
          <TabsTrigger value="content">Contenuti</TabsTrigger>
          <TabsTrigger value="calendar">Calendario</TabsTrigger>
          <TabsTrigger value="schedules">Programmazione</TabsTrigger>
          <TabsTrigger value="approvals" className="relative gap-2">
            Approvazioni
            {allContents.filter((c) => c.status === ContentStatus.InReview).length > 0 && (
              <span className="inline-flex items-center justify-center size-5 rounded-full bg-destructive text-destructive-foreground text-[10px] font-bold">
                {allContents.filter((c) => c.status === ContentStatus.InReview).length}
              </span>
            )}
          </TabsTrigger>
          <TabsTrigger value="analytics">Analytics</TabsTrigger>
          <TabsTrigger value="notifications" className="relative">
            Notifiche
            {(unreadNotifCount ?? 0) > 0 && (
              <span className="ml-1.5 inline-flex items-center justify-center size-5 rounded-full bg-destructive text-destructive-foreground text-[10px] font-bold">
                {unreadNotifCount}
              </span>
            )}
          </TabsTrigger>
          <TabsTrigger value="sources">Fonti & Prompt</TabsTrigger>
          <TabsTrigger value="settings">Impostazioni</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="mt-6 space-y-6">
          <div className="grid md:grid-cols-4 gap-4">
            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center gap-2 text-muted-foreground text-xs mb-1">
                  <Rss className="size-3.5" /> Fonti contenuto
                </div>
                <div className="text-2xl font-semibold">{project.contentSourcesCount}</div>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center gap-2 text-muted-foreground text-xs mb-1">
                  <FileText className="size-3.5" /> Contenuti generati
                </div>
                <div className="text-2xl font-semibold">{project.generatedContentsCount}</div>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center gap-2 text-muted-foreground text-xs mb-1">
                  <Globe className="size-3.5" /> Sito web
                </div>
                {project.websiteUrl ? (
                  <a href={project.websiteUrl} target="_blank" rel="noopener noreferrer" className="text-primary hover:underline text-sm truncate block">
                    {project.websiteUrl}
                  </a>
                ) : (
                  <div className="text-sm text-muted-foreground">—</div>
                )}
              </CardContent>
            </Card>
            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center gap-2 text-muted-foreground text-xs mb-1">
                  <Share2 className="size-3.5" /> Brand Voice
                </div>
                <div className="text-sm font-medium capitalize">
                  {project.brandVoice?.tone || "—"} · {project.brandVoice?.language?.toUpperCase() || "—"}
                </div>
              </CardContent>
            </Card>
          </div>

          {allContents.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle className="text-base">Ultimi contenuti</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                {allContents.slice(0, 5).map((c) => (
                  <div key={c.id} className="flex items-center justify-between gap-3 py-2 border-b last:border-0">
                    <div className="flex items-center gap-2 min-w-0 flex-1">
                      <Badge variant="outline" className="text-[10px] shrink-0">
                        {c.contentType === 1 ? "Blog" : c.contentType === 2 ? "Social" : c.contentType === 3 ? "Newsletter" : `Tipo ${c.contentType}`}
                      </Badge>
                      <span className="text-sm truncate">{c.title}</span>
                    </div>
                    <div className="flex items-center gap-2 shrink-0">
                      {c.status === 4 && <Badge className="bg-emerald-500 text-white text-[10px]">Pubblicato</Badge>}
                      {c.status === 3 && <Badge className="bg-blue-500 text-white text-[10px]">Approvato</Badge>}
                      {c.status === 2 && <Badge variant="outline" className="text-[10px]">In revisione</Badge>}
                      <span className="text-[11px] text-muted-foreground">{formatDate(c.createdAt)}</span>
                    </div>
                  </div>
                ))}
              </CardContent>
            </Card>
          )}

          <div>
            <h2 className="text-sm font-semibold tracking-tight mb-3 text-muted-foreground">Lancia un agente AI</h2>
            <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-3">
              {agents.map((agent) => (
                <Card key={agent.type}>
                  <CardContent className="pt-4">
                    <div className={`size-9 rounded-lg flex items-center justify-center mb-2 ${agent.color}`}>
                      <agent.icon className="size-4" />
                    </div>
                    <h4 className="font-semibold text-sm">{agent.label}</h4>
                    <p className="text-muted-foreground text-xs mt-0.5">{agent.desc}</p>
                    <Button
                      className="mt-3 w-full"
                      size="sm"
                      onClick={() => handleOpenGenerate(agent.type, agent.label)}
                    >
                      <agent.icon className="size-4" /> Avvia
                    </Button>
                  </CardContent>
                </Card>
              ))}
            </div>
          </div>
        </TabsContent>

        <TabsContent value="content" className="mt-6 space-y-6">
          <Tabs defaultValue="social">
            <TabsList>
              <TabsTrigger value="social">
                <Share2 className="size-4" /> Social
                <Badge variant="secondary" className="ml-1">{socials.length}</Badge>
              </TabsTrigger>
              <TabsTrigger value="blog">
                <PenLine className="size-4" /> Blog
                <Badge variant="secondary" className="ml-1">{blogs.length}</Badge>
              </TabsTrigger>
              <TabsTrigger value="newsletter">
                <Mail className="size-4" /> Newsletter
                <Badge variant="secondary" className="ml-1">{newsletters.length}</Badge>
              </TabsTrigger>
            </TabsList>
            <TabsContent value="social" className="mt-4">
              <ContentSection title="Social" icon={Share2} items={socials} formatDate={formatDate} onClick={setEditingContent} onDelete={handleDeleteContent} />
            </TabsContent>
            <TabsContent value="blog" className="mt-4">
              <ContentSection title="Blog" icon={PenLine} items={blogs} formatDate={formatDate} onClick={setEditingContent} onDelete={handleDeleteContent} />
            </TabsContent>
            <TabsContent value="newsletter" className="mt-4">
              <ContentSection title="Newsletter" icon={Mail} items={newsletters} formatDate={formatDate} onClick={setEditingContent} onDelete={handleDeleteContent} />
            </TabsContent>
          </Tabs>
        </TabsContent>

        <TabsContent value="calendar" className="mt-6">
          <ProjectCalendarSection agencyId={agencyId as string} projectId={projectId as string} />
        </TabsContent>

        <TabsContent value="schedules" className="mt-6">
          <ProjectSchedulesSection agencyId={agencyId as string} projectId={projectId as string} project={project} />
        </TabsContent>

        <TabsContent value="approvals" className="mt-6">
          <ProjectApprovalsSection agencyId={agencyId as string} projectId={projectId as string} />
        </TabsContent>

        <TabsContent value="analytics" className="mt-6">
          <ProjectAnalyticsSection
            agencyId={agencyId as string}
            projectId={projectId as string}
            contents={allContents}
            formatDate={formatDate}
          />
        </TabsContent>

        <TabsContent value="notifications" className="mt-6">
          <ProjectNotificationsSection agencyId={agencyId as string} projectId={projectId as string} />
        </TabsContent>

        <TabsContent value="sources" className="mt-6 space-y-6">
          <ProjectBrandVoiceCard
            agencyId={agencyId as string}
            projectId={projectId as string}
            project={project}
            onExtract={handleExtractBrand}
            extracting={brandExtracting}
          />
          <ProjectContentSourcesSection agencyId={agencyId as string} projectId={projectId as string} />
          <ProjectPromptsSection agencyId={agencyId as string} projectId={projectId as string} />
        </TabsContent>

        <TabsContent value="settings" className="mt-6 space-y-6">
          <ProjectApprovalSettingsCard
            agencyId={agencyId as string}
            projectId={projectId as string}
            project={project}
            agency={agency}
          />
          <ProjectImageSettingsCard
            agencyId={agencyId as string}
            projectId={projectId as string}
            project={project}
            agency={agency}
          />
          <ProjectEmailNotificationsCard
            agencyId={agencyId as string}
            projectId={projectId as string}
            project={project}
          />
          <ProjectTelegramBot agencyId={agencyId as string} projectId={projectId as string} />
          <ProjectDeleteCard agencyId={agencyId as string} projectId={projectId as string} />
        </TabsContent>
      </Tabs>

      {generateFor && (
        <GenerateContentDialog
          open={!!generateFor}
          onOpenChange={(open) => !open && setGenerateFor(null)}
          agencyId={agencyId as string}
          agentType={generateFor.type}
          agentLabel={generateFor.label}
          projectId={projectId as string}
          onStarted={() => refetchContent()}
        />
      )}

      <ContentEditDialog
        content={editingContent}
        agencyId={agencyId as string}
        open={!!editingContent}
        onOpenChange={(open) => { if (!open) setEditingContent(null); }}
      />
    </div>
  );
}

function ContentSection({
  title,
  icon: Icon,
  items,
  formatDate,
  onClick,
  onDelete,
}: {
  title: string;
  icon: any;
  items: GeneratedContent[];
  formatDate: (iso: string) => string;
  onClick: (c: GeneratedContent) => void;
  onDelete: (c: GeneratedContent) => void;
}) {
  const totalCost = items.reduce(
    (acc, c) => acc + (c.aiGenerationCostUsd ?? 0) + (c.aiImageCostUsd ?? 0),
    0
  );
  return (
    <div>
      {items.length === 0 ? (
        <Card className="text-center py-8">
          <CardContent>
            <p className="text-muted-foreground text-sm">Nessun contenuto {title.toLowerCase()} generato per questo progetto</p>
          </CardContent>
        </Card>
      ) : (
        <>
        {totalCost > 0 && (
          <p className="text-xs text-muted-foreground mb-3">
            Totale costo AI ({items.length} contenut{items.length === 1 ? "o" : "i"}):
            <span className="ml-1 font-semibold">${totalCost.toFixed(4)}</span>
          </p>
        )}
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-3">
          {items.map((c) => (
            <Card key={c.id} className="hover:shadow-md transition-shadow cursor-pointer group relative" onClick={() => onClick(c)}>
              <CardContent className="pt-4">
                <button
                  type="button"
                  onClick={(e) => { e.stopPropagation(); onDelete(c); }}
                  className="absolute top-2 right-2 p-1.5 rounded-md opacity-0 group-hover:opacity-100 hover:bg-destructive/10 hover:text-destructive transition-opacity"
                  aria-label="Elimina contenuto"
                  title="Elimina"
                >
                  <Trash2 className="size-3.5" />
                </button>
                <div className="flex items-start gap-3">
                  {c.imageUrl && (
                    <img src={resolveImageUrl(c.imageUrl)} alt={c.title} className="w-16 h-16 rounded object-cover shrink-0 ring-1 ring-border" />
                  )}
                  <div className="flex-1 min-w-0">
                    <h4 className="font-medium text-sm truncate">{c.title}</h4>
                    <p className="text-xs text-muted-foreground line-clamp-2 mt-1">{c.body.slice(0, 120)}</p>
                    <div className="flex items-center gap-1.5 mt-2 flex-wrap">
                      <Badge variant="outline" className="text-[10px] gap-0.5" title={`Qualità: ${c.qualityScore}/10 · SEO: ${c.seoScore}/10 · Brand: ${c.brandVoiceScore}/10`}>
                        ⭐ {c.overallScore?.toFixed?.(1) ?? c.overallScore}/10
                      </Badge>
                      {c.status === ContentStatus.Published && (
                        <Badge className="bg-emerald-500 text-white hover:bg-emerald-600 text-[10px]">
                          Pubblicato{c.publishedAt ? ` · ${new Date(c.publishedAt).toLocaleDateString("it-IT")}` : ""}
                        </Badge>
                      )}
                      {c.status === ContentStatus.Approved && c.isScheduled && (
                        <Badge className="bg-amber-500 text-white hover:bg-amber-600 text-[10px]">
                          Programmato
                        </Badge>
                      )}
                      {c.status === ContentStatus.Approved && !c.isScheduled && (
                        <Badge className="bg-blue-500 text-white hover:bg-blue-600 text-[10px]">
                          Approvato{c.approvedAt ? ` · ${new Date(c.approvedAt).toLocaleDateString("it-IT")}` : ""}
                        </Badge>
                      )}
                      {c.status === ContentStatus.Rejected && (
                        <Badge className="bg-red-500 text-white hover:bg-red-600 text-[10px]">
                          Rifiutato
                        </Badge>
                      )}
                      {c.status === ContentStatus.InReview && (
                        <Badge variant="outline" className="text-[10px]">
                          In revisione
                        </Badge>
                      )}
                    </div>
                    <div className="flex items-center gap-2 mt-2 text-xs text-muted-foreground">
                      <Calendar className="size-3" />
                      <span>{formatDate(c.createdAt)}</span>
                      {(c.aiGenerationCostUsd || c.aiImageCostUsd) && (
                        <span className="ml-auto text-[10px] font-medium" title="Costo AI stimato">
                          ${((c.aiGenerationCostUsd ?? 0) + (c.aiImageCostUsd ?? 0)).toFixed(4)}
                        </span>
                      )}
                      {c.imageUrls && c.imageUrls.length > 1 && (
                        <Badge variant="outline" className="ml-auto">
                          <Images className="size-3" /> {c.imageUrls.length}
                        </Badge>
                      )}
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
        </>
      )}
    </div>
  );
}

function ProjectPromptsSection({ agencyId, projectId }: { agencyId: string; projectId: string }) {
  const queryClient = useQueryClient();
  const { data: prompts, isLoading } = useQuery({
    queryKey: ["project-prompts", agencyId, projectId],
    queryFn: () => projectsApi.getPrompts(agencyId, projectId),
  });

  const [blog, setBlog] = useState<string>("");
  const [social, setSocial] = useState<string>("");
  const [newsletter, setNewsletter] = useState<string>("");
  const [context, setContext] = useState<string>("");

  useEffect(() => {
    if (prompts) {
      setBlog(prompts.blogPromptTemplate ?? "");
      setSocial(prompts.socialPromptTemplate ?? "");
      setNewsletter(prompts.newsletterPromptTemplate ?? "");
      setContext(prompts.extractedContext ?? "");
    }
  }, [prompts]);

  const save = useMutation({
    mutationFn: () =>
      projectsApi.updatePrompts(agencyId, projectId, {
        blogPromptTemplate: blog,
        socialPromptTemplate: social,
        newsletterPromptTemplate: newsletter,
        extractedContext: context,
      }),
    onSuccess: () => {
      toast.success("Prompt salvati");
      queryClient.invalidateQueries({ queryKey: ["project-prompts", agencyId, projectId] });
      queryClient.invalidateQueries({ queryKey: ["project", agencyId, projectId] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  return (
    <Card data-testid="project-prompts-section">
      <CardHeader>
        <CardTitle className="text-base flex items-center gap-2">
          <Sparkles className="size-4" /> Prompt di progetto
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        {isLoading && <Skeleton className="h-24" />}
        {!isLoading && (
          <>
            <p className="text-xs text-muted-foreground">
              Usa &quot;Estrai dal sito&quot; per generare automaticamente contesto e template dal tuo sito.
              Placeholder disponibili: <code>{"{product}"}</code> <code>{"{brandVoice}"}</code>{" "}
              <code>{"{audience}"}</code> <code>{"{projectContext}"}</code> <code>{"{sources}"}</code>{" "}
              <code>{"{task}"}</code>.
            </p>
            <div>
              <Label>Contesto estratto</Label>
              <Textarea
                rows={4}
                value={context}
                onChange={(e) => setContext(e.target.value)}
                placeholder="Descrizione del progetto estratta dal sito"
                data-testid="extracted-context"
              />
              {prompts?.extractedContextAt && (
                <p className="text-[10px] text-muted-foreground mt-1">
                  Estratto: {new Date(prompts.extractedContextAt).toLocaleString("it-IT")}
                </p>
              )}
            </div>
            <div>
              <Label>Template Blog</Label>
              <Textarea
                rows={6}
                value={blog}
                onChange={(e) => setBlog(e.target.value)}
                placeholder="Prompt per generare articoli blog per questo progetto"
                data-testid="blog-prompt"
              />
            </div>
            <div>
              <Label>Template Social</Label>
              <Textarea
                rows={5}
                value={social}
                onChange={(e) => setSocial(e.target.value)}
                placeholder="Prompt per generare post social"
                data-testid="social-prompt"
              />
            </div>
            <div>
              <Label>Template Newsletter</Label>
              <Textarea
                rows={5}
                value={newsletter}
                onChange={(e) => setNewsletter(e.target.value)}
                placeholder="Prompt per generare newsletter"
                data-testid="newsletter-prompt"
              />
            </div>
            <Button
              onClick={() => save.mutate()}
              disabled={save.isPending}
              data-testid="save-prompts"
            >
              {save.isPending ? <Loader2 className="size-4 animate-spin" /> : null} Salva prompt
            </Button>
          </>
        )}
      </CardContent>
    </Card>
  );
}

interface CalendarEntryListItem {
  id: string;
  contentId: string;
  contentTitle: string;
  contentType: string;
  platform?: string | null;
  scheduledAt: string;
  publishedAt?: string | null;
  status: string;
  errorMessage?: string | null;
  postUrl?: string | null;
}

function ProjectApprovalsSection({ agencyId, projectId }: { agencyId: string; projectId: string }) {
  const queryClient = useQueryClient();
  const { data: pendingData, isLoading: loadingPending } = useQuery({
    queryKey: ["approvals", agencyId, "pending"],
    queryFn: () => apiClient.get<ApiResponse<any[]>>(`/api/v1/agencies/${agencyId}/approvals`),
  });
  const { data: historyData, isLoading: loadingHistory } = useQuery({
    queryKey: ["approvals", agencyId, "history", "all"],
    queryFn: () => apiClient.get<ApiResponse<any[]>>(`/api/v1/agencies/${agencyId}/approvals/history?take=100`),
  });
  const pending = (pendingData?.data ?? []).filter((p: any) => p.projectId === projectId);
  const history = (historyData?.data ?? []).filter((p: any) => p.projectId === projectId);

  const approve = useMutation({
    mutationFn: (id: string) => apiClient.put(`/api/v1/agencies/${agencyId}/approvals/${id}/approve`),
    onSuccess: () => {
      toast.success("Contenuto approvato");
      queryClient.invalidateQueries({ queryKey: ["approvals", agencyId] });
      queryClient.invalidateQueries({ queryKey: ["content", agencyId] });
    },
    onError: (e: Error) => toast.error(e.message),
  });
  const reject = useMutation({
    mutationFn: (id: string) => apiClient.put(`/api/v1/agencies/${agencyId}/approvals/${id}/reject`),
    onSuccess: () => {
      toast.info("Contenuto rifiutato");
      queryClient.invalidateQueries({ queryKey: ["approvals", agencyId] });
    },
    onError: (e: Error) => toast.error(e.message),
  });
  const deleteContent = useMutation({
    mutationFn: (id: string) => apiClient.delete(`/api/v1/agencies/${agencyId}/approvals/${id}`),
    onSuccess: () => {
      toast.success("Contenuto eliminato");
      queryClient.invalidateQueries({ queryKey: ["approvals", agencyId] });
      queryClient.invalidateQueries({ queryKey: ["content", agencyId] });
      queryClient.invalidateQueries({ queryKey: ["project-calendar", agencyId] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  if (loadingPending || loadingHistory) {
    return <Skeleton className="h-40" />;
  }

  return (
    <div className="space-y-6">
      <div>
        <h3 className="text-sm font-semibold mb-3 text-muted-foreground">In attesa di approvazione ({pending.length})</h3>
        {pending.length === 0 ? (
          <Card><CardContent className="pt-5 text-sm text-muted-foreground">Nessuna approvazione in sospeso per questo progetto.</CardContent></Card>
        ) : (
          <div className="space-y-3">
            {pending.map((item: any) => (
              <Card key={item.id}>
                <CardContent className="pt-5">
                  <div className="flex items-start gap-3">
                    {item.imageUrl && (
                      <img src={resolveImageUrl(item.imageUrl)} alt={item.title} className="w-16 h-16 rounded object-cover shrink-0 ring-1 ring-border" />
                    )}
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center justify-between gap-2">
                        <div className="font-medium text-sm truncate">{item.title}</div>
                        <div className="flex gap-2 shrink-0">
                          <Button size="sm" variant="outline" className="text-destructive" onClick={() => reject.mutate(item.id)} disabled={reject.isPending}>Rifiuta</Button>
                          <Button size="sm" onClick={() => approve.mutate(item.id)} disabled={approve.isPending}>Approva</Button>
                        </div>
                      </div>
                      <p className="text-xs text-muted-foreground line-clamp-3 mt-1">{item.body}</p>
                      <p className="text-[10px] text-muted-foreground mt-1">Score: {item.overallScore?.toFixed?.(1) ?? "-"}/10 · {new Date(item.createdAt).toLocaleString("it-IT")}</p>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </div>

      <div>
        <h3 className="text-sm font-semibold mb-3 text-muted-foreground">Cronologia ({history.length})</h3>
        {history.length === 0 ? (
          <Card><CardContent className="pt-5 text-sm text-muted-foreground">Nessuna cronologia.</CardContent></Card>
        ) : (
          <div className="space-y-2">
            {history.map((item: any) => (
              <Card key={item.id}>
                <CardContent className="pt-4 pb-4">
                  <div className="flex items-start gap-3">
                    {item.imageUrl && (
                      <img src={resolveImageUrl(item.imageUrl)} alt={item.title} className="w-12 h-12 rounded object-cover shrink-0" />
                    )}
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2 flex-wrap">
                        <span className="font-medium text-sm truncate">{item.title}</span>
                        <Badge variant={item.status === 5 ? "destructive" : item.status === 4 ? "secondary" : item.status === 3 ? "default" : "outline"} className="text-[10px]">
                          {item.status === 3 ? "Approvato" : item.status === 4 ? "Pubblicato" : item.status === 5 ? "Rifiutato" : "Bozza"}
                        </Badge>
                        {item.autoApproved && <Badge variant="outline" className="text-[10px]">Auto</Badge>}
                      </div>
                      <p className="text-[10px] text-muted-foreground mt-1">{new Date(item.approvedAt ?? item.createdAt).toLocaleString("it-IT")}</p>
                    </div>
                    <Button
                      size="sm"
                      variant="ghost"
                      className="shrink-0 text-destructive hover:text-destructive"
                      onClick={() => { if (confirm("Eliminare questo contenuto?")) deleteContent.mutate(item.id); }}
                    >
                      <Trash2 className="size-4" />
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

function ProjectCalendarSection({ agencyId, projectId }: { agencyId: string; projectId: string }) {
  const queryClient = useQueryClient();
  const { data: schedulesRes } = useQuery({
    queryKey: ["project-schedules", agencyId, projectId],
    queryFn: () => schedulesApi.list(agencyId),
  });
  const projectSchedules = (schedulesRes?.data || []).filter(
    (s: ContentSchedule) => s.projectId === projectId && s.isActive && s.nextRunAt
  );
  const [view, setView] = useState<"list" | "month">("list");
  const [monthCursor, setMonthCursor] = useState<Date>(() => {
    const d = new Date();
    return new Date(d.getFullYear(), d.getMonth(), 1);
  });
  const [expandedError, setExpandedError] = useState<Record<string, boolean>>({});

  const { data: entries, isLoading } = useQuery({
    queryKey: ["project-calendar", agencyId, projectId],
    queryFn: async () => {
      const res = await apiClient.get<ApiResponse<CalendarEntryListItem[]>>(
        `/api/v1/agencies/${agencyId}/projects/${projectId}/calendar`
      );
      return res.data ?? [];
    },
  });

  const publishNow = useMutation({
    mutationFn: (entryId: string) =>
      apiClient.post<ApiResponse<{ success: boolean; message?: string }>>(
        `/api/v1/agencies/${agencyId}/calendar/${entryId}/publish-now`
      ),
    onSuccess: (res) => {
      if (res.data?.success) toast.success("Pubblicato");
      else toast.error(res.data?.message ?? "Pubblicazione fallita");
      queryClient.invalidateQueries({ queryKey: ["project-calendar", agencyId, projectId] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const remove = useMutation({
    mutationFn: (entryId: string) =>
      apiClient.delete(`/api/v1/agencies/${agencyId}/calendar/${entryId}`),
    onSuccess: () => {
      toast.success("Elemento rimosso");
      queryClient.invalidateQueries({ queryKey: ["project-calendar", agencyId, projectId] });
    },
  });

  const statusBadge = (s: string) => {
    const dot = (cls: string) => <span className={`size-2 rounded-full ${cls} inline-block mr-1.5`} />;
    switch (s) {
      case "Published": return <Badge variant="outline">{dot("bg-emerald-500")}Pubblicato</Badge>;
      case "Failed": return <Badge variant="outline">{dot("bg-red-500")}Errore</Badge>;
      case "Scheduled": return <Badge variant="outline">{dot("bg-amber-500")}Programmato</Badge>;
      case "Draft": return <Badge variant="outline">{dot("bg-slate-400")}Bozza</Badge>;
      default: return <Badge variant="outline">{s}</Badge>;
    }
  };

  return (
    <Card data-testid="project-calendar-section">
      <CardHeader className="space-y-2">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <CardTitle className="text-base flex items-center gap-2">
            <Calendar className="size-4" /> Calendario editoriale
          </CardTitle>
          <div className="inline-flex rounded-md border text-xs">
            <button
              type="button"
              className={`px-3 py-1 ${view === "list" ? "bg-muted" : ""}`}
              onClick={() => setView("list")}
            >
              Lista
            </button>
            <button
              type="button"
              className={`px-3 py-1 border-l ${view === "month" ? "bg-muted" : ""}`}
              onClick={() => setView("month")}
            >
              Mese
            </button>
          </div>
        </div>
        <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-[11px] text-muted-foreground">
          <span className="font-medium">Legenda:</span>
          <span className="flex items-center gap-1.5">
            <span className="size-2.5 rounded-full bg-slate-400 inline-block" /> Bozza
          </span>
          <span className="flex items-center gap-1.5">
            <span className="size-2.5 rounded-full bg-amber-500 inline-block" /> Programmato
          </span>
          <span className="flex items-center gap-1.5">
            <span className="size-2.5 rounded-full bg-emerald-500 inline-block" /> Pubblicato
          </span>
          <span className="flex items-center gap-1.5">
            <span className="size-2.5 rounded-full bg-red-500 inline-block" /> Errore
          </span>
        </div>
      </CardHeader>
      <CardContent className="space-y-2">
        {projectSchedules.length > 0 && (
          <div className="flex flex-wrap gap-2 pb-2 border-b mb-2">
            {projectSchedules.map((s: ContentSchedule) => (
              <div
                key={s.id}
                className={`inline-flex items-center gap-1.5 text-[11px] px-2 py-1 rounded-md border ${
                  s.scheduleType === 2
                    ? "border-amber-300 bg-amber-50 text-amber-700 dark:border-amber-700 dark:bg-amber-950 dark:text-amber-300"
                    : "border-blue-300 bg-blue-50 text-blue-700 dark:border-blue-700 dark:bg-blue-950 dark:text-blue-300"
                }`}
              >
                <span className={`size-2 rounded-full ${s.scheduleType === 2 ? "bg-amber-500" : "bg-blue-500"}`} />
                <span className="font-medium">{s.name}</span>
                <span>·</span>
                <span>{s.scheduleType === 2 ? "Pubblica" : "Genera"}</span>
                {s.nextRunAt && (
                  <>
                    <span>·</span>
                    <span>Prossimo: {new Date(s.nextRunAt).toLocaleString("it-IT", { day: "2-digit", month: "short", hour: "2-digit", minute: "2-digit" })}</span>
                  </>
                )}
              </div>
            ))}
          </div>
        )}
        {isLoading && <Skeleton className="h-16" />}
        {!isLoading && !entries?.length && (
          <p className="text-sm text-muted-foreground">
            Nessun contenuto programmato per questo progetto.
          </p>
        )}
        {!isLoading && !!entries?.length && view === "list" && entries.map((e) => {
          const errOpen = !!expandedError[e.id];
          return (
            <div
              key={e.id}
              className="p-3 rounded-md border"
              data-testid={`calendar-entry-${e.id}`}
            >
              <div className="flex items-start gap-3">
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 flex-wrap">
                    <span className="font-medium text-sm truncate">{e.contentTitle}</span>
                    <Badge variant="outline" className="text-[10px]">{e.contentType}</Badge>
                    {e.platform && <Badge variant="outline" className="text-[10px]">{e.platform}</Badge>}
                    {statusBadge(e.status)}
                  </div>
                  <p className="text-xs text-muted-foreground mt-1">
                    Programmato: {new Date(e.scheduledAt).toLocaleString("it-IT")}
                    {e.publishedAt && ` · Pubblicato: ${new Date(e.publishedAt).toLocaleString("it-IT")}`}
                  </p>
                  {e.postUrl && (
                    <a
                      href={e.postUrl}
                      target="_blank"
                      rel="noreferrer"
                      className="text-xs text-primary hover:underline mt-1 inline-block"
                    >
                      Apri post →
                    </a>
                  )}
                  {e.errorMessage && (
                    <div className="mt-2">
                      <button
                        type="button"
                        className="text-xs text-destructive hover:underline"
                        onClick={() =>
                          setExpandedError((s) => ({ ...s, [e.id]: !s[e.id] }))
                        }
                      >
                        {errOpen ? "Nascondi errore" : "Mostra errore"}
                      </button>
                      {errOpen && (
                        <pre className="mt-1 whitespace-pre-wrap break-words text-[11px] bg-destructive/10 text-destructive rounded p-2 max-h-48 overflow-auto">
                          {e.errorMessage}
                        </pre>
                      )}
                    </div>
                  )}
                </div>
                <div className="flex flex-col gap-1 shrink-0">
                  {e.platform && e.status !== "Published" && (
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => publishNow.mutate(e.id)}
                      disabled={publishNow.isPending}
                    >
                      Pubblica ora
                    </Button>
                  )}
                  <Button
                    size="sm"
                    variant="ghost"
                    onClick={() => remove.mutate(e.id)}
                    className="text-destructive"
                  >
                    <Trash2 className="size-4" />
                  </Button>
                </div>
              </div>
            </div>
          );
        })}
        {!isLoading && !!entries?.length && view === "month" && (
          <MonthCalendarGrid
            cursor={monthCursor}
            onCursorChange={setMonthCursor}
            entries={entries}
            onPublishNow={(id) => publishNow.mutate(id)}
            onRemove={(id) => remove.mutate(id)}
          />
        )}
      </CardContent>
    </Card>
  );
}

function MonthCalendarGrid({
  cursor,
  onCursorChange,
  entries,
  onPublishNow,
  onRemove,
}: {
  cursor: Date;
  onCursorChange: (d: Date) => void;
  entries: CalendarEntryListItem[];
  onPublishNow: (id: string) => void;
  onRemove: (id: string) => void;
}) {
  const [selected, setSelected] = useState<string | null>(null);
  const [selectedDay, setSelectedDay] = useState<number | null>(null);
  const year = cursor.getFullYear();
  const month = cursor.getMonth();
  const firstDay = new Date(year, month, 1);
  const startOffset = (firstDay.getDay() + 6) % 7; // Monday-first
  const daysInMonth = new Date(year, month + 1, 0).getDate();
  const totalCells = Math.ceil((startOffset + daysInMonth) / 7) * 7;

  const entriesByDay = new Map<number, CalendarEntryListItem[]>();
  for (const e of entries) {
    const d = new Date(e.scheduledAt);
    if (d.getFullYear() !== year || d.getMonth() !== month) continue;
    const day = d.getDate();
    const list = entriesByDay.get(day) ?? [];
    list.push(e);
    entriesByDay.set(day, list);
  }

  const monthLabel = cursor.toLocaleDateString("it-IT", { month: "long", year: "numeric" });
  const dotColor = (s: string) => {
    switch (s) {
      case "Published": return "bg-emerald-500";
      case "Failed": return "bg-red-500";
      case "Scheduled": return "bg-amber-500";
      default: return "bg-slate-400";
    }
  };

  const selectedEntry = selected ? entries.find((x) => x.id === selected) : null;

  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between">
        <button
          type="button"
          className="text-xs px-2 py-1 rounded border hover:bg-muted"
          onClick={() => onCursorChange(new Date(year, month - 1, 1))}
        >
          ← Prec
        </button>
        <span className="text-sm font-medium capitalize">{monthLabel}</span>
        <button
          type="button"
          className="text-xs px-2 py-1 rounded border hover:bg-muted"
          onClick={() => onCursorChange(new Date(year, month + 1, 1))}
        >
          Succ →
        </button>
      </div>
      <div className="grid grid-cols-7 gap-px bg-border border rounded overflow-hidden text-[11px]">
        {["Lun", "Mar", "Mer", "Gio", "Ven", "Sab", "Dom"].map((d) => (
          <div key={d} className="bg-muted py-1 text-center font-medium text-muted-foreground">
            {d}
          </div>
        ))}
        {Array.from({ length: totalCells }).map((_, idx) => {
          const dayNum = idx - startOffset + 1;
          const inMonth = dayNum >= 1 && dayNum <= daysInMonth;
          const dayEntries = inMonth ? entriesByDay.get(dayNum) ?? [] : [];
          return (
            <div
              key={idx}
              className={`bg-background min-h-[70px] p-1 ${inMonth ? "" : "opacity-40"}`}
            >
              {inMonth && (
                <>
                  <button
                    type="button"
                    className="text-[10px] font-medium text-muted-foreground hover:text-primary"
                    onClick={() => { setSelectedDay(selectedDay === dayNum ? null : dayNum); setSelected(null); }}
                  >
                    {dayNum}
                  </button>
                  <div className="space-y-0.5 mt-1">
                    {dayEntries.slice(0, 3).map((e) => (
                      <button
                        key={e.id}
                        type="button"
                        onClick={() => { setSelected(e.id); setSelectedDay(null); }}
                        className="w-full text-left flex items-center gap-1 truncate hover:underline"
                      >
                        <span className={`size-1.5 rounded-full ${dotColor(e.status)} shrink-0`} />
                        <span className="truncate">{e.contentTitle}</span>
                      </button>
                    ))}
                    {dayEntries.length > 3 && (
                      <button
                        type="button"
                        className="text-[10px] text-primary hover:underline"
                        onClick={() => { setSelectedDay(dayNum); setSelected(null); }}
                      >
                        +{dayEntries.length - 3} altri
                      </button>
                    )}
                  </div>
                </>
              )}
            </div>
          );
        })}
      </div>
      {selectedDay && (entriesByDay.get(selectedDay) ?? []).length > 0 && (
        <div className="p-3 rounded-md border space-y-2">
          <div className="flex items-center justify-between">
            <span className="font-medium text-sm">
              {selectedDay} {cursor.toLocaleDateString("it-IT", { month: "long" })} — {(entriesByDay.get(selectedDay) ?? []).length} post
            </span>
            <button type="button" className="text-xs text-muted-foreground hover:text-foreground" onClick={() => setSelectedDay(null)}>✕</button>
          </div>
          <div className="space-y-2 max-h-64 overflow-auto">
            {(entriesByDay.get(selectedDay) ?? []).map((e) => (
              <div key={e.id} className="flex items-center gap-2 p-2 rounded hover:bg-muted/50 cursor-pointer" onClick={() => { setSelected(e.id); setSelectedDay(null); }}>
                <span className={`size-2 rounded-full ${dotColor(e.status)} shrink-0`} />
                <span className="text-xs flex-1 truncate">{e.contentTitle}</span>
                {e.platform && <Badge variant="outline" className="text-[10px]">{e.platform}</Badge>}
                <Badge variant="outline" className="text-[10px]">{e.status}</Badge>
                <span className="text-[10px] text-muted-foreground shrink-0">{new Date(e.scheduledAt).toLocaleTimeString("it-IT", { hour: "2-digit", minute: "2-digit" })}</span>
              </div>
            ))}
          </div>
        </div>
      )}
      {selectedEntry && (
        <div className="p-3 rounded-md border space-y-2">
          <div className="flex items-start gap-3">
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2 flex-wrap">
                <span className="font-medium text-sm">{selectedEntry.contentTitle}</span>
                <Badge variant="outline" className="text-[10px]">{selectedEntry.contentType}</Badge>
                {selectedEntry.platform && (
                  <Badge variant="outline" className="text-[10px]">{selectedEntry.platform}</Badge>
                )}
                <Badge variant="outline" className="text-[10px]">{selectedEntry.status}</Badge>
              </div>
              <p className="text-xs text-muted-foreground mt-1">
                Programmato: {new Date(selectedEntry.scheduledAt).toLocaleString("it-IT")}
                {selectedEntry.publishedAt && ` · Pubblicato: ${new Date(selectedEntry.publishedAt).toLocaleString("it-IT")}`}
              </p>
              {selectedEntry.postUrl && (
                <a
                  href={selectedEntry.postUrl}
                  target="_blank"
                  rel="noreferrer"
                  className="text-xs text-primary hover:underline mt-1 inline-block"
                >
                  Apri post →
                </a>
              )}
              {selectedEntry.errorMessage && (
                <pre className="mt-2 whitespace-pre-wrap break-words text-[11px] bg-destructive/10 text-destructive rounded p-2 max-h-48 overflow-auto">
                  {selectedEntry.errorMessage}
                </pre>
              )}
            </div>
            <div className="flex flex-col gap-1 shrink-0">
              {selectedEntry.platform && selectedEntry.status !== "Published" && (
                <Button
                  size="sm"
                  variant="outline"
                  onClick={() => onPublishNow(selectedEntry.id)}
                >
                  Pubblica ora
                </Button>
              )}
              <Button
                size="sm"
                variant="ghost"
                onClick={() => {
                  onRemove(selectedEntry.id);
                  setSelected(null);
                }}
                className="text-destructive"
              >
                <Trash2 className="size-4" />
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

function ProjectBrandVoiceCard({
  agencyId,
  projectId,
  project,
  onExtract,
  extracting,
}: {
  agencyId: string;
  projectId: string;
  project: any;
  onExtract: () => void;
  extracting: boolean;
}) {
  const queryClient = useQueryClient();
  const [tone, setTone] = useState<string>("");
  const [style, setStyle] = useState<string>("");
  const [keywords, setKeywords] = useState<string>("");
  const [audienceDesc, setAudienceDesc] = useState<string>("");
  const [audienceInterests, setAudienceInterests] = useState<string>("");
  const [editWebsiteUrl, setEditWebsiteUrl] = useState<string>("");

  useEffect(() => {
    if (project) {
      setTone(project.brandVoice?.tone ?? "");
      setStyle(project.brandVoice?.style ?? "");
      setKeywords((project.brandVoice?.keywords ?? []).join(", "));
      setAudienceDesc(project.targetAudience?.description ?? "");
      setAudienceInterests((project.targetAudience?.interests ?? []).join(", "));
      setEditWebsiteUrl(project.websiteUrl ?? "");
    }
  }, [project]);

  const save = useMutation({
    mutationFn: () =>
      projectsApi.update(agencyId, projectId, {
        name: project.name,
        description: project.description,
        websiteUrl: editWebsiteUrl || null,
        logoUrl: project.logoUrl,
        brandVoice: {
          ...(project.brandVoice ?? {}),
          tone,
          style,
          keywords: keywords.split(",").map((k) => k.trim()).filter(Boolean),
        },
        targetAudience: {
          ...(project.targetAudience ?? {}),
          description: audienceDesc,
          interests: audienceInterests.split(",").map((k) => k.trim()).filter(Boolean),
        },
      } as any),
    onSuccess: () => {
      toast.success("Brand voice salvato");
      queryClient.invalidateQueries({ queryKey: ["project", agencyId, projectId] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  return (
    <Card data-testid="project-brand-voice-card">
      <CardHeader className="flex flex-row items-center justify-between">
        <CardTitle className="text-base">Brand Voice &amp; Audience</CardTitle>
        <Button size="sm" variant="outline" onClick={onExtract} disabled={extracting || !editWebsiteUrl}>
          {extracting ? <Loader2 className="size-4 animate-spin" /> : <Sparkles className="size-4" />}
          Estrai dal sito
        </Button>
      </CardHeader>
      <CardContent className="space-y-3">
        <div>
          <Label className="text-xs">Sito web del progetto</Label>
          <div className="flex items-center gap-2">
            <Input
              value={editWebsiteUrl}
              onChange={(e) => setEditWebsiteUrl(e.target.value)}
              placeholder="https://www.esempio.com"
            />
            {editWebsiteUrl && (
              <a href={editWebsiteUrl} target="_blank" rel="noopener noreferrer" className="text-primary hover:underline text-xs shrink-0">
                Apri →
              </a>
            )}
          </div>
        </div>
        <div className="grid grid-cols-2 gap-2">
          <div>
            <Label className="text-xs">Tono</Label>
            <Input value={tone} onChange={(e) => setTone(e.target.value)} placeholder="professionale, amichevole..." />
          </div>
          <div>
            <Label className="text-xs">Stile</Label>
            <Input value={style} onChange={(e) => setStyle(e.target.value)} placeholder="formale, conversazionale..." />
          </div>
        </div>
        <div>
          <Label className="text-xs">Keywords (separate da virgola)</Label>
          <Input value={keywords} onChange={(e) => setKeywords(e.target.value)} placeholder="cloud, saas, presenze" />
        </div>
        <div>
          <Label className="text-xs">Audience: descrizione</Label>
          <Textarea
            rows={2}
            value={audienceDesc}
            onChange={(e) => setAudienceDesc(e.target.value)}
            placeholder="Aziende del settore agricolo con piu di 10 dipendenti..."
          />
        </div>
        <div>
          <Label className="text-xs">Audience: interessi (separati da virgola)</Label>
          <Input
            value={audienceInterests}
            onChange={(e) => setAudienceInterests(e.target.value)}
            placeholder="produttivita, compliance, digitalizzazione"
          />
        </div>
        <Button size="sm" onClick={() => save.mutate()} disabled={save.isPending} data-testid="save-brand-voice">
          {save.isPending ? <Loader2 className="size-4 animate-spin" /> : null} Salva
        </Button>
      </CardContent>
    </Card>
  );
}

interface ContentSourceItem {
  id: string;
  agencyId: string;
  projectId?: string | null;
  type: number;
  url: string;
  name?: string | null;
  isActive: boolean;
}

function ProjectApprovalSettingsCard({
  agencyId,
  projectId,
  project,
  agency,
}: {
  agencyId: string;
  projectId: string;
  project: any;
  agency: any;
}) {
  const queryClient = useQueryClient();
  const [mode, setMode] = useState<number>(project?.approvalMode ?? agency?.approvalMode ?? ApprovalMode.Manual);
  const [minScore, setMinScore] = useState<number>(project?.autoApproveMinScore ?? agency?.autoApproveMinScore ?? 7);
  const [autoSchedule, setAutoSchedule] = useState<boolean>(
    project?.autoScheduleOnApproval ?? agency?.autoScheduleOnApproval ?? true
  );
  const usingAgencyDefault = project?.approvalMode == null && project?.autoScheduleOnApproval == null;

  useEffect(() => {
    setMode(project?.approvalMode ?? agency?.approvalMode ?? ApprovalMode.Manual);
    setMinScore(project?.autoApproveMinScore ?? agency?.autoApproveMinScore ?? 7);
    setAutoSchedule(project?.autoScheduleOnApproval ?? agency?.autoScheduleOnApproval ?? true);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [project?.id, agency?.id, project?.approvalMode, project?.autoApproveMinScore, project?.autoScheduleOnApproval]);

  const saveOverride = useMutation({
    mutationFn: () => projectsApi.updateApprovalMode(agencyId, projectId, mode, minScore, autoSchedule),
    onSuccess: () => {
      toast.success("Metodo di approvazione salvato per il progetto");
      queryClient.invalidateQueries({ queryKey: ["project", agencyId, projectId] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const resetToAgency = useMutation({
    mutationFn: () => projectsApi.updateApprovalMode(agencyId, projectId, null, null, null),
    onSuccess: () => {
      toast.success("Ripristinato al default dell'agenzia");
      queryClient.invalidateQueries({ queryKey: ["project", agencyId, projectId] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const agencyModeLabel = (m?: number) => {
    switch (m) {
      case ApprovalMode.AutoApprove: return "Auto (sempre)";
      case ApprovalMode.AutoApproveAboveScore: return `Auto se score >= ${agency?.autoApproveMinScore ?? 7}`;
      default: return "Manuale";
    }
  };

  return (
    <Card data-testid="project-approval-settings">
      <CardHeader>
        <CardTitle className="text-base">Metodo di approvazione</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="rounded-md border p-3 space-y-1">
          <p className="text-xs text-muted-foreground">
            Default agenzia: <span className="font-medium">{agencyModeLabel(agency?.approvalMode)}</span>
          </p>
          <p className="text-xs">
            Stato attuale progetto:{" "}
            <span className="font-medium">
              {usingAgencyDefault ? "Usa default agenzia" : agencyModeLabel(project?.approvalMode)}
            </span>
          </p>
        </div>

        <div className="space-y-3">
          <div>
            <Label className="text-xs">Modalità (override di progetto)</Label>
            <select
              className="h-9 rounded-md border bg-transparent px-3 text-sm w-full"
              value={mode}
              onChange={(e) => setMode(Number(e.target.value))}
            >
              <option value={ApprovalMode.Manual}>Manuale (richiedi sempre approvazione)</option>
              <option value={ApprovalMode.AutoApprove}>Auto-approva tutto</option>
              <option value={ApprovalMode.AutoApproveAboveScore}>Auto-approva se score sufficiente</option>
            </select>
          </div>
          {mode === ApprovalMode.AutoApproveAboveScore && (
            <div>
              <Label className="text-xs">Score minimo per auto-approvazione</Label>
              <Input
                type="number"
                min={1}
                max={10}
                value={minScore}
                onChange={(e) => setMinScore(Number(e.target.value))}
              />
            </div>
          )}
          <div className="rounded-md border p-3 flex items-center justify-between gap-3">
            <div className="space-y-1">
              <Label className="text-xs font-medium">Auto-schedule in calendario</Label>
              <p className="text-xs text-muted-foreground">
                Se attivo, i contenuti approvati vengono subito inseriti nel calendario editoriale.
                Se disattivo, puoi pianificare manualmente ogni contenuto aprendolo dalla lista
                (tab <strong>Contenuti</strong>) e compilando la sezione &quot;Programma pubblicazione&quot;.
              </p>
            </div>
            <Switch checked={autoSchedule} onCheckedChange={setAutoSchedule} />
          </div>
          <p className="text-xs text-muted-foreground">
            Con approvazione automatica + auto-schedule i contenuti vanno direttamente in calendario.
            Con approvazione manuale o per score, ricevi una notifica e devi approvare prima che il calendario venga creato.
          </p>
        </div>

        <div className="flex gap-2">
          <Button
            size="sm"
            onClick={() => saveOverride.mutate()}
            disabled={saveOverride.isPending}
            data-testid="save-approval-settings"
          >
            {saveOverride.isPending ? <Loader2 className="size-4 animate-spin" /> : null} Salva override
          </Button>
          <Button
            size="sm"
            variant="outline"
            onClick={() => resetToAgency.mutate()}
            disabled={resetToAgency.isPending || usingAgencyDefault}
          >
            {resetToAgency.isPending ? <Loader2 className="size-4 animate-spin" /> : null} Usa default agenzia
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}

function ProjectImageSettingsCard({
  agencyId,
  projectId,
  project,
  agency,
}: {
  agencyId: string;
  projectId: string;
  project: any;
  agency: any;
}) {
  const queryClient = useQueryClient();
  const usingAgencyDefault =
    project?.enableLogoOverlay == null && !project?.logoUrl;
  const [enabled, setEnabled] = useState<boolean>(
    project?.enableLogoOverlay ?? agency?.enableLogoOverlay ?? false
  );
  const [position, setPosition] = useState<number>(
    project?.logoOverlayPosition ?? agency?.logoOverlayPosition ?? 3
  );
  const [mode, setMode] = useState<number>(
    project?.logoOverlayMode ?? agency?.logoOverlayMode ?? 0
  );
  const [bannerColor, setBannerColor] = useState<string>(
    project?.brandBannerColor ?? agency?.brandBannerColor ?? "#003366"
  );
  const [logoUrl, setLogoUrl] = useState<string>(
    project?.logoUrl ?? agency?.logoUrl ?? ""
  );
  const [uploading, setUploading] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    setEnabled(project?.enableLogoOverlay ?? agency?.enableLogoOverlay ?? false);
    setPosition(project?.logoOverlayPosition ?? agency?.logoOverlayPosition ?? 3);
    setMode(project?.logoOverlayMode ?? agency?.logoOverlayMode ?? 0);
    setBannerColor(project?.brandBannerColor ?? agency?.brandBannerColor ?? "#003366");
    setLogoUrl(project?.logoUrl ?? agency?.logoUrl ?? "");
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [project?.id]);

  const handleFileSelected = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    if (file.size > 5 * 1024 * 1024) {
      toast.error("File troppo grande (max 5 MB)");
      return;
    }
    setUploading(true);
    try {
      const res = await projectsApi.uploadLogo(agencyId, projectId, file);
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

  const saveOverride = useMutation({
    mutationFn: () =>
      projectsApi.updateImageSettings(
        agencyId,
        projectId,
        enabled,
        position,
        logoUrl || null,
        mode,
        bannerColor || null
      ),
    onSuccess: () => {
      toast.success("Logo del progetto salvato");
      queryClient.invalidateQueries({ queryKey: ["project", agencyId, projectId] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const resetToAgency = useMutation({
    mutationFn: () =>
      projectsApi.updateImageSettings(agencyId, projectId, null, null, null, null, null),
    onSuccess: () => {
      toast.success("Ripristinato default agenzia");
      queryClient.invalidateQueries({ queryKey: ["project", agencyId, projectId] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const positions = [
    { value: 0, label: "In alto a sinistra" },
    { value: 1, label: "In alto a destra" },
    { value: 2, label: "In basso a sinistra" },
    { value: 3, label: "In basso a destra" },
  ];

  return (
    <Card data-testid="project-image-settings">
      <CardHeader>
        <CardTitle className="text-base flex items-center gap-2">
          <ImageIcon className="size-4" /> Logo overlay immagini (progetto)
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-4 min-w-0">
        <div className="rounded-md border p-3 space-y-1">
          <p className="text-xs text-muted-foreground">
            Default agenzia:{" "}
            <span className="font-medium">
              {agency?.enableLogoOverlay ? "Abilitato" : "Disabilitato"}
            </span>
          </p>
          <p className="text-xs text-muted-foreground">
            Stato attuale progetto:{" "}
            <span className="font-medium">
              {usingAgencyDefault
                ? "Usa default agenzia"
                : project?.enableLogoOverlay
                ? "Override abilitato"
                : "Override disabilitato"}
            </span>
          </p>
        </div>

        <div className="flex items-center justify-between">
          <Label className="text-sm">Applica logo alle immagini generate</Label>
          <Switch checked={enabled} onCheckedChange={setEnabled} />
        </div>
        <div className="space-y-2 min-w-0">
          <Label className="text-xs">Logo del progetto</Label>
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
              <span className="text-[10px] text-muted-foreground truncate min-w-0 flex-1">
                {logoUrl}
              </span>
            </div>
          )}
        </div>
        <div className="space-y-2">
          <Label className="text-xs">Modalità</Label>
          <div className="flex gap-2">
            <Button
              type="button"
              variant={mode === 0 ? "default" : "outline"}
              size="sm"
              className="flex-1"
              disabled={!enabled}
              onClick={() => setMode(0)}
            >
              Overlay
            </Button>
            <Button
              type="button"
              variant={mode === 1 ? "default" : "outline"}
              size="sm"
              className="flex-1"
              disabled={!enabled}
              onClick={() => setMode(1)}
            >
              Banner brand
            </Button>
          </div>
        </div>
        {mode === 0 && (
          <div>
            <Label className="text-xs">Posizione overlay</Label>
            <select
              className="h-9 rounded-md border bg-transparent px-3 text-sm w-full"
              value={position}
              onChange={(e) => setPosition(Number(e.target.value))}
              disabled={!enabled}
            >
              {positions.map((p) => (
                <option key={p.value} value={p.value}>
                  {p.label}
                </option>
              ))}
            </select>
          </div>
        )}
        {mode === 1 && (
          <div className="space-y-2">
            <Label className="text-xs">Colore banner brand</Label>
            <div className="flex gap-2 items-center">
              <input
                type="color"
                value={bannerColor}
                onChange={(e) => setBannerColor(e.target.value)}
                disabled={!enabled}
                className="h-9 w-14 rounded border disabled:opacity-50"
              />
              <Input
                type="text"
                value={bannerColor}
                onChange={(e) => setBannerColor(e.target.value)}
                placeholder="#003366"
                disabled={!enabled}
                className="flex-1"
              />
            </div>
            <p className="text-[10px] text-muted-foreground">
              Aggiunge un banner colorato sotto l'immagine con il logo all'interno.
            </p>
          </div>
        )}

        <div className="flex gap-2 pt-2 border-t">
          <Button
            size="sm"
            onClick={() => saveOverride.mutate()}
            disabled={saveOverride.isPending}
          >
            {saveOverride.isPending ? (
              <Loader2 className="size-4 animate-spin" />
            ) : null}{" "}
            Salva override
          </Button>
          <Button
            size="sm"
            variant="outline"
            onClick={() => resetToAgency.mutate()}
            disabled={resetToAgency.isPending || usingAgencyDefault}
          >
            {resetToAgency.isPending ? (
              <Loader2 className="size-4 animate-spin" />
            ) : null}{" "}
            Usa default agenzia
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}

function ProjectAnalyticsSection({
  agencyId,
  projectId,
  contents,
  formatDate,
}: {
  agencyId: string;
  projectId: string;
  contents: GeneratedContent[];
  formatDate: (iso: string) => string;
}) {
  const { data: stats, isLoading } = useQuery({
    queryKey: ["project-cost-stats", agencyId, projectId],
    queryFn: () => projectsApi.getCostStats(agencyId, projectId),
    refetchInterval: 60000,
  });

  const contentTypeLabel = (t: number) =>
    t === 1 ? "Blog" : t === 2 ? "Social" : t === 3 ? "Newsletter" : `Tipo ${t}`;

  const grouped = useMemo(() => {
    const map = new Map<number, { count: number; textCost: number; imageCost: number; total: number }>();
    for (const c of contents) {
      const textCost = c.aiGenerationCostUsd ?? 0;
      const imageCost = c.aiImageCostUsd ?? 0;
      const entry = map.get(c.contentType) ?? { count: 0, textCost: 0, imageCost: 0, total: 0 };
      entry.count += 1;
      entry.textCost += textCost;
      entry.imageCost += imageCost;
      entry.total += textCost + imageCost;
      map.set(c.contentType, entry);
    }
    return Array.from(map.entries())
      .map(([type, v]) => ({ type, ...v }))
      .sort((a, b) => b.total - a.total);
  }, [contents]);

  const perPost = useMemo(
    () =>
      [...contents]
        .map((c) => ({
          ...c,
          totalCost: (c.aiGenerationCostUsd ?? 0) + (c.aiImageCostUsd ?? 0),
        }))
        .filter((c) => c.totalCost > 0)
        .sort((a, b) => b.totalCost - a.totalCost),
    [contents]
  );

  const queryClient = useQueryClient();
  const resetAnalytics = useMutation({
    mutationFn: () => projectsApi.resetAnalytics(agencyId, projectId),
    onSuccess: () => {
      toast.success("Dati progetto azzerati");
      queryClient.invalidateQueries({ queryKey: ["project-cost-stats", agencyId, projectId] });
      queryClient.invalidateQueries({ queryKey: ["content", agencyId] });
      queryClient.invalidateQueries({ queryKey: ["project-calendar", agencyId, projectId] });
      queryClient.invalidateQueries({ queryKey: ["approvals", agencyId] });
      queryClient.invalidateQueries({ queryKey: ["notifications"] });
      queryClient.invalidateQueries({ queryKey: ["project", agencyId, projectId] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const avgCost = stats && stats.totalContents > 0 ? stats.totalCostUsd / stats.totalContents : 0;

  return (
    <div className="space-y-6" data-testid="project-analytics">
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle className="text-base flex items-center gap-2">
            <BarChart3 className="size-4" /> Aggregati costi AI
          </CardTitle>
          <Button
            size="sm"
            variant="outline"
            className="text-destructive"
            onClick={() => { if (confirm("Eliminare tutti i contenuti, calendario e notifiche del progetto? Questa azione è irreversibile.")) resetAnalytics.mutate(); }}
            disabled={resetAnalytics.isPending}
          >
            <Trash2 className="size-3.5 mr-1" /> Reset
          </Button>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <Skeleton className="h-24" />
          ) : !stats ? (
            <p className="text-sm text-muted-foreground">Nessun dato disponibile.</p>
          ) : (
            <div className="grid grid-cols-2 md:grid-cols-5 gap-4">
              <div>
                <p className="text-[10px] uppercase text-muted-foreground">Contenuti</p>
                <p className="text-xl font-semibold">{stats.totalContents}</p>
              </div>
              <div>
                <p className="text-[10px] uppercase text-muted-foreground">Costo totale</p>
                <p className="text-xl font-semibold">${stats.totalCostUsd.toFixed(4)}</p>
              </div>
              <div>
                <p className="text-[10px] uppercase text-muted-foreground">Testo</p>
                <p className="text-sm font-medium">${stats.totalTextCostUsd.toFixed(4)}</p>
              </div>
              <div>
                <p className="text-[10px] uppercase text-muted-foreground">Immagini</p>
                <p className="text-sm font-medium">${stats.totalImageCostUsd.toFixed(4)}</p>
              </div>
              <div>
                <p className="text-[10px] uppercase text-muted-foreground">Ultimi 30 gg</p>
                <p className="text-xl font-semibold">${stats.last30DaysCostUsd.toFixed(4)}</p>
              </div>
              <div>
                <p className="text-[10px] uppercase text-muted-foreground">Costo medio / post</p>
                <p className="text-sm font-medium">${avgCost.toFixed(4)}</p>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="text-base flex items-center gap-2">
            <FileText className="size-4" /> Costi per tipo di contenuto
          </CardTitle>
        </CardHeader>
        <CardContent>
          {grouped.length === 0 ? (
            <p className="text-sm text-muted-foreground">Nessun contenuto generato.</p>
          ) : (
            <div className="space-y-2">
              {grouped.map((g) => (
                <div
                  key={g.type}
                  className="flex items-center justify-between gap-3 p-3 rounded-md border"
                >
                  <div className="flex items-center gap-2">
                    <Badge variant="outline">{contentTypeLabel(g.type)}</Badge>
                    <span className="text-xs text-muted-foreground">{g.count} contenut{g.count === 1 ? "o" : "i"}</span>
                  </div>
                  <div className="flex items-center gap-4 text-xs">
                    <span className="text-muted-foreground">
                      Testo <span className="font-medium text-foreground">${g.textCost.toFixed(4)}</span>
                    </span>
                    <span className="text-muted-foreground">
                      Immagini <span className="font-medium text-foreground">${g.imageCost.toFixed(4)}</span>
                    </span>
                    <span className="font-semibold">${g.total.toFixed(4)}</span>
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="text-base flex items-center gap-2">
            <BarChart3 className="size-4" /> Costo per singolo contenuto
          </CardTitle>
        </CardHeader>
        <CardContent>
          {perPost.length === 0 ? (
            <p className="text-sm text-muted-foreground">Nessun contenuto con costi registrati.</p>
          ) : (
            <div className="space-y-1.5 max-h-[480px] overflow-y-auto">
              {perPost.map((c) => (
                <div
                  key={c.id}
                  className="flex items-center justify-between gap-3 px-3 py-2 rounded-md border hover:bg-muted/40"
                >
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium truncate">{c.title}</p>
                    <div className="flex items-center gap-2 text-[11px] text-muted-foreground">
                      <Badge variant="outline" className="text-[10px]">{contentTypeLabel(c.contentType)}</Badge>
                      <span>{formatDate(c.createdAt)}</span>
                    </div>
                  </div>
                  <div className="text-right text-xs shrink-0">
                    <p className="font-semibold text-sm">${c.totalCost.toFixed(4)}</p>
                    <p className="text-muted-foreground text-[10px]">
                      T ${(c.aiGenerationCostUsd ?? 0).toFixed(4)} · I ${(c.aiImageCostUsd ?? 0).toFixed(4)}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

function ProjectContentSourcesSection({ agencyId, projectId }: { agencyId: string; projectId: string }) {
  const queryClient = useQueryClient();
  const [newType, setNewType] = useState<number>(1);
  const [newUrl, setNewUrl] = useState<string>("");
  const [newName, setNewName] = useState<string>("");

  const { data: sources, isLoading } = useQuery({
    queryKey: ["project-sources", agencyId, projectId],
    queryFn: async () => {
      const res = await apiClient.get<ApiResponse<ContentSourceItem[]>>(
        `/api/v1/agencies/${agencyId}/sources?projectId=${projectId}`
      );
      return res.data ?? [];
    },
  });

  const create = useMutation({
    mutationFn: () =>
      apiClient.post<ApiResponse<ContentSourceItem>>(
        `/api/v1/agencies/${agencyId}/sources`,
        { type: newType, url: newUrl, name: newName || null, projectId }
      ),
    onSuccess: () => {
      toast.success("Fonte aggiunta");
      setNewUrl("");
      setNewName("");
      queryClient.invalidateQueries({ queryKey: ["project-sources", agencyId, projectId] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const remove = useMutation({
    mutationFn: (id: string) => apiClient.delete(`/api/v1/agencies/${agencyId}/sources/${id}`),
    onSuccess: () => {
      toast.success("Fonte rimossa");
      queryClient.invalidateQueries({ queryKey: ["project-sources", agencyId, projectId] });
    },
  });

  const typeLabel = (t: number) => (t === 1 ? "RSS" : t === 2 ? "Website" : t === 3 ? "Social" : String(t));

  return (
    <Card data-testid="project-sources-section">
      <CardHeader>
        <CardTitle className="text-base flex items-center gap-2">
          <Rss className="size-4" /> Fonti contenuto del progetto
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-3">
        {isLoading && <Skeleton className="h-12" />}
        {!isLoading && !sources?.length && (
          <p className="text-sm text-muted-foreground">Nessuna fonte per questo progetto.</p>
        )}
        {sources?.map((s) => (
          <div key={s.id} className="flex items-center gap-2 p-2 rounded-md border">
            <Badge variant="outline" className="text-[10px]">{typeLabel(s.type)}</Badge>
            <div className="flex-1 min-w-0">
              {s.name && <p className="text-sm font-medium truncate">{s.name}</p>}
              <p className="text-xs text-muted-foreground truncate">{s.url}</p>
            </div>
            <Button size="sm" variant="ghost" onClick={() => remove.mutate(s.id)} className="text-destructive">
              <Trash2 className="size-4" />
            </Button>
          </div>
        ))}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-2 pt-2 border-t">
          <select
            className="h-9 rounded-md border bg-transparent px-3 text-sm"
            value={newType}
            onChange={(e) => setNewType(Number(e.target.value))}
          >
            <option value={1}>RSS</option>
            <option value={2}>Website</option>
            <option value={3}>Social</option>
          </select>
          <Input placeholder="https://example.com/feed" value={newUrl} onChange={(e) => setNewUrl(e.target.value)} />
          <Input placeholder="Nome (opzionale)" value={newName} onChange={(e) => setNewName(e.target.value)} />
          <Button size="sm" onClick={() => create.mutate()} disabled={!newUrl || create.isPending}>
            {create.isPending ? <Loader2 className="size-4 animate-spin" /> : null} Aggiungi
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}

const SCHEDULE_AGENT_LABELS: Record<number, string> = {
  1: "Content Writer (Blog)",
  2: "Social Manager",
  3: "Newsletter",
};

const SCHEDULE_DAY_FLAGS = [
  { flag: DayOfWeekFlag.Monday, label: "Lun" },
  { flag: DayOfWeekFlag.Tuesday, label: "Mar" },
  { flag: DayOfWeekFlag.Wednesday, label: "Mer" },
  { flag: DayOfWeekFlag.Thursday, label: "Gio" },
  { flag: DayOfWeekFlag.Friday, label: "Ven" },
  { flag: DayOfWeekFlag.Saturday, label: "Sab" },
  { flag: DayOfWeekFlag.Sunday, label: "Dom" },
];

const ALL_PLATFORMS = ["Twitter", "LinkedIn", "Instagram", "Facebook"] as const;

const PUBLISH_CONTENT_TYPE_LABELS: Record<number, string> = {
  1: "Blog",
  2: "Social",
  3: "Newsletter",
};

function formatScheduleDays(days: number): string {
  if (days === DayOfWeekFlag.EveryDay) return "Ogni giorno";
  if (days === DayOfWeekFlag.Weekdays) return "Lun-Ven";
  if (days === DayOfWeekFlag.Weekend) return "Sab-Dom";
  const names: string[] = [];
  for (const { flag, label } of SCHEDULE_DAY_FLAGS) {
    if (days & flag) names.push(label);
  }
  return names.join(", ") || "Nessun giorno";
}

function ProjectSchedulesSection({
  agencyId,
  projectId,
  project,
}: {
  agencyId: string;
  projectId: string;
  project: any;
}) {
  const queryClient = useQueryClient();
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [name, setName] = useState("");
  const [scheduleType, setScheduleType] = useState<number>(1);
  const [agentType, setAgentType] = useState<number>(1);
  const [publishContentType, setPublishContentType] = useState<number | null>(null);
  const [maxPostsPerPlatform, setMaxPostsPerPlatform] = useState<number>(1);
  const [days, setDays] = useState<number>(DayOfWeekFlag.Weekdays);
  const [timeOfDay, setTimeOfDay] = useState("09:00");
  const [timeZone, setTimeZone] = useState("Europe/Rome");
  const [input, setInput] = useState("");
  // Per-schedule overrides
  const [overridePlatforms, setOverridePlatforms] = useState<string[] | null>(null);
  const [overrideApprovalMode, setOverrideApprovalMode] = useState<number | null>(null);
  const [overrideMinScore, setOverrideMinScore] = useState<number>(70);
  const [overrideAutoSchedule, setOverrideAutoSchedule] = useState<boolean | null>(null);

  const { data: schedulesRes, isLoading } = useQuery({
    queryKey: ["project-schedules", agencyId, projectId],
    queryFn: () => schedulesApi.list(agencyId),
  });

  const allSchedules: ContentSchedule[] = schedulesRes?.data || [];
  const schedules = allSchedules.filter((s) => s.projectId === projectId);

  const resetForm = () => {
    setEditingId(null);
    setName("");
    setScheduleType(1);
    setAgentType(1);
    setPublishContentType(null);
    setMaxPostsPerPlatform(1);
    setDays(DayOfWeekFlag.Weekdays);
    setTimeOfDay("09:00");
    setTimeZone("Europe/Rome");
    setInput("");
    setOverridePlatforms(null);
    setOverrideApprovalMode(null);
    setOverrideMinScore(70);
    setOverrideAutoSchedule(null);
  };

  const createSchedule = useMutation({
    mutationFn: () =>
      schedulesApi.create(agencyId, {
        name,
        days,
        timeOfDay,
        timeZone,
        scheduleType,
        agentType,
        publishContentType: scheduleType === 2 ? publishContentType : null,
        maxPostsPerPlatform: scheduleType === 2 ? maxPostsPerPlatform : null,
        input: input || null,
        projectId,
        enabledSocialPlatforms: overridePlatforms === null ? null : overridePlatforms.join(","),
        approvalMode: overrideApprovalMode,
        autoApproveMinScore: overrideApprovalMode === 3 ? overrideMinScore : null,
        autoScheduleOnApproval: overrideAutoSchedule,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["project-schedules", agencyId, projectId] });
      toast.success("Programmazione creata");
      setShowForm(false);
      resetForm();
    },
    onError: (e: any) => toast.error(e?.message || "Errore"),
  });

  const updateSchedule = useMutation({
    mutationFn: () =>
      schedulesApi.update(agencyId, editingId!, {
        name,
        days,
        timeOfDay,
        timeZone,
        scheduleType,
        agentType,
        publishContentType: scheduleType === 2 ? publishContentType : null,
        maxPostsPerPlatform: scheduleType === 2 ? maxPostsPerPlatform : null,
        input: input || null,
        projectId,
        isActive: true,
        enabledSocialPlatforms: overridePlatforms === null ? null : overridePlatforms.join(","),
        approvalMode: overrideApprovalMode,
        autoApproveMinScore: overrideApprovalMode === 3 ? overrideMinScore : null,
        autoScheduleOnApproval: overrideAutoSchedule,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["project-schedules", agencyId, projectId] });
      toast.success("Programmazione aggiornata");
      setShowForm(false);
      resetForm();
    },
    onError: (e: any) => toast.error(e?.message || "Errore"),
  });

  const toggleSchedule = useMutation({
    mutationFn: (s: ContentSchedule) =>
      schedulesApi.update(agencyId, s.id, {
        name: s.name,
        days: s.days,
        timeOfDay: s.timeOfDay,
        timeZone: s.timeZone,
        scheduleType: s.scheduleType,
        agentType: s.agentType,
        publishContentType: s.publishContentType ?? null,
        maxPostsPerPlatform: s.maxPostsPerPlatform ?? null,
        input: s.input,
        projectId: s.projectId ?? null,
        isActive: !s.isActive,
        enabledSocialPlatforms: s.enabledSocialPlatforms ?? null,
        approvalMode: s.approvalMode ?? null,
        autoApproveMinScore: s.autoApproveMinScore ?? null,
        autoScheduleOnApproval: s.autoScheduleOnApproval ?? null,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["project-schedules", agencyId, projectId] });
    },
    onError: (e: any) => toast.error(e?.message || "Errore"),
  });

  const deleteSchedule = useMutation({
    mutationFn: (id: string) => schedulesApi.delete(agencyId, id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["project-schedules", agencyId, projectId] });
      toast.success("Eliminata");
    },
    onError: (e: any) => toast.error(e?.message || "Errore"),
  });

  const runNow = useMutation({
    mutationFn: (id: string) =>
      apiClient.post(`/api/v1/agencies/${agencyId}/schedules/${id}/run-now`),
    onSuccess: () => toast.success("Esecuzione avviata (entro 1 minuto)"),
    onError: (e: any) => toast.error(e?.message || "Errore"),
  });

  const startEdit = (s: ContentSchedule) => {
    setEditingId(s.id);
    setName(s.name);
    setScheduleType(s.scheduleType ?? 1);
    setAgentType(s.agentType);
    setPublishContentType(s.publishContentType ?? null);
    setMaxPostsPerPlatform(s.maxPostsPerPlatform ?? 1);
    setDays(s.days);
    setTimeOfDay(s.timeOfDay.slice(0, 5));
    setTimeZone(s.timeZone);
    setInput(s.input || "");
    setOverridePlatforms(
      s.enabledSocialPlatforms
        ? s.enabledSocialPlatforms.split(",").map((p) => p.trim()).filter(Boolean)
        : null
    );
    setOverrideApprovalMode(s.approvalMode ?? null);
    setOverrideMinScore(s.autoApproveMinScore ?? 70);
    setOverrideAutoSchedule(s.autoScheduleOnApproval ?? null);
    setShowForm(true);
  };

  const agentToContentType = (at: number) => at === 1 ? 1 : at === 2 ? 2 : at === 3 ? 3 : null;

  const submit = () => {
    if (!name.trim()) {
      toast.error("Nome obbligatorio");
      return;
    }
    if (scheduleType === 2) {
      const targetCt = publishContentType;
      const conflicting = schedules.find(
        (s) =>
          s.id !== editingId &&
          (s.scheduleType ?? 1) === 1 &&
          s.autoScheduleOnApproval === true &&
          (targetCt === null || agentToContentType(s.agentType) === targetCt)
      );
      if (conflicting) {
        toast.error(
          `La programmazione "${conflicting.name}" ha già auto-schedule abilitato per questo tipo di contenuto. Disabilitalo prima di creare una pubblicazione separata.`
        );
        return;
      }
    }
    if (editingId) updateSchedule.mutate();
    else createSchedule.mutate();
  };

  const toggleDay = (flag: number) => setDays((d) => d ^ flag);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h3 className="text-sm font-semibold">Programmazioni del progetto</h3>
          <p className="text-xs text-muted-foreground">
            Programmazioni per generare e pubblicare contenuti di questo progetto.
          </p>
        </div>
        <Button
          size="sm"
          onClick={() => { if (showForm) { setShowForm(false); resetForm(); } else setShowForm(true); }}
        >
          {showForm ? <><X className="size-4" /> Chiudi</> : <><Plus className="size-4" /> Nuova</>}
        </Button>
      </div>

      {showForm && (
        <Card>
          <CardContent className="pt-6 space-y-4">
            <div className="space-y-2">
              <Label>Nome</Label>
              <Input value={name} onChange={(e) => setName(e.target.value)} placeholder="es. Post social giornalieri" />
            </div>
            <div className="space-y-2">
              <Label>Tipo programmazione</Label>
              <div className="flex items-center gap-2">
                <Button
                  size="sm"
                  type="button"
                  variant={scheduleType === 1 ? "default" : "outline"}
                  onClick={() => setScheduleType(1)}
                >
                  Generazione
                </Button>
                <Button
                  size="sm"
                  type="button"
                  variant={scheduleType === 2 ? "default" : "outline"}
                  onClick={() => setScheduleType(2)}
                >
                  Pubblicazione
                </Button>
              </div>
              <p className="text-[11px] text-muted-foreground">
                {scheduleType === 1
                  ? "Lancia un agente AI per generare nuovi contenuti."
                  : "Pubblica automaticamente i contenuti approvati in coda."}
              </p>
            </div>
            {scheduleType === 1 ? (
              <div className="space-y-2">
                <Label>Tipo agente</Label>
                <select
                  value={agentType}
                  onChange={(e) => setAgentType(Number(e.target.value))}
                  className="flex h-9 w-full rounded-lg border border-input bg-transparent px-3 py-1 text-sm"
                >
                  {Object.entries(SCHEDULE_AGENT_LABELS).map(([k, v]) => (
                    <option key={k} value={k}>{v}</option>
                  ))}
                </select>
                {agentType === 2 && (
                  <p className="text-[11px] text-muted-foreground">
                    Le piattaforme selezionate nella sezione in alto saranno usate da questa programmazione.
                  </p>
                )}
              </div>
            ) : (
              <div className="space-y-2">
                <Label>Tipo contenuto da pubblicare</Label>
                <select
                  value={publishContentType ?? ""}
                  onChange={(e) => setPublishContentType(e.target.value === "" ? null : Number(e.target.value))}
                  className="flex h-9 w-full rounded-lg border border-input bg-transparent px-3 py-1 text-sm"
                >
                  <option value="">Tutti i contenuti approvati</option>
                  <option value={2}>Social Post</option>
                  <option value={3}>Newsletter</option>
                  <option value={1}>Blog Post</option>
                </select>
                <p className="text-[11px] text-muted-foreground">
                  Pubblica tutti i contenuti approvati del tipo selezionato per questo progetto.
                </p>
              </div>
            )}
            {scheduleType === 2 && (
              <div className="space-y-2">
                <Label>Max post per piattaforma</Label>
                <Input
                  type="number"
                  min={1}
                  max={20}
                  value={maxPostsPerPlatform}
                  onChange={(e) => setMaxPostsPerPlatform(Math.max(1, Number(e.target.value)))}
                />
                <p className="text-[11px] text-muted-foreground">
                  Quanti contenuti pubblicare per piattaforma ad ogni esecuzione. I rimanenti verranno distribuiti nelle esecuzioni successive.
                </p>
              </div>
            )}
            {((scheduleType === 1 && agentType === 2) || (scheduleType === 2 && (publishContentType === 2 || publishContentType === null))) && (
              <div className="space-y-2">
                <Label>Piattaforme social</Label>
                <p className="text-[11px] text-muted-foreground">
                  Seleziona le piattaforme su cui pubblicare i contenuti social.
                </p>
                <div className="flex flex-wrap gap-2">
                  {ALL_PLATFORMS.map((p) => {
                    const selected = overridePlatforms ?? [...ALL_PLATFORMS];
                    const active = selected.includes(p);
                    return (
                      <button
                        key={p}
                        type="button"
                        onClick={() => {
                          const current = overridePlatforms ?? [...ALL_PLATFORMS];
                          setOverridePlatforms(
                            active ? current.filter((x) => x !== p) : [...current, p]
                          );
                        }}
                        className={`px-3 py-1.5 rounded-md text-sm font-medium border transition-colors ${
                          active
                            ? "bg-primary text-primary-foreground border-primary"
                            : "bg-muted text-muted-foreground border-transparent hover:bg-muted/80"
                        }`}
                      >
                        {p}
                      </button>
                    );
                  })}
                </div>
              </div>
            )}
            <div className="space-y-2">
              <Label>Giorni</Label>
              <div className="flex flex-wrap gap-2">
                {SCHEDULE_DAY_FLAGS.map(({ flag, label }) => (
                  <button
                    key={flag}
                    type="button"
                    onClick={() => toggleDay(flag)}
                    className={`px-3 py-1.5 rounded-md text-sm font-medium border transition-colors ${
                      days & flag
                        ? "bg-primary text-primary-foreground border-primary"
                        : "bg-muted text-muted-foreground border-transparent hover:bg-muted/80"
                    }`}
                  >
                    {label}
                  </button>
                ))}
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Orario</Label>
                <Input type="time" value={timeOfDay} onChange={(e) => setTimeOfDay(e.target.value)} />
              </div>
              <div className="space-y-2">
                <Label>Fuso orario</Label>
                <select
                  value={timeZone}
                  onChange={(e) => setTimeZone(e.target.value)}
                  className="flex h-9 w-full rounded-lg border border-input bg-transparent px-3 py-1 text-sm"
                >
                  <option value="Europe/Rome">Europe/Rome</option>
                  <option value="Europe/London">Europe/London</option>
                  <option value="America/New_York">America/New_York</option>
                  <option value="America/Los_Angeles">America/Los_Angeles</option>
                </select>
              </div>
            </div>
            <div className="space-y-2">
              <Label>Input aggiuntivo (opzionale)</Label>
              <Input value={input} onChange={(e) => setInput(e.target.value)} placeholder="es. Focus su AI" />
            </div>

            {scheduleType === 1 && (
              <details className="rounded-lg border border-border/60 bg-muted/30 px-3 py-2">
                <summary className="cursor-pointer text-sm font-medium select-none">
                  Override avanzati (opzionale)
                </summary>
                <div className="mt-4 space-y-4">
                  <p className="text-[11px] text-muted-foreground">
                    Solo per questa programmazione. Lasciati vuoti, usano le impostazioni del progetto/agenzia.
                  </p>

                  <div className="space-y-2">
                    <Label className="text-xs">Modalità approvazione (override)</Label>
                    <select
                      value={overrideApprovalMode ?? ""}
                      onChange={(e) =>
                        setOverrideApprovalMode(e.target.value === "" ? null : Number(e.target.value))
                      }
                      className="flex h-9 w-full rounded-lg border border-input bg-transparent px-3 py-1 text-sm"
                    >
                      <option value="">Default progetto</option>
                      <option value={ApprovalMode.Manual}>Manuale</option>
                      <option value={ApprovalMode.AutoApprove}>Auto-approva tutto</option>
                      <option value={ApprovalMode.AutoApproveAboveScore}>Auto-approva se score sufficiente</option>
                    </select>
                    {overrideApprovalMode === ApprovalMode.AutoApproveAboveScore && (
                      <div className="space-y-1">
                        <Label className="text-xs">Score minimo</Label>
                        <Input
                          type="number"
                          min={0}
                          max={100}
                          value={overrideMinScore}
                          onChange={(e) => setOverrideMinScore(Number(e.target.value))}
                        />
                      </div>
                    )}
                  </div>

                </div>
              </details>
            )}

            {scheduleType === 1 && (
              <div className="space-y-2">
                <Label>Auto-schedule pubblicazione</Label>
                <p className="text-[11px] text-muted-foreground">
                  Se abilitato, i contenuti approvati da questa programmazione vengono inseriti automaticamente nel calendario.
                </p>
                <div className="flex items-center gap-2">
                  <Button
                    size="sm"
                    type="button"
                    variant={overrideAutoSchedule === null ? "default" : "outline"}
                    onClick={() => setOverrideAutoSchedule(null)}
                  >
                    Default agenzia
                  </Button>
                  <Button
                    size="sm"
                    type="button"
                    variant={overrideAutoSchedule === true ? "default" : "outline"}
                    onClick={() => setOverrideAutoSchedule(true)}
                  >
                    Abilitato
                  </Button>
                  <Button
                    size="sm"
                    type="button"
                    variant={overrideAutoSchedule === false ? "default" : "outline"}
                    onClick={() => setOverrideAutoSchedule(false)}
                  >
                    Disabilitato
                  </Button>
                </div>
              </div>
            )}

            <div className="flex gap-2">
              <Button onClick={submit} disabled={createSchedule.isPending || updateSchedule.isPending}>
                {(createSchedule.isPending || updateSchedule.isPending) ? <Loader2 className="size-4 animate-spin" /> : <Calendar className="size-4" />}
                {editingId ? "Aggiorna" : "Crea"}
              </Button>
              <Button variant="ghost" onClick={() => { setShowForm(false); resetForm(); }}>Annulla</Button>
            </div>
          </CardContent>
        </Card>
      )}

      {isLoading ? (
        <Skeleton className="h-24" />
      ) : schedules.length === 0 ? (
        <Card className="text-center py-10">
          <CardContent className="space-y-2">
            <Calendar className="size-8 text-muted-foreground mx-auto" />
            <p className="text-sm text-muted-foreground">Nessuna programmazione per questo progetto.</p>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-3">
          {schedules.map((s) => (
            <Card key={s.id}>
              <CardContent className="pt-6 flex items-center justify-between flex-wrap gap-3">
                <div className="flex items-center gap-3">
                  <div className={`size-10 rounded-lg flex items-center justify-center ${s.isActive ? "bg-primary/10" : "bg-muted"}`}>
                    <Calendar className={`size-4 ${s.isActive ? "text-primary" : "text-muted-foreground"}`} />
                  </div>
                  <div>
                    <h4 className="font-medium text-sm">{s.name}</h4>
                    <p className="text-xs text-muted-foreground">
                      {s.scheduleType === 2
                        ? `Pubblicazione${s.publishContentType ? ` · ${PUBLISH_CONTENT_TYPE_LABELS[s.publishContentType] ?? "Tipo " + s.publishContentType}` : " · Tutti"}`
                        : (SCHEDULE_AGENT_LABELS[s.agentType] || `Agent ${s.agentType}`)
                      } · {formatScheduleDays(s.days)} alle {s.timeOfDay.slice(0, 5)}
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-1">
                  <Badge variant="outline" className={s.scheduleType === 2 ? "border-amber-500 text-amber-600" : "border-blue-500 text-blue-600"}>
                    {s.scheduleType === 2 ? "Pubblica" : "Genera"}
                  </Badge>
                  <Badge variant={s.isActive ? "default" : "secondary"}>{s.isActive ? "Attiva" : "In pausa"}</Badge>
                  <Button variant="ghost" size="icon-sm" title="Esegui ora" onClick={() => runNow.mutate(s.id)}><Zap className="size-4" /></Button>
                  <Button variant="ghost" size="icon-sm" title="Modifica" onClick={() => startEdit(s)}><Pencil className="size-4" /></Button>
                  <Button variant="ghost" size="icon-sm" onClick={() => toggleSchedule.mutate(s)}>
                    {s.isActive ? <Pause className="size-4" /> : <Play className="size-4" />}
                  </Button>
                  <Button variant="ghost" size="icon-sm" className="text-destructive hover:text-destructive" onClick={() => deleteSchedule.mutate(s.id)}>
                    <Trash2 className="size-4" />
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}

function ProjectNotificationsSection({ agencyId, projectId }: { agencyId: string; projectId: string }) {
  const queryClient = useQueryClient();
  const [activeTab, setActiveTab] = useState<"notifiche" | "jobs">("notifiche");

  const { data, isLoading } = useQuery({
    queryKey: ["notifications", "project", projectId],
    queryFn: async () => {
      const res = await notificationsApi.list(false, 200);
      const all = res.data?.items ?? [];
      return all.filter((n: NotificationItem) => n.projectId === projectId);
    },
    refetchInterval: 15000,
  });

  const { data: jobsData, isLoading: jobsLoading } = useQuery({
    queryKey: ["jobs", "project", projectId],
    queryFn: async () => {
      const res = await jobsApi.list(agencyId, 50);
      return (res.data ?? []).filter((j: JobListItem) => j.projectId === projectId);
    },
  });

  const publishNotifications = (data ?? []).filter(
    (n) => n.type.includes("publish")
  );

  const markRead = useMutation({
    mutationFn: (id: string) => notificationsApi.markRead(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["notifications"] });
    },
  });

  const markAllRead = useMutation({
    mutationFn: async () => {
      const unreadIds = (data ?? []).filter((n) => !n.read).map((n) => n.id);
      for (const id of unreadIds) await notificationsApi.markRead(id);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["notifications"] });
      toast.success("Tutte le notifiche segnate come lette");
    },
  });

  const notifications = data ?? [];
  const jobs = jobsData ?? [];
  const unread = notifications.filter((n) => !n.read);

  if (isLoading) return <Skeleton className="h-40" />;

  const typeIcon = (type: string) => {
    if (type.includes("success")) return <Badge variant="default" className="text-[10px]">Successo</Badge>;
    if (type.includes("failed")) return <Badge variant="destructive" className="text-[10px]">Errore</Badge>;
    if (type.includes("completed")) return <Badge variant="default" className="text-[10px]">Completato</Badge>;
    return <Badge variant="outline" className="text-[10px]">{type}</Badge>;
  };

  const jobStatusBadge = (status: string) => {
    if (status === "Completed") return <Badge variant="default" className="text-[10px]">Completato</Badge>;
    if (status === "Failed") return <Badge variant="destructive" className="text-[10px]">Fallito</Badge>;
    if (status === "Running" || status === "Processing") return <Badge variant="secondary" className="text-[10px]">In corso</Badge>;
    if (status === "Queued") return <Badge variant="outline" className="text-[10px]">In coda</Badge>;
    return <Badge variant="outline" className="text-[10px]">{status}</Badge>;
  };

  return (
    <div className="space-y-4">
      {/* Tab buttons */}
      <div className="flex border-b border-border">
        <button
          type="button"
          onClick={() => setActiveTab("notifiche")}
          className={`flex items-center gap-2 px-4 py-2 text-sm font-medium transition-colors -mb-px ${
            activeTab === "notifiche"
              ? "border-b-2 border-primary text-primary"
              : "text-muted-foreground hover:text-foreground"
          }`}
        >
          <Bell className="size-4" />
          Notifiche
          {unread.length > 0 && (
            <Badge variant="destructive" className="text-[10px]">{unread.length}</Badge>
          )}
        </button>
        <button
          type="button"
          onClick={() => setActiveTab("jobs")}
          className={`flex items-center gap-2 px-4 py-2 text-sm font-medium transition-colors -mb-px ${
            activeTab === "jobs"
              ? "border-b-2 border-primary text-primary"
              : "text-muted-foreground hover:text-foreground"
          }`}
        >
          <Bot className="size-4" />
          Jobs
        </button>
      </div>

      {/* Notifiche tab content */}
      {activeTab === "notifiche" && (
        <div>
          {unread.length > 0 && (
            <div className="flex justify-end mb-3">
              <Button size="sm" variant="outline" onClick={() => markAllRead.mutate()} disabled={markAllRead.isPending}>
                <CheckCheck className="size-4 mr-1" /> Segna tutte come lette
              </Button>
            </div>
          )}

          {notifications.length === 0 ? (
            <Card>
              <CardContent className="pt-5 text-sm text-muted-foreground">
                Nessuna notifica per questo progetto.
              </CardContent>
            </Card>
          ) : (
            <div className="space-y-2">
              {notifications.map((n) => (
                <Card key={n.id} className={n.read ? "opacity-60" : ""}>
                  <CardContent className="pt-4 pb-4">
                    <div className="flex items-start justify-between gap-3">
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2 flex-wrap">
                          <span className="font-medium text-sm">{n.title}</span>
                          {typeIcon(n.type)}
                        </div>
                        {n.body && (
                          <p className={`text-xs mt-1 line-clamp-3 ${n.type.includes("failed") ? "text-destructive font-medium" : "text-muted-foreground"}`}>
                            {n.body}
                          </p>
                        )}
                        <p className="text-[10px] text-muted-foreground mt-1">
                          {new Date(n.createdAt).toLocaleString("it-IT")}
                        </p>
                      </div>
                      {!n.read && (
                        <Button size="sm" variant="ghost" className="shrink-0" onClick={() => markRead.mutate(n.id)}>
                          <CheckCheck className="size-4" />
                        </Button>
                      )}
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Jobs tab content */}
      {activeTab === "jobs" && (
        <div className="space-y-4">
          {/* Agent Jobs */}
          {jobsLoading ? <Skeleton className="h-32" /> : jobs.length > 0 && (
            <div className="space-y-2">
              <p className="text-xs font-semibold text-muted-foreground uppercase">Generazione</p>
              {jobs.map((j) => (
                <Card key={j.id}>
                  <CardContent className="pt-4 pb-4">
                    <div className="flex items-center justify-between gap-3">
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2 flex-wrap">
                          <span className="font-medium text-sm">{j.agentType}</span>
                          {jobStatusBadge(j.status)}
                        </div>
                        {j.errorMessage && <p className="text-xs text-destructive mt-1 line-clamp-2">{j.errorMessage}</p>}
                        {j.output && !j.errorMessage && <p className="text-xs text-muted-foreground mt-1 line-clamp-1">{j.output}</p>}
                        <p className="text-[10px] text-muted-foreground mt-1">
                          {new Date(j.createdAt).toLocaleString("it-IT")}
                          {j.completedAt && ` · Completato: ${new Date(j.completedAt).toLocaleString("it-IT")}`}
                        </p>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}

          {/* Publish results */}
          {publishNotifications.length > 0 && (
            <div className="space-y-2">
              <p className="text-xs font-semibold text-muted-foreground uppercase">Pubblicazione</p>
              {publishNotifications.map((n) => (
                <Card key={n.id} className={n.read ? "opacity-60" : ""}>
                  <CardContent className="pt-4 pb-4">
                    <div className="flex items-start gap-3">
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2 flex-wrap">
                          <span className="font-medium text-sm">{n.title}</span>
                          {typeIcon(n.type)}
                        </div>
                        {n.body && (
                          <p className={`text-xs mt-1 line-clamp-3 ${n.type.includes("failed") ? "text-destructive" : "text-muted-foreground"}`}>
                            {n.body}
                          </p>
                        )}
                        <p className="text-[10px] text-muted-foreground mt-1">
                          {new Date(n.createdAt).toLocaleString("it-IT")}
                        </p>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}

          {jobs.length === 0 && publishNotifications.length === 0 && (
            <Card><CardContent className="pt-5 text-sm text-muted-foreground">Nessun job per questo progetto.</CardContent></Card>
          )}
        </div>
      )}
    </div>
  );
}
