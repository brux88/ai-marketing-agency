"use client";

import { useParams } from "next/navigation";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { agenciesApi } from "@/lib/api/agencies.api";
import { newsletterApi } from "@/lib/api/newsletter.api";
import { projectsApi } from "@/lib/api/projects.api";
import { apiClient } from "@/lib/api/client";
import { EmailProviderType } from "@/types/api";
import type { NewsletterSubscriber, EmailConnectorDto } from "@/types/api";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Mail, Loader2, Newspaper, Sparkles, Target, Settings2, Users, Plus, Trash2, Send, Copy, Code2 } from "lucide-react";
import { toast } from "sonner";

export default function NewsletterPage() {
  const { agencyId } = useParams<{ agencyId: string }>();
  const queryClient = useQueryClient();
  const [generating, setGenerating] = useState(false);

  // Config form state
  const [showConfig, setShowConfig] = useState(false);
  const [configProjectId, setConfigProjectId] = useState<string>("");
  const [providerType, setProviderType] = useState<number>(EmailProviderType.Smtp);
  const [smtpHost, setSmtpHost] = useState("");
  const [smtpPort, setSmtpPort] = useState("587");
  const [smtpUsername, setSmtpUsername] = useState("");
  const [smtpPassword, setSmtpPassword] = useState("");
  const [apiKey, setApiKey] = useState("");
  const [fromEmail, setFromEmail] = useState("");
  const [fromName, setFromName] = useState("");

  const resetConfigForm = () => {
    setConfigProjectId("");
    setProviderType(EmailProviderType.Smtp);
    setSmtpHost("");
    setSmtpPort("587");
    setSmtpUsername("");
    setSmtpPassword("");
    setApiKey("");
    setFromEmail("");
    setFromName("");
  };

  const loadConfigIntoForm = (c: EmailConnectorDto) => {
    setConfigProjectId(c.projectId ?? "");
    setProviderType(c.providerType);
    setSmtpHost(c.smtpHost ?? "");
    setSmtpPort(c.smtpPort?.toString() ?? "587");
    setSmtpUsername(c.smtpUsername ?? "");
    setSmtpPassword("");
    setApiKey("");
    setFromEmail(c.fromEmail);
    setFromName(c.fromName);
    setShowConfig(true);
  };

  // Subscriber form state
  const [showAddSub, setShowAddSub] = useState(false);
  const [subEmail, setSubEmail] = useState("");
  const [subName, setSubName] = useState("");

  const { data: agencyData } = useQuery({
    queryKey: ["agency", agencyId],
    queryFn: () => agenciesApi.get(agencyId),
  });

  const { data: configData, isLoading: configLoading } = useQuery({
    queryKey: ["newsletter-config", agencyId],
    queryFn: () => newsletterApi.getConfig(agencyId),
  });

  const { data: projects } = useQuery({
    queryKey: ["projects", agencyId],
    queryFn: () => projectsApi.list(agencyId),
  });

  const { data: subsData, isLoading: subsLoading } = useQuery({
    queryKey: ["newsletter-subscribers", agencyId],
    queryFn: () => newsletterApi.getSubscribers(agencyId),
  });

  const agency = agencyData?.data;
  const configs: EmailConnectorDto[] = configData?.data ?? [];
  const agencyDefaultConfig = configs.find((c) => !c.projectId) ?? null;
  const subscribers: NewsletterSubscriber[] = subsData?.data ?? [];
  const activeSubscribers = subscribers.filter((s) => s.isActive);

  const saveConfigMutation = useMutation({
    mutationFn: () =>
      newsletterApi.saveConfig(agencyId, {
        providerType,
        smtpHost: providerType === EmailProviderType.Smtp ? smtpHost : undefined,
        smtpPort: providerType === EmailProviderType.Smtp ? Number(smtpPort) : undefined,
        smtpUsername: providerType === EmailProviderType.Smtp ? smtpUsername : undefined,
        smtpPassword: providerType === EmailProviderType.Smtp ? smtpPassword : undefined,
        apiKey: providerType === EmailProviderType.SendGrid ? apiKey : undefined,
        fromEmail,
        fromName,
        projectId: configProjectId || null,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["newsletter-config", agencyId] });
      toast.success("Configurazione email salvata!");
      setShowConfig(false);
      resetConfigForm();
    },
    onError: () => toast.error("Errore nel salvataggio."),
  });

  const addSubMutation = useMutation({
    mutationFn: () => newsletterApi.addSubscriber(agencyId, { email: subEmail, name: subName || undefined }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["newsletter-subscribers", agencyId] });
      toast.success("Iscritto aggiunto!");
      setSubEmail("");
      setSubName("");
      setShowAddSub(false);
    },
    onError: () => toast.error("Errore nell'aggiunta."),
  });

  const removeSubMutation = useMutation({
    mutationFn: (subscriberId: string) => newsletterApi.removeSubscriber(agencyId, subscriberId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["newsletter-subscribers", agencyId] });
      toast.success("Iscritto rimosso.");
    },
  });

  const handleGenerate = async () => {
    if (!agency?.defaultLlmProviderKeyId) {
      toast.error("Configura prima una chiave LLM nelle impostazioni dell'agenzia");
      return;
    }
    setGenerating(true);
    try {
      await apiClient.post(`/api/v1/agencies/${agencyId}/agents/newsletter/run`, {});
      toast.success("Generazione newsletter avviata!");
    } catch (err: any) {
      toast.error(err?.message || "Errore durante la generazione");
    } finally {
      setGenerating(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold tracking-tight">Newsletter</h2>
          <p className="text-muted-foreground text-sm mt-1">
            Genera newsletter con AI e inviale agli iscritti
          </p>
        </div>
        <Button onClick={handleGenerate} disabled={generating}>
          {generating ? (
            <><Loader2 className="size-4 animate-spin" /> Generazione...</>
          ) : (
            <><Mail className="size-4" /> Genera newsletter</>
          )}
        </Button>
      </div>

      {/* Public subscription URL */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <Code2 className="size-4" /> Link iscrizione pubblica
          </CardTitle>
          <CardDescription>
            Condividi questo URL o incorpora il form HTML nel tuo sito per raccogliere iscritti.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label>URL di iscrizione</Label>
            <div className="flex gap-2">
              <Input
                readOnly
                value={`https://wepostai-api.azurewebsites.net/api/v1/public/agencies/${agencyId}/newsletter/subscribe`}
                className="font-mono text-xs"
              />
              <Button
                variant="outline"
                size="icon"
                onClick={() => {
                  navigator.clipboard.writeText(
                    `https://wepostai-api.azurewebsites.net/api/v1/public/agencies/${agencyId}/newsletter/subscribe`
                  );
                  toast.success("URL copiato!");
                }}
              >
                <Copy className="size-4" />
              </Button>
            </div>
          </div>
          <div className="space-y-2">
            <Label>Codice HTML da incorporare</Label>
            <div className="relative">
              <pre className="p-3 rounded-lg bg-muted text-xs overflow-x-auto whitespace-pre-wrap font-mono">{`<form action="https://wepostai-api.azurewebsites.net/api/v1/public/agencies/${agencyId}/newsletter/subscribe" method="POST">
  <input type="email" name="email" placeholder="La tua email" required>
  <input type="text" name="name" placeholder="Il tuo nome">
  <button type="submit">Iscriviti</button>
</form>`}</pre>
              <Button
                variant="outline"
                size="sm"
                className="absolute top-2 right-2"
                onClick={() => {
                  navigator.clipboard.writeText(
                    `<form action="https://wepostai-api.azurewebsites.net/api/v1/public/agencies/${agencyId}/newsletter/subscribe" method="POST">\n  <input type="email" name="email" placeholder="La tua email" required>\n  <input type="text" name="name" placeholder="Il tuo nome">\n  <button type="submit">Iscriviti</button>\n</form>`
                  );
                  toast.success("Codice HTML copiato!");
                }}
              >
                <Copy className="size-3.5 mr-1" /> Copia
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Pipeline overview */}
      <div className="grid sm:grid-cols-3 gap-4">
        <Card>
          <CardContent className="pt-6 text-center">
            <div className="size-10 rounded-lg bg-emerald-100 dark:bg-emerald-950 flex items-center justify-center mx-auto mb-3">
              <Newspaper className="size-5 text-emerald-600" />
            </div>
            <h4 className="font-medium text-sm">Scansione fonti</h4>
            <p className="text-xs text-muted-foreground mt-1">Monitora RSS e siti web</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6 text-center">
            <div className="size-10 rounded-lg bg-amber-100 dark:bg-amber-950 flex items-center justify-center mx-auto mb-3">
              <Target className="size-5 text-amber-600" />
            </div>
            <h4 className="font-medium text-sm">Scoring rilevanza</h4>
            <p className="text-xs text-muted-foreground mt-1">AI classifica i contenuti</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6 text-center">
            <div className="size-10 rounded-lg bg-blue-100 dark:bg-blue-950 flex items-center justify-center mx-auto mb-3">
              <Sparkles className="size-5 text-blue-600" />
            </div>
            <h4 className="font-medium text-sm">Generazione</h4>
            <p className="text-xs text-muted-foreground mt-1">Newsletter in brand voice</p>
          </CardContent>
        </Card>
      </div>

      {/* Email Configuration */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <div>
            <CardTitle className="flex items-center gap-2 text-base">
              <Settings2 className="size-4" /> Configurazione Email
            </CardTitle>
            <CardDescription>
              {configLoading
                ? "Caricamento..."
                : `Default agency${agencyDefaultConfig ? " configurato" : " non configurato"} · ${configs.filter((c) => c.projectId).length} override per progetto`}
            </CardDescription>
          </div>
          <Button variant="outline" size="sm" onClick={() => { resetConfigForm(); setShowConfig(!showConfig); }}>
            <Plus className="size-4 mr-1" /> Nuova configurazione
          </Button>
        </CardHeader>

        {configs.length > 0 && (
          <CardContent className="space-y-2 border-t pt-4">
            {configs.map((c) => (
              <div key={c.id} className="flex items-center justify-between p-3 border rounded-lg">
                <div>
                  <p className="text-sm font-medium">
                    {c.projectId ? (c.projectName || "Progetto") : "Default agency"}
                  </p>
                  <p className="text-xs text-muted-foreground">
                    {c.providerType === EmailProviderType.Smtp ? "SMTP" : "SendGrid"} — {c.fromEmail}
                  </p>
                </div>
                <Button variant="ghost" size="sm" onClick={() => loadConfigIntoForm(c)}>
                  Modifica
                </Button>
              </div>
            ))}
          </CardContent>
        )}

        {showConfig && (
          <CardContent className="space-y-4 border-t pt-4">
            <div className="space-y-2">
              <Label>Ambito</Label>
              <select
                className="w-full border rounded-md px-3 py-2 text-sm bg-background"
                value={configProjectId}
                onChange={(e) => setConfigProjectId(e.target.value)}
              >
                <option value="">Default agency (fallback per tutti i progetti)</option>
                {projects?.map((p) => (
                  <option key={p.id} value={p.id}>Progetto: {p.name}</option>
                ))}
              </select>
              <p className="text-xs text-muted-foreground">
                Se un progetto ha una sua configurazione, verrà usata al posto di quella dell&apos;agency.
              </p>
            </div>
            <div className="space-y-2">
              <Label>Provider</Label>
              <select
                className="w-full border rounded-md px-3 py-2 text-sm bg-background"
                value={providerType}
                onChange={(e) => setProviderType(Number(e.target.value))}
              >
                <option value={EmailProviderType.Smtp}>SMTP</option>
                <option value={EmailProviderType.SendGrid}>SendGrid</option>
              </select>
            </div>

            {providerType === EmailProviderType.Smtp ? (
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label>Host SMTP</Label>
                  <Input value={smtpHost} onChange={(e) => setSmtpHost(e.target.value)} placeholder="smtp.gmail.com" />
                </div>
                <div className="space-y-2">
                  <Label>Porta</Label>
                  <Input value={smtpPort} onChange={(e) => setSmtpPort(e.target.value)} placeholder="587" />
                </div>
                <div className="space-y-2">
                  <Label>Username</Label>
                  <Input value={smtpUsername} onChange={(e) => setSmtpUsername(e.target.value)} />
                </div>
                <div className="space-y-2">
                  <Label>Password</Label>
                  <Input type="password" value={smtpPassword} onChange={(e) => setSmtpPassword(e.target.value)} />
                </div>
              </div>
            ) : (
              <div className="space-y-2">
                <Label>API Key SendGrid</Label>
                <Input type="password" value={apiKey} onChange={(e) => setApiKey(e.target.value)} placeholder="SG.xxxx..." />
              </div>
            )}

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Email mittente</Label>
                <Input value={fromEmail} onChange={(e) => setFromEmail(e.target.value)} placeholder="newsletter@dominio.com" />
              </div>
              <div className="space-y-2">
                <Label>Nome mittente</Label>
                <Input value={fromName} onChange={(e) => setFromName(e.target.value)} placeholder="La tua Azienda" />
              </div>
            </div>

            <Button onClick={() => saveConfigMutation.mutate()} disabled={!fromEmail || saveConfigMutation.isPending}>
              {saveConfigMutation.isPending ? "Salvataggio..." : "Salva"}
            </Button>
          </CardContent>
        )}
      </Card>

      {/* Subscribers */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <div>
            <CardTitle className="flex items-center gap-2 text-base">
              <Users className="size-4" /> Iscritti ({activeSubscribers.length})
            </CardTitle>
            <CardDescription>Gestisci gli iscritti alla newsletter.</CardDescription>
          </div>
          <Button size="sm" onClick={() => setShowAddSub(!showAddSub)}>
            <Plus className="size-4 mr-1" /> Aggiungi
          </Button>
        </CardHeader>
        <CardContent className="space-y-3">
          {showAddSub && (
            <div className="flex gap-3 items-end p-4 border rounded-lg bg-muted/30">
              <div className="flex-1 space-y-1">
                <Label className="text-xs">Email</Label>
                <Input value={subEmail} onChange={(e) => setSubEmail(e.target.value)} placeholder="email@esempio.com" />
              </div>
              <div className="flex-1 space-y-1">
                <Label className="text-xs">Nome</Label>
                <Input value={subName} onChange={(e) => setSubName(e.target.value)} placeholder="Opzionale" />
              </div>
              <Button onClick={() => addSubMutation.mutate()} disabled={!subEmail || addSubMutation.isPending}>
                Aggiungi
              </Button>
            </div>
          )}

          {subsLoading ? (
            <div className="space-y-2">
              {[1, 2, 3].map((i) => <Skeleton key={i} className="h-10 rounded" />)}
            </div>
          ) : subscribers.length === 0 ? (
            <p className="text-sm text-muted-foreground text-center py-6">
              Nessun iscritto. Aggiungi il primo iscritto alla newsletter.
            </p>
          ) : (
            <div className="divide-y">
              {subscribers.map((sub) => (
                <div key={sub.id} className="flex items-center justify-between py-2.5">
                  <div>
                    <p className="font-medium text-sm">{sub.email}</p>
                    {sub.name && <p className="text-xs text-muted-foreground">{sub.name}</p>}
                  </div>
                  <div className="flex items-center gap-2">
                    <Badge variant={sub.isActive ? "default" : "secondary"} className="text-xs">
                      {sub.isActive ? "Attivo" : "Disiscritto"}
                    </Badge>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => removeSubMutation.mutate(sub.id)}
                    >
                      <Trash2 className="size-3.5" />
                    </Button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {agencyDefaultConfig && activeSubscribers.length > 0 && (
        <Card className="border-green-500/20 bg-green-50/50 dark:bg-green-950/20">
          <CardContent className="flex items-center gap-4 py-4">
            <Send className="size-5 text-green-600" />
            <div>
              <p className="font-medium text-sm">Pronto per l&apos;invio</p>
              <p className="text-xs text-muted-foreground">
                Vai alla pagina Contenuti o Approvazioni, approva una newsletter e verr&agrave; inviata agli {activeSubscribers.length} iscritti attivi.
              </p>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
