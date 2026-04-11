"use client";

import { useState, useMemo } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import type { ApiResponse, LlmKey } from "@/types/api";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { Separator } from "@/components/ui/separator";
import { Key, Plus, Loader2, Shield, X, Trash2 } from "lucide-react";
import { toast } from "sonner";

const providerLabels: Record<number, string> = {
  1: "OpenAI",
  2: "Anthropic (Claude)",
  3: "NanoBanana",
  4: "Azure OpenAI",
  5: "Custom",
  6: "HiggField",
};

const categoryLabels: Record<number, string> = {
  0: "Testo",
  1: "Immagine",
  2: "Video",
};

// Static model catalog per provider+category
const modelCatalog: Record<string, { id: string; name: string }[]> = {
  "1_0": [
    { id: "gpt-4o", name: "GPT-4o" },
    { id: "gpt-4o-mini", name: "GPT-4o Mini" },
    { id: "gpt-4-turbo", name: "GPT-4 Turbo" },
    { id: "o3-mini", name: "o3-mini" },
    { id: "o4-mini", name: "o4-mini" },
  ],
  "1_1": [
    { id: "dall-e-3", name: "DALL-E 3" },
    { id: "dall-e-2", name: "DALL-E 2" },
  ],
  "2_0": [
    { id: "claude-sonnet-4-20250514", name: "Claude Sonnet 4" },
    { id: "claude-opus-4-20250514", name: "Claude Opus 4" },
    { id: "claude-haiku-4-5-20251001", name: "Claude Haiku 4.5" },
  ],
  "4_0": [
    { id: "gpt-4o", name: "GPT-4o" },
    { id: "gpt-4o-mini", name: "GPT-4o Mini" },
    { id: "gpt-4-turbo", name: "GPT-4 Turbo" },
  ],
  "3_1": [
    { id: "nanobanana-v1", name: "NanoBanana v1" },
  ],
  "6_1": [
    { id: "higgfield-v1", name: "HiggField v1" },
    { id: "higgfield-xl", name: "HiggField XL" },
  ],
  "6_2": [
    { id: "seedance-1.0", name: "Seedance 1.0" },
  ],
};

type FormState = {
  displayName: string;
  providerType: number;
  apiKey: string;
  apiKeySecret: string;
  modelName: string;
  baseUrl: string;
  category: number;
};

const defaultForm: FormState = {
  displayName: "",
  providerType: 1,
  apiKey: "",
  apiKeySecret: "",
  modelName: "",
  baseUrl: "",
  category: 0,
};

export default function ApiKeysPage() {
  const queryClient = useQueryClient();
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState<FormState>(defaultForm);
  const [customModel, setCustomModel] = useState(false);

  const { data, isLoading } = useQuery({
    queryKey: ["llm-keys"],
    queryFn: () => apiClient.get<ApiResponse<LlmKey[]>>("/api/v1/llmkeys"),
  });

  const addKey = useMutation({
    mutationFn: (data: FormState) => apiClient.post("/api/v1/llmkeys", {
      ...data,
      apiKeySecret: data.apiKeySecret || null,
      baseUrl: data.baseUrl || null,
      modelName: data.modelName || null,
    }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["llm-keys"] });
      setShowForm(false);
      setForm(defaultForm);
      setCustomModel(false);
      toast.success("Chiave API aggiunta");
    },
    onError: (err: any) => {
      toast.error(err?.message || "Errore durante il salvataggio");
    },
  });

  const deleteKey = useMutation({
    mutationFn: (id: string) => apiClient.delete(`/api/v1/llmkeys/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["llm-keys"] });
      toast.success("Chiave rimossa");
    },
    onError: (err: any) => {
      toast.error(err?.message || "Errore durante l'eliminazione");
    },
  });

  const keys = data?.data || [];

  const suggestedModels = useMemo(() => {
    const key = `${form.providerType}_${form.category}`;
    return modelCatalog[key] || [];
  }, [form.providerType, form.category]);

  const showBaseUrl = [3, 5, 6].includes(form.providerType);
  const showApiKeySecret = form.providerType === 6; // HiggField

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Chiavi API LLM</h1>
          <p className="text-muted-foreground mt-1">Gestisci le chiavi API dei provider AI</p>
        </div>
        <Button onClick={() => setShowForm(!showForm)}>
          {showForm ? <><X className="size-4" /> Chiudi</> : <><Plus className="size-4" /> Aggiungi chiave</>}
        </Button>
      </div>

      {showForm && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Nuova chiave API</CardTitle>
            <CardDescription>La chiave verra criptata e non sara piu visibile</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label>Nome</Label>
              <Input value={form.displayName}
                onChange={(e) => setForm({ ...form, displayName: e.target.value })}
                placeholder="es. La mia chiave OpenAI" />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Provider</Label>
                <select value={form.providerType}
                  onChange={(e) => {
                    const pt = Number(e.target.value);
                    setForm({ ...form, providerType: pt, modelName: "" });
                    setCustomModel(false);
                  }}
                  className="flex h-9 w-full rounded-lg border border-input bg-transparent px-3 py-1 text-sm shadow-xs transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring">
                  {Object.entries(providerLabels).map(([key, label]) => (
                    <option key={key} value={key}>{label}</option>
                  ))}
                </select>
              </div>
              <div className="space-y-2">
                <Label>Categoria</Label>
                <select value={form.category}
                  onChange={(e) => {
                    setForm({ ...form, category: Number(e.target.value), modelName: "" });
                    setCustomModel(false);
                  }}
                  className="flex h-9 w-full rounded-lg border border-input bg-transparent px-3 py-1 text-sm shadow-xs transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring">
                  {Object.entries(categoryLabels).map(([key, label]) => (
                    <option key={key} value={key}>{label}</option>
                  ))}
                </select>
              </div>
            </div>
            <div className="space-y-2">
              <Label>API Key</Label>
              <Input type="password" value={form.apiKey}
                onChange={(e) => setForm({ ...form, apiKey: e.target.value })}
                placeholder="sk-..." />
            </div>
            {showApiKeySecret && (
              <div className="space-y-2">
                <Label>API Key Secret</Label>
                <Input type="password" value={form.apiKeySecret}
                  onChange={(e) => setForm({ ...form, apiKeySecret: e.target.value })}
                  placeholder="Inserisci l'API Key Secret di HiggField" />
                <p className="text-xs text-muted-foreground">
                  Richiesto da HiggField per l'autenticazione
                </p>
              </div>
            )}
            {showBaseUrl && (
              <div className="space-y-2">
                <Label>Base URL</Label>
                <Input value={form.baseUrl}
                  onChange={(e) => setForm({ ...form, baseUrl: e.target.value })}
                  placeholder="es. https://api.example.com/v1" />
                <p className="text-xs text-muted-foreground">
                  URL base dell'API del provider (obbligatorio per provider custom)
                </p>
              </div>
            )}
            <div className="space-y-2">
              <Label>Modello</Label>
              {suggestedModels.length > 0 && !customModel ? (
                <div className="space-y-2">
                  <select
                    value={form.modelName}
                    onChange={(e) => setForm({ ...form, modelName: e.target.value })}
                    className="flex h-9 w-full rounded-lg border border-input bg-transparent px-3 py-1 text-sm shadow-xs transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
                  >
                    <option value="">Seleziona un modello...</option>
                    {suggestedModels.map((m) => (
                      <option key={m.id} value={m.id}>{m.name} ({m.id})</option>
                    ))}
                  </select>
                  <button
                    type="button"
                    onClick={() => setCustomModel(true)}
                    className="text-xs text-primary hover:underline"
                  >
                    Inserisci nome modello personalizzato
                  </button>
                </div>
              ) : (
                <div className="space-y-2">
                  <Input value={form.modelName}
                    onChange={(e) => setForm({ ...form, modelName: e.target.value })}
                    placeholder="es. gpt-4o, claude-sonnet-4-20250514" />
                  {suggestedModels.length > 0 && (
                    <button
                      type="button"
                      onClick={() => { setCustomModel(false); setForm({ ...form, modelName: "" }); }}
                      className="text-xs text-primary hover:underline"
                    >
                      Scegli da lista modelli
                    </button>
                  )}
                </div>
              )}
            </div>
            <Separator />
            <div className="flex gap-2">
              <Button onClick={() => addKey.mutate(form)} disabled={addKey.isPending}>
                {addKey.isPending ? (
                  <><Loader2 className="size-4 animate-spin" /> Salvataggio...</>
                ) : (
                  <><Shield className="size-4" /> Salva chiave</>
                )}
              </Button>
              <Button variant="ghost" onClick={() => setShowForm(false)}>Annulla</Button>
            </div>
          </CardContent>
        </Card>
      )}

      {isLoading ? (
        <div className="space-y-4">
          {[1, 2].map((i) => (
            <Card key={i}>
              <CardContent className="pt-6 flex items-center gap-4">
                <Skeleton className="size-10 rounded-lg" />
                <div className="flex-1">
                  <Skeleton className="h-4 w-48" />
                  <Skeleton className="h-3 w-32 mt-2" />
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      ) : keys.length === 0 ? (
        <Card className="text-center py-16">
          <CardContent className="space-y-4">
            <div className="size-16 rounded-full bg-primary/10 flex items-center justify-center mx-auto">
              <Key className="size-8 text-primary" />
            </div>
            <h3 className="text-lg font-semibold">Nessuna chiave API</h3>
            <p className="text-muted-foreground max-w-sm mx-auto">
              Aggiungi una chiave API per iniziare a usare gli agenti AI
            </p>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-3">
          {keys.map((key) => (
            <Card key={key.id}>
              <CardContent className="pt-6 flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className="size-10 rounded-lg bg-muted flex items-center justify-center">
                    <Key className="size-4" />
                  </div>
                  <div>
                    <h4 className="font-medium">{key.displayName}</h4>
                    <p className="text-sm text-muted-foreground">
                      {providerLabels[key.providerType] || `Provider ${key.providerType}`} &middot; {key.maskedKey}
                      {key.modelName && ` \u00b7 ${key.modelName}`}
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <Badge variant="outline">
                    {categoryLabels[key.category ?? 0] || "Testo"}
                  </Badge>
                  <Badge variant={key.isActive ? "default" : "secondary"}>
                    {key.isActive ? "Attiva" : "Disattivata"}
                  </Badge>
                  <Button
                    variant="ghost"
                    size="icon-sm"
                    className="text-destructive hover:text-destructive"
                    onClick={() => deleteKey.mutate(key.id)}
                    disabled={deleteKey.isPending}
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
  );
}
