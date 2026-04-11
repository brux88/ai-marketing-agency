"use client";

import { useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { projectsApi } from "@/lib/api/projects.api";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Loader2 } from "lucide-react";
import { toast } from "sonner";
import Link from "next/link";
import { cn } from "@/lib/utils";
import { buttonVariants } from "@/components/ui/button";

export default function NewProjectPage() {
  const { agencyId } = useParams();
  const router = useRouter();
  const [saving, setSaving] = useState(false);

  const [form, setForm] = useState({
    name: "",
    description: "",
    websiteUrl: "",
    tone: "professional",
    style: "",
    keywords: "",
    forbiddenWords: "",
    audienceDescription: "",
    ageRange: "",
    interests: "",
    painPoints: "",
  });

  const handleSubmit = async () => {
    if (!form.name.trim()) {
      toast.error("Il nome del progetto e obbligatorio");
      return;
    }
    setSaving(true);
    try {
      await projectsApi.create(agencyId as string, {
        name: form.name,
        description: form.description || null,
        websiteUrl: form.websiteUrl || null,
        brandVoice: {
          tone: form.tone,
          style: form.style,
          keywords: form.keywords.split(",").map((k) => k.trim()).filter(Boolean),
          examplePhrases: [],
          forbiddenWords: form.forbiddenWords.split(",").map((w) => w.trim()).filter(Boolean),
          language: "it",
        },
        targetAudience: {
          description: form.audienceDescription,
          ageRange: form.ageRange || undefined,
          interests: form.interests.split(",").map((i) => i.trim()).filter(Boolean),
          painPoints: form.painPoints.split(",").map((p) => p.trim()).filter(Boolean),
          personas: [],
        },
      });
      toast.success("Progetto creato con successo");
      router.push(`/agencies/${agencyId}/projects`);
    } catch (err: any) {
      const message = err?.message || "Errore durante la creazione";
      toast.error(message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="space-y-6 max-w-2xl">
      <div>
        <h2 className="text-xl font-bold tracking-tight">Nuovo Progetto</h2>
        <p className="text-muted-foreground text-sm mt-1">
          Crea un nuovo progetto per organizzare i tuoi contenuti
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Informazioni generali</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label>Nome *</Label>
            <Input
              value={form.name}
              onChange={(e) => setForm({ ...form, name: e.target.value })}
              placeholder="es. Prodotto X"
            />
          </div>
          <div className="space-y-2">
            <Label>Descrizione</Label>
            <Textarea
              value={form.description}
              onChange={(e) => setForm({ ...form, description: e.target.value })}
              placeholder="Descrivi il progetto..."
            />
          </div>
          <div className="space-y-2">
            <Label>Sito Web</Label>
            <Input
              value={form.websiteUrl}
              onChange={(e) => setForm({ ...form, websiteUrl: e.target.value })}
              placeholder="https://..."
            />
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Brand Voice</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label>Tono</Label>
            <select
              value={form.tone}
              onChange={(e) => setForm({ ...form, tone: e.target.value })}
              className="flex h-9 w-full rounded-lg border border-input bg-transparent px-3 py-1 text-sm shadow-xs transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
            >
              <option value="professional">Professional</option>
              <option value="casual">Casual</option>
              <option value="playful">Playful</option>
              <option value="formal">Formal</option>
              <option value="friendly">Friendly</option>
            </select>
          </div>
          <div className="space-y-2">
            <Label>Stile</Label>
            <Input
              value={form.style}
              onChange={(e) => setForm({ ...form, style: e.target.value })}
              placeholder="es. conciso, tecnico"
            />
          </div>
          <div className="space-y-2">
            <Label>Keywords (separati da virgola)</Label>
            <Input
              value={form.keywords}
              onChange={(e) => setForm({ ...form, keywords: e.target.value })}
              placeholder="keyword1, keyword2"
            />
          </div>
          <div className="space-y-2">
            <Label>Parole vietate (separati da virgola)</Label>
            <Input
              value={form.forbiddenWords}
              onChange={(e) => setForm({ ...form, forbiddenWords: e.target.value })}
              placeholder="parola1, parola2"
            />
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Target Audience</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label>Descrizione</Label>
            <Textarea
              value={form.audienceDescription}
              onChange={(e) => setForm({ ...form, audienceDescription: e.target.value })}
              placeholder="Descrivi il tuo target audience"
            />
          </div>
          <div className="space-y-2">
            <Label>Fascia di eta</Label>
            <Input
              value={form.ageRange}
              onChange={(e) => setForm({ ...form, ageRange: e.target.value })}
              placeholder="es. 25-45"
            />
          </div>
          <div className="space-y-2">
            <Label>Interessi (separati da virgola)</Label>
            <Input
              value={form.interests}
              onChange={(e) => setForm({ ...form, interests: e.target.value })}
              placeholder="interesse1, interesse2"
            />
          </div>
          <div className="space-y-2">
            <Label>Pain Points (separati da virgola)</Label>
            <Input
              value={form.painPoints}
              onChange={(e) => setForm({ ...form, painPoints: e.target.value })}
              placeholder="problema1, problema2"
            />
          </div>
        </CardContent>
      </Card>

      <div className="flex gap-3">
        <Button onClick={handleSubmit} disabled={saving}>
          {saving ? (
            <><Loader2 className="size-4 animate-spin" /> Creazione...</>
          ) : (
            "Crea progetto"
          )}
        </Button>
        <Link
          href={`/agencies/${agencyId}/projects`}
          className={cn(buttonVariants({ variant: "outline" }))}
        >
          Annulla
        </Link>
      </div>
    </div>
  );
}
