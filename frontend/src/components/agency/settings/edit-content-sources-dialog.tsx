"use client";

import { useState, useEffect } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import {
  Loader2,
  Plus,
  Trash2,
  Rss,
  Globe,
  Share2,
  Sparkles,
  ExternalLink,
} from "lucide-react";
import { toast } from "sonner";
import { sourcesApi } from "@/lib/api/sources.api";
import type { ContentSource } from "@/types/api";
import { ContentSourceType } from "@/types/api";

interface DiscoveredSource {
  url: string;
  name: string;
  description: string;
  type: number;
}

interface EditContentSourcesDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  agencyId: string;
  projectId?: string;
  onSaved: () => void;
}

const sourceTypeOptions = [
  { value: ContentSourceType.RssFeed, label: "RSS Feed", icon: Rss },
  { value: ContentSourceType.Website, label: "Sito Web", icon: Globe },
  { value: ContentSourceType.SocialAccount, label: "Social", icon: Share2 },
];

export function EditContentSourcesDialog({
  open,
  onOpenChange,
  agencyId,
  projectId,
  onSaved,
}: EditContentSourcesDialogProps) {
  const [sources, setSources] = useState<ContentSource[]>([]);
  const [loading, setLoading] = useState(false);
  const [adding, setAdding] = useState(false);
  const [newType, setNewType] = useState<number>(ContentSourceType.RssFeed);
  const [newUrl, setNewUrl] = useState("");
  const [newName, setNewName] = useState("");
  const [discovering, setDiscovering] = useState(false);
  const [suggestions, setSuggestions] = useState<DiscoveredSource[]>([]);
  const [addingSuggestion, setAddingSuggestion] = useState<string | null>(null);

  const fetchSources = async () => {
    setLoading(true);
    try {
      const res = await sourcesApi.list(agencyId);
      setSources(res.data ?? []);
    } catch {
      toast.error("Errore nel caricamento delle fonti");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (open) {
      fetchSources();
      setNewUrl("");
      setNewName("");
      setSuggestions([]);
    }
  }, [open]);

  const handleAdd = async () => {
    if (!newUrl.trim()) {
      toast.error("Inserisci un URL");
      return;
    }
    setAdding(true);
    try {
      await sourcesApi.create(agencyId, {
        type: newType,
        url: newUrl.trim(),
        name: newName.trim() || undefined,
      });
      toast.success("Fonte aggiunta");
      setNewUrl("");
      setNewName("");
      fetchSources();
      onSaved();
    } catch (err: any) {
      toast.error(err?.message || "Errore durante l'aggiunta");
    } finally {
      setAdding(false);
    }
  };

  const handleDelete = async (sourceId: string) => {
    try {
      await sourcesApi.delete(agencyId, sourceId);
      toast.success("Fonte rimossa");
      setSources((prev) => prev.filter((s) => s.id !== sourceId));
      onSaved();
    } catch (err: any) {
      toast.error(err?.message || "Errore durante la rimozione");
    }
  };

  const handleDiscover = async () => {
    setDiscovering(true);
    setSuggestions([]);
    try {
      const res = await sourcesApi.discover(agencyId, projectId);
      const data = res?.data ?? res ?? [];
      setSuggestions(Array.isArray(data) ? data : []);
      if ((Array.isArray(data) ? data : []).length === 0) {
        toast.info("Nessuna fonte suggerita trovata");
      }
    } catch {
      toast.error("Errore durante la ricerca delle fonti");
    } finally {
      setDiscovering(false);
    }
  };

  const handleAddSuggestion = async (suggestion: DiscoveredSource) => {
    setAddingSuggestion(suggestion.url);
    try {
      await sourcesApi.create(agencyId, {
        type: suggestion.type,
        url: suggestion.url,
        name: suggestion.name,
      });
      toast.success("Fonte aggiunta");
      setSuggestions((prev) => prev.filter((s) => s.url !== suggestion.url));
      fetchSources();
      onSaved();
    } catch (err: any) {
      toast.error(err?.message || "Errore durante l'aggiunta");
    } finally {
      setAddingSuggestion(null);
    }
  };

  const getTypeInfo = (type: number) =>
    sourceTypeOptions.find((o) => o.value === type) ?? sourceTypeOptions[0];

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-xl">
        <DialogHeader>
          <DialogTitle>Fonti Contenuto</DialogTitle>
        </DialogHeader>

        <div className="space-y-4">
          {/* Add new source */}
          <div className="space-y-3 rounded-lg border p-4">
            <p className="text-sm font-medium">Aggiungi fonte</p>
            <div className="space-y-2">
              <Label>Tipo</Label>
              <select
                value={newType}
                onChange={(e) => setNewType(Number(e.target.value))}
                className="flex h-9 w-full rounded-lg border border-input bg-transparent px-3 py-1 text-sm shadow-xs transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
              >
                {sourceTypeOptions.map((o) => (
                  <option key={o.value} value={o.value}>
                    {o.label}
                  </option>
                ))}
              </select>
            </div>
            <div className="space-y-2">
              <Label>URL</Label>
              <Input
                value={newUrl}
                onChange={(e) => setNewUrl(e.target.value)}
                placeholder="https://example.com/feed.xml"
              />
            </div>
            <div className="space-y-2">
              <Label>Nome (opzionale)</Label>
              <Input
                value={newName}
                onChange={(e) => setNewName(e.target.value)}
                placeholder="es. Blog aziendale"
              />
            </div>
            <div className="flex gap-2">
              <Button onClick={handleAdd} disabled={adding} size="sm">
                {adding ? (
                  <Loader2 className="size-4 animate-spin" />
                ) : (
                  <Plus className="size-4" />
                )}
                Aggiungi
              </Button>
              <Button
                onClick={handleDiscover}
                disabled={discovering}
                size="sm"
                variant="outline"
              >
                {discovering ? (
                  <Loader2 className="size-4 animate-spin" />
                ) : (
                  <Sparkles className="size-4" />
                )}
                Cerca fonti AI
              </Button>
            </div>
          </div>

          {/* Source list */}
          <div className="space-y-2">
            <p className="text-sm font-medium text-muted-foreground">
              Fonti attive ({sources.length})
            </p>
            {loading ? (
              <div className="flex justify-center py-4">
                <Loader2 className="size-5 animate-spin text-muted-foreground" />
              </div>
            ) : sources.length === 0 ? (
              <p className="text-sm text-muted-foreground py-4 text-center">
                Nessuna fonte configurata. Aggiungi un feed RSS o un sito web
                per iniziare.
              </p>
            ) : (
              <div className="space-y-2 max-h-60 overflow-y-auto">
                {sources.map((source) => {
                  const typeInfo = getTypeInfo(source.type);
                  const Icon = typeInfo.icon;
                  return (
                    <div
                      key={source.id}
                      className="flex items-center gap-3 rounded-lg border px-3 py-2"
                    >
                      <Icon className="size-4 text-muted-foreground shrink-0" />
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium truncate">
                          {source.name || source.url}
                        </p>
                        {source.name && (
                          <p className="text-xs text-muted-foreground truncate">
                            {source.url}
                          </p>
                        )}
                      </div>
                      <Badge variant="secondary" className="text-xs shrink-0">
                        {typeInfo.label}
                      </Badge>
                      <Button
                        variant="ghost"
                        size="icon-xs"
                        onClick={() => handleDelete(source.id)}
                        className="text-destructive hover:text-destructive shrink-0"
                      >
                        <Trash2 className="size-3.5" />
                      </Button>
                    </div>
                  );
                })}
              </div>
            )}
          </div>

          {/* AI-discovered suggestions */}
          {discovering && (
            <div className="flex items-center justify-center gap-2 py-6 text-sm text-muted-foreground">
              <Loader2 className="size-4 animate-spin" />
              Ricerca fonti in corso...
            </div>
          )}

          {suggestions.length > 0 && (
            <div className="space-y-2">
              <p className="text-sm font-medium text-muted-foreground">
                Fonti suggerite dall&apos;AI ({suggestions.length})
              </p>
              <div className="space-y-2 max-h-60 overflow-y-auto">
                {suggestions.map((suggestion) => {
                  const typeInfo = getTypeInfo(suggestion.type);
                  const Icon = typeInfo.icon;
                  const isAdding = addingSuggestion === suggestion.url;
                  return (
                    <div
                      key={suggestion.url}
                      className="rounded-lg border border-dashed p-3 space-y-1"
                    >
                      <div className="flex items-center gap-2">
                        <Icon className="size-4 text-muted-foreground shrink-0" />
                        <span className="text-sm font-semibold truncate flex-1">
                          {suggestion.name}
                        </span>
                        <Badge
                          variant="secondary"
                          className="text-xs shrink-0"
                        >
                          {typeInfo.label}
                        </Badge>
                        <Button
                          size="sm"
                          variant="outline"
                          disabled={isAdding}
                          onClick={() => handleAddSuggestion(suggestion)}
                          className="shrink-0"
                        >
                          {isAdding ? (
                            <Loader2 className="size-3.5 animate-spin" />
                          ) : (
                            <Plus className="size-3.5" />
                          )}
                          Aggiungi
                        </Button>
                      </div>
                      <a
                        href={suggestion.url}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-xs text-primary hover:underline inline-flex items-center gap-1"
                      >
                        {suggestion.url}
                        <ExternalLink className="size-3" />
                      </a>
                      {suggestion.description && (
                        <p className="text-xs text-muted-foreground">
                          {suggestion.description}
                        </p>
                      )}
                    </div>
                  );
                })}
              </div>
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}
