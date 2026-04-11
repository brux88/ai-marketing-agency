"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { agenciesApi } from "@/lib/api/agencies.api";
import { ApprovalMode, LlmProviderType } from "@/types/api";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Slider } from "@/components/ui/slider";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import {
  ChevronLeft, ChevronRight, Check, Loader2, Sparkles,
  Building2, Users, Mic2, Rss, Brain, ClipboardCheck,
  Plus, X, Info,
} from "lucide-react";

const STEPS = [
  { label: "Brand", icon: Building2 },
  { label: "Audience", icon: Users },
  { label: "Voice", icon: Mic2 },
  { label: "Fonti", icon: Rss },
  { label: "LLM", icon: Brain },
  { label: "Riepilogo", icon: ClipboardCheck },
];

export default function NewAgencyPage() {
  const router = useRouter();
  const [step, setStep] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const [formData, setFormData] = useState({
    name: "",
    productName: "",
    description: "",
    websiteUrl: "",
    audienceDescription: "",
    ageRange: "",
    interests: "",
    painPoints: "",
    tone: "professional",
    style: "",
    keywords: "",
    forbiddenWords: "",
    language: "it",
    contentSources: [""],
    llmProvider: LlmProviderType.OpenAI,
    approvalMode: ApprovalMode.Manual,
    autoApproveMinScore: 7,
  });

  const updateField = (field: string, value: unknown) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async () => {
    setLoading(true);
    setError("");
    try {
      const result = await agenciesApi.create({
        name: formData.name,
        productName: formData.productName,
        description: formData.description,
        websiteUrl: formData.websiteUrl,
        brandVoice: {
          tone: formData.tone,
          style: formData.style,
          keywords: formData.keywords.split(",").map((k) => k.trim()).filter(Boolean),
          examplePhrases: [],
          forbiddenWords: formData.forbiddenWords.split(",").map((w) => w.trim()).filter(Boolean),
          language: formData.language,
        },
        targetAudience: {
          description: formData.audienceDescription,
          ageRange: formData.ageRange,
          interests: formData.interests.split(",").map((i) => i.trim()).filter(Boolean),
          painPoints: formData.painPoints.split(",").map((p) => p.trim()).filter(Boolean),
          personas: [],
        },
        approvalMode: formData.approvalMode,
        autoApproveMinScore: formData.autoApproveMinScore,
      });
      router.push(`/agencies/${result.data.id}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Errore nella creazione");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="max-w-2xl mx-auto">
      <div className="mb-8">
        <h1 className="text-2xl font-bold tracking-tight">Crea nuova agenzia</h1>
        <p className="text-muted-foreground mt-1">Configura la tua agenzia di marketing AI in 6 step</p>
      </div>

      {/* Step indicator */}
      <div className="flex gap-2 mb-8">
        {STEPS.map((s, i) => (
          <button key={s.label} onClick={() => i < step && setStep(i)} className="flex-1 group">
            <div className={`h-1.5 rounded-full transition-colors ${
              i <= step ? "bg-primary" : "bg-muted"
            }`} />
            <div className={`flex items-center gap-1 mt-2 text-xs transition-colors ${
              i === step ? "text-primary font-medium" :
              i < step ? "text-muted-foreground cursor-pointer" : "text-muted-foreground/50"
            }`}>
              <s.icon className="size-3" />
              <span className="hidden sm:inline">{s.label}</span>
            </div>
          </button>
        ))}
      </div>

      {error && (
        <div className="bg-destructive/10 text-destructive text-sm p-3 rounded-lg mb-4">{error}</div>
      )}

      <Card>
        <CardContent className="pt-6">
          {/* Step 1: Brand Info */}
          {step === 0 && (
            <div className="space-y-4">
              <CardTitle className="text-lg">Informazioni brand</CardTitle>
              <div className="space-y-2">
                <Label>Nome agenzia *</Label>
                <Input value={formData.name} onChange={(e) => updateField("name", e.target.value)}
                  placeholder="es. Marketing AI Pro" />
              </div>
              <div className="space-y-2">
                <Label>Nome prodotto *</Label>
                <Input value={formData.productName} onChange={(e) => updateField("productName", e.target.value)}
                  placeholder="es. Il mio SaaS" />
              </div>
              <div className="space-y-2">
                <Label>Descrizione</Label>
                <Textarea value={formData.description} onChange={(e) => updateField("description", e.target.value)}
                  placeholder="Descrivi brevemente il tuo prodotto..." rows={3} />
              </div>
              <div className="space-y-2">
                <Label>Website URL</Label>
                <Input type="url" value={formData.websiteUrl} onChange={(e) => updateField("websiteUrl", e.target.value)}
                  placeholder="https://..." />
              </div>
            </div>
          )}

          {/* Step 2: Target Audience */}
          {step === 1 && (
            <div className="space-y-4">
              <CardTitle className="text-lg">Target audience</CardTitle>
              <div className="space-y-2">
                <Label>Descrizione audience</Label>
                <Textarea value={formData.audienceDescription} onChange={(e) => updateField("audienceDescription", e.target.value)}
                  rows={3} placeholder="Chi sono i tuoi clienti ideali?" />
              </div>
              <div className="space-y-2">
                <Label>Fascia eta</Label>
                <Input value={formData.ageRange} onChange={(e) => updateField("ageRange", e.target.value)}
                  placeholder="es. 25-45" />
              </div>
              <div className="space-y-2">
                <Label>Interessi (separati da virgola)</Label>
                <Input value={formData.interests} onChange={(e) => updateField("interests", e.target.value)}
                  placeholder="marketing, tech, business" />
              </div>
              <div className="space-y-2">
                <Label>Pain points (separati da virgola)</Label>
                <Input value={formData.painPoints} onChange={(e) => updateField("painPoints", e.target.value)}
                  placeholder="poco tempo, budget limitato" />
              </div>
            </div>
          )}

          {/* Step 3: Brand Voice */}
          {step === 2 && (
            <div className="space-y-4">
              <CardTitle className="text-lg">Voce del brand</CardTitle>
              <div className="space-y-2">
                <Label>Tono</Label>
                <select value={formData.tone} onChange={(e) => updateField("tone", e.target.value)}
                  className="flex h-9 w-full rounded-lg border border-input bg-transparent px-3 py-1 text-sm shadow-xs transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring">
                  <option value="professional">Professionale</option>
                  <option value="casual">Casual</option>
                  <option value="playful">Giocoso</option>
                  <option value="authoritative">Autorevole</option>
                  <option value="friendly">Amichevole</option>
                </select>
              </div>
              <div className="space-y-2">
                <Label>Stile di scrittura</Label>
                <Textarea value={formData.style} onChange={(e) => updateField("style", e.target.value)}
                  rows={2} placeholder="Descrivi il tuo stile di scrittura" />
              </div>
              <div className="space-y-2">
                <Label>Keyword (separate da virgola)</Label>
                <Input value={formData.keywords} onChange={(e) => updateField("keywords", e.target.value)}
                  placeholder="innovazione, qualita, semplicita" />
              </div>
              <div className="space-y-2">
                <Label>Parole vietate (separate da virgola)</Label>
                <Input value={formData.forbiddenWords} onChange={(e) => updateField("forbiddenWords", e.target.value)}
                  placeholder="spam, gratis, urgente" />
              </div>
              <div className="space-y-2">
                <Label>Lingua</Label>
                <select value={formData.language} onChange={(e) => updateField("language", e.target.value)}
                  className="flex h-9 w-full rounded-lg border border-input bg-transparent px-3 py-1 text-sm shadow-xs transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring">
                  <option value="it">Italiano</option>
                  <option value="en">English</option>
                  <option value="es">Espanol</option>
                  <option value="fr">Francais</option>
                  <option value="de">Deutsch</option>
                </select>
              </div>
            </div>
          )}

          {/* Step 4: Content Sources */}
          {step === 3 && (
            <div className="space-y-4">
              <CardTitle className="text-lg">Fonti di contenuto</CardTitle>
              <p className="text-sm text-muted-foreground">Aggiungi URL di feed RSS, siti web o pagine social da monitorare</p>
              {formData.contentSources.map((url, i) => (
                <div key={i} className="flex gap-2">
                  <Input value={url}
                    onChange={(e) => {
                      const sources = [...formData.contentSources];
                      sources[i] = e.target.value;
                      updateField("contentSources", sources);
                    }}
                    placeholder="https://..." className="flex-1" />
                  {formData.contentSources.length > 1 && (
                    <Button type="button" variant="ghost" size="icon"
                      onClick={() => updateField("contentSources", formData.contentSources.filter((_, j) => j !== i))}>
                      <X className="size-4" />
                    </Button>
                  )}
                </div>
              ))}
              <Button type="button" variant="outline" size="sm"
                onClick={() => updateField("contentSources", [...formData.contentSources, ""])}>
                <Plus className="size-4" /> Aggiungi fonte
              </Button>
            </div>
          )}

          {/* Step 5: LLM Config */}
          {step === 4 && (
            <div className="space-y-4">
              <CardTitle className="text-lg">Configurazione AI</CardTitle>
              <div className="space-y-2">
                <Label>Provider LLM</Label>
                <select value={formData.llmProvider}
                  onChange={(e) => updateField("llmProvider", Number(e.target.value))}
                  className="flex h-9 w-full rounded-lg border border-input bg-transparent px-3 py-1 text-sm shadow-xs transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring">
                  <option value={LlmProviderType.OpenAI}>OpenAI (GPT-4)</option>
                  <option value={LlmProviderType.Anthropic}>Anthropic (Claude)</option>
                  <option value={LlmProviderType.NanoBanana}>NanoBanana</option>
                  <option value={LlmProviderType.AzureOpenAI}>Azure OpenAI</option>
                  <option value={LlmProviderType.Custom}>Custom endpoint</option>
                </select>
              </div>
              <div className="space-y-2">
                <Label>Modalita approvazione</Label>
                <select value={formData.approvalMode}
                  onChange={(e) => updateField("approvalMode", Number(e.target.value))}
                  className="flex h-9 w-full rounded-lg border border-input bg-transparent px-3 py-1 text-sm shadow-xs transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring">
                  <option value={ApprovalMode.Manual}>Manuale - rivedi tutto</option>
                  <option value={ApprovalMode.AutoApprove}>Auto-approva tutto</option>
                  <option value={ApprovalMode.AutoApproveAboveScore}>Auto-approva sopra punteggio minimo</option>
                </select>
              </div>
              {formData.approvalMode === ApprovalMode.AutoApproveAboveScore && (
                <div className="space-y-3">
                  <Label>Punteggio minimo: {formData.autoApproveMinScore}/10</Label>
                  <Slider
                    min={1} max={10} step={1}
                    value={[formData.autoApproveMinScore]}
                    onValueChange={(val) => updateField("autoApproveMinScore", Array.isArray(val) ? val[0] : val)}
                  />
                </div>
              )}
              <div className="flex items-start gap-2 bg-muted/50 p-3 rounded-lg text-sm text-muted-foreground">
                <Info className="size-4 shrink-0 mt-0.5" />
                Le chiavi API si configurano in Impostazioni &gt; Chiavi API. Qui selezioni solo il provider.
              </div>
            </div>
          )}

          {/* Step 6: Review */}
          {step === 5 && (
            <div className="space-y-4">
              <CardTitle className="text-lg">Riepilogo</CardTitle>
              <div className="space-y-0 divide-y">
                {[
                  { label: "Nome", value: formData.name },
                  { label: "Prodotto", value: formData.productName },
                  { label: "Tono", value: formData.tone },
                  { label: "Lingua", value: formData.language.toUpperCase() },
                  { label: "Fonti", value: `${formData.contentSources.filter(Boolean).length} configurate` },
                  {
                    label: "Approvazione",
                    value: formData.approvalMode === ApprovalMode.Manual ? "Manuale" :
                           formData.approvalMode === ApprovalMode.AutoApprove ? "Automatica" :
                           `Auto sopra ${formData.autoApproveMinScore}/10`
                  },
                ].map((row) => (
                  <div key={row.label} className="flex justify-between py-3 text-sm">
                    <span className="text-muted-foreground">{row.label}</span>
                    <span className="font-medium">{row.value || "—"}</span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Navigation */}
      <div className="flex justify-between mt-6">
        <Button variant="outline" onClick={() => setStep((s) => Math.max(0, s - 1))} disabled={step === 0}>
          <ChevronLeft className="size-4" /> Indietro
        </Button>
        {step < STEPS.length - 1 ? (
          <Button onClick={() => setStep((s) => s + 1)}>
            Avanti <ChevronRight className="size-4" />
          </Button>
        ) : (
          <Button onClick={handleSubmit} disabled={loading}>
            {loading ? (
              <><Loader2 className="size-4 animate-spin" /> Creazione...</>
            ) : (
              <><Sparkles className="size-4" /> Crea Agenzia</>
            )}
          </Button>
        )}
      </div>
    </div>
  );
}
