"use client";

import { useParams } from "next/navigation";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { agenciesApi } from "@/lib/api/agencies.api";
import { connectorsApi } from "@/lib/api/connectors.api";
import { projectsApi } from "@/lib/api/projects.api";
import { apiClient } from "@/lib/api/client";
import { SocialPlatform } from "@/types/api";
import type { SocialConnector } from "@/types/api";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import { Share2, Loader2, Camera, Briefcase, Hash, Globe, CalendarDays, Plus, Trash2, Link2 } from "lucide-react";
import { toast } from "sonner";

const platformConfig: Record<number, { label: string; icon: typeof Camera; color: string }> = {
  [SocialPlatform.Twitter]: { label: "X (Twitter)", icon: Hash, color: "text-foreground bg-muted" },
  [SocialPlatform.LinkedIn]: { label: "LinkedIn", icon: Briefcase, color: "text-blue-700 bg-blue-100 dark:bg-blue-950" },
  [SocialPlatform.Instagram]: { label: "Instagram", icon: Camera, color: "text-pink-600 bg-pink-100 dark:bg-pink-950" },
  [SocialPlatform.Facebook]: { label: "Facebook", icon: Globe, color: "text-blue-600 bg-blue-100 dark:bg-blue-950" },
};

const allPlatforms = [SocialPlatform.Instagram, SocialPlatform.LinkedIn, SocialPlatform.Twitter, SocialPlatform.Facebook];

export default function SocialPage() {
  const { agencyId } = useParams<{ agencyId: string }>();
  const queryClient = useQueryClient();
  const [generating, setGenerating] = useState(false);
  const [showConnect, setShowConnect] = useState(false);
  const [formProjectId, setFormProjectId] = useState<string>("");
  const [formPlatform, setFormPlatform] = useState<number>(SocialPlatform.Twitter);
  const [formToken, setFormToken] = useState("");
  const [formAccountId, setFormAccountId] = useState("");
  const [formAccountName, setFormAccountName] = useState("");

  const { data: agencyData } = useQuery({
    queryKey: ["agency", agencyId],
    queryFn: () => agenciesApi.get(agencyId),
  });

  const { data: connectorsData, isLoading: connectorsLoading } = useQuery({
    queryKey: ["connectors", agencyId],
    queryFn: () => connectorsApi.list(agencyId),
  });

  const { data: projects } = useQuery({
    queryKey: ["projects", agencyId],
    queryFn: () => projectsApi.list(agencyId),
  });

  const agency = agencyData?.data;
  const connectors: SocialConnector[] = connectorsData?.data ?? [];
  const defaultConnectors = connectors.filter((c) => !c.projectId);
  const projectConnectorGroups = connectors
    .filter((c) => c.projectId)
    .reduce<Record<string, { projectId: string; projectName: string; items: SocialConnector[] }>>((acc, c) => {
      const pid = c.projectId!;
      if (!acc[pid]) acc[pid] = { projectId: pid, projectName: c.projectName || "Progetto", items: [] };
      acc[pid].items.push(c);
      return acc;
    }, {});

  const connectMutation = useMutation({
    mutationFn: () =>
      connectorsApi.connect(agencyId, {
        platform: formPlatform,
        projectId: formProjectId || null,
        accessToken: formToken,
        accountId: formAccountId || undefined,
        accountName: formAccountName || undefined,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["connectors", agencyId] });
      toast.success("Piattaforma connessa!");
      setShowConnect(false);
      setFormProjectId("");
      setFormToken("");
      setFormAccountId("");
      setFormAccountName("");
    },
    onError: () => toast.error("Errore nella connessione."),
  });

  const disconnectMutation = useMutation({
    mutationFn: (connectorId: string) => connectorsApi.disconnect(agencyId, connectorId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["connectors", agencyId] });
      toast.success("Piattaforma disconnessa.");
    },
    onError: () => toast.error("Errore nella disconnessione."),
  });

  const handleGenerate = async () => {
    if (!agency?.defaultLlmProviderKeyId) {
      toast.error("Configura prima una chiave LLM nelle impostazioni dell'agenzia");
      return;
    }
    setGenerating(true);
    try {
      await apiClient.post(`/api/v1/agencies/${agencyId}/agents/social-manager/run`, {});
      toast.success("Generazione post social avviata!");
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
          <h2 className="text-xl font-bold tracking-tight">Social Media Manager</h2>
          <p className="text-muted-foreground text-sm mt-1">
            Genera post multi-piattaforma e pubblica automaticamente sui social connessi
          </p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => setShowConnect(!showConnect)}>
            <Link2 className="size-4 mr-1" /> Connetti
          </Button>
          <Button onClick={handleGenerate} disabled={generating}>
            {generating ? (
              <><Loader2 className="size-4 animate-spin" /> Generazione...</>
            ) : (
              <><Share2 className="size-4" /> Genera post social</>
            )}
          </Button>
        </div>
      </div>

      {/* Connect form */}
      {showConnect && (
        <Card>
          <CardHeader>
            <CardTitle>Connetti piattaforma social</CardTitle>
            <CardDescription>Inserisci il token OAuth della piattaforma per pubblicare automaticamente.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label>Ambito</Label>
              <select
                className="w-full border rounded-md px-3 py-2 text-sm bg-background"
                value={formProjectId}
                onChange={(e) => setFormProjectId(e.target.value)}
              >
                <option value="">Default agency (fallback per tutti i progetti)</option>
                {projects?.map((p) => (
                  <option key={p.id} value={p.id}>Progetto: {p.name}</option>
                ))}
              </select>
              <p className="text-xs text-muted-foreground">
                Se un progetto ha una sua pagina per questa piattaforma, verrà usata al posto del default.
              </p>
            </div>
            <div className="space-y-2">
              <Label>Piattaforma</Label>
              <select
                className="w-full border rounded-md px-3 py-2 text-sm bg-background"
                value={formPlatform}
                onChange={(e) => setFormPlatform(Number(e.target.value))}
              >
                {allPlatforms.map((p) => (
                  <option key={p} value={p}>{platformConfig[p]?.label}</option>
                ))}
              </select>
            </div>
            <div className="space-y-2">
              <Label>Access Token</Label>
              <Input type="password" value={formToken} onChange={(e) => setFormToken(e.target.value)} placeholder="Token OAuth..." />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Account/Page ID</Label>
                <Input value={formAccountId} onChange={(e) => setFormAccountId(e.target.value)} placeholder="Opzionale" />
              </div>
              <div className="space-y-2">
                <Label>Nome Account</Label>
                <Input value={formAccountName} onChange={(e) => setFormAccountName(e.target.value)} placeholder="Opzionale" />
              </div>
            </div>
            <div className="flex gap-2">
              <Button onClick={() => connectMutation.mutate()} disabled={!formToken || connectMutation.isPending}>
                {connectMutation.isPending ? "Connessione..." : "Connetti"}
              </Button>
              <Button variant="outline" onClick={() => setShowConnect(false)}>Annulla</Button>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Platform cards grouped by scope */}
      {connectorsLoading ? (
        <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-4">
          {[1, 2, 3, 4].map((i) => <Skeleton key={i} className="h-32 rounded-xl" />)}
        </div>
      ) : (
        <div className="space-y-6">
          <div>
            <h3 className="text-sm font-semibold mb-3 text-muted-foreground">Default agency (fallback)</h3>
            <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-4">
              {allPlatforms.map((platform) => {
                const config = platformConfig[platform];
                const connector = defaultConnectors.find((c) => c.platform === platform);
                const Icon = config.icon;
                return (
                  <Card key={platform} className={connector ? "border-green-500/30" : ""}>
                    <CardContent className="pt-6 text-center">
                      <div className={`size-12 rounded-xl flex items-center justify-center mx-auto ${config.color}`}>
                        <Icon className="size-6" />
                      </div>
                      <h4 className="font-medium mt-3">{config.label}</h4>
                      {connector ? (
                        <div className="mt-2 space-y-2">
                          <Badge variant="default">Connesso</Badge>
                          {connector.accountName && (
                            <p className="text-xs text-muted-foreground">{connector.accountName}</p>
                          )}
                          <Button
                            variant="ghost"
                            size="sm"
                            className="text-destructive"
                            onClick={() => disconnectMutation.mutate(connector.id)}
                          >
                            <Trash2 className="size-3 mr-1" /> Disconnetti
                          </Button>
                        </div>
                      ) : (
                        <p className="text-muted-foreground text-xs mt-1">Non connesso</p>
                      )}
                    </CardContent>
                  </Card>
                );
              })}
            </div>
          </div>

          {Object.values(projectConnectorGroups).map((group) => (
            <div key={group.projectId}>
              <h3 className="text-sm font-semibold mb-3 text-muted-foreground">
                Progetto: {group.projectName}
              </h3>
              <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-4">
                {group.items.map((connector) => {
                  const config = platformConfig[connector.platform];
                  const Icon = config.icon;
                  return (
                    <Card key={connector.id} className="border-green-500/30">
                      <CardContent className="pt-6 text-center">
                        <div className={`size-12 rounded-xl flex items-center justify-center mx-auto ${config.color}`}>
                          <Icon className="size-6" />
                        </div>
                        <h4 className="font-medium mt-3">{config.label}</h4>
                        <div className="mt-2 space-y-2">
                          <Badge variant="default">Connesso</Badge>
                          {connector.accountName && (
                            <p className="text-xs text-muted-foreground">{connector.accountName}</p>
                          )}
                          <Button
                            variant="ghost"
                            size="sm"
                            className="text-destructive"
                            onClick={() => disconnectMutation.mutate(connector.id)}
                          >
                            <Trash2 className="size-3 mr-1" /> Disconnetti
                          </Button>
                        </div>
                      </CardContent>
                    </Card>
                  );
                })}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Auto-publish info */}
      <Card className="text-center py-12">
        <CardContent className="space-y-4">
          <div className="size-16 rounded-full bg-violet-100 dark:bg-violet-950 flex items-center justify-center mx-auto">
            <CalendarDays className="size-8 text-violet-600" />
          </div>
          <h3 className="text-lg font-semibold">Pubblicazione Automatica</h3>
          <p className="text-muted-foreground max-w-md mx-auto">
            Quando approvi un contenuto, viene pubblicato sulla pagina specifica del progetto se configurata,
            altrimenti sul default dell&apos;agency. Un post per piattaforma.
          </p>
          {connectors.length > 0 && (
            <Badge variant="outline" className="text-green-600">
              {connectors.length} piattaform{connectors.length === 1 ? "a" : "e"} conness{connectors.length === 1 ? "a" : "e"}
            </Badge>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
