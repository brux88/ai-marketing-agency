"use client";

import { useState } from "react";
import Link from "next/link";
import { Button, buttonVariants } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import {
  PenLine, Share2, Mail, BarChart3, Check, ArrowRight, Zap, Shield,
  Calendar, Bot, Bell, Image, Smartphone, Globe, Users, Clock, MessageSquare,
  Download, Monitor, ChevronRight, Loader2, CheckCircle2
} from "lucide-react";
import { Logo } from "@/components/shared/Logo";

const features = [
  {
    title: "Content Writer",
    desc: "Genera articoli blog SEO-optimized da fonti reali, con fact-checking automatico e brand voice personalizzata.",
    icon: PenLine,
    color: "text-blue-600 bg-blue-100 dark:bg-blue-950",
  },
  {
    title: "Social Manager",
    desc: "Crea post per Instagram, LinkedIn, X e Facebook con calendario editoriale intelligente e pubblicazione automatica.",
    icon: Share2,
    color: "text-violet-600 bg-violet-100 dark:bg-violet-950",
  },
  {
    title: "Newsletter",
    desc: "Scansiona fonti, cura i contenuti migliori e genera newsletter professionali in brand voice.",
    icon: Mail,
    color: "text-emerald-600 bg-emerald-100 dark:bg-emerald-950",
  },
  {
    title: "Analytics",
    desc: "Analizza le performance, genera report con insight AI e suggerisce miglioramenti concreti.",
    icon: BarChart3,
    color: "text-amber-600 bg-amber-100 dark:bg-amber-950",
  },
];

const capabilities = [
  {
    title: "Calendario Editoriale",
    desc: "Pianifica e visualizza tutti i tuoi contenuti in un calendario mensile interattivo. Pubblica direttamente o programma la pubblicazione automatica.",
    icon: Calendar,
  },
  {
    title: "Bot Telegram Integrato",
    desc: "Gestisci tutto dal tuo smartphone: approva contenuti, avvia generazioni, visualizza statistiche e ricevi notifiche in tempo reale.",
    icon: MessageSquare,
  },
  {
    title: "Generazione Immagini AI",
    desc: "Immagini generate automaticamente per ogni post con overlay del logo e banner personalizzati. Supporto DALL-E e Stability AI.",
    icon: Image,
  },
  {
    title: "Multi-progetto",
    desc: "Gestisci piu brand e progetti dalla stessa piattaforma. Ogni progetto ha la sua brand voice, audience e configurazione.",
    icon: Users,
  },
  {
    title: "Scheduling Automatico",
    desc: "Configura scheduler per generazione e pubblicazione automatica. Imposta orari, frequenze e lascia fare all'AI.",
    icon: Clock,
  },
  {
    title: "Notifiche Multicanale",
    desc: "Ricevi notifiche via Telegram, email e web quando i contenuti vengono generati, approvati o pubblicati.",
    icon: Bell,
  },
];

const howItWorks = [
  {
    step: "1",
    title: "Configura il tuo brand",
    desc: "Inserisci il sito web del tuo progetto e l'AI estrarra automaticamente il tono di voce, il pubblico target e il contesto del brand.",
  },
  {
    step: "2",
    title: "Collega i tuoi canali",
    desc: "Collega Instagram, LinkedIn, Facebook, X e il tuo bot Telegram. Configura le chiavi API del tuo provider AI preferito.",
  },
  {
    step: "3",
    title: "Genera e approva",
    desc: "L'AI genera contenuti personalizzati. Approva con un click da web o Telegram, e la pubblicazione avviene automaticamente.",
  },
  {
    step: "4",
    title: "Automatizza",
    desc: "Configura gli scheduler per generazione e pubblicazione automatica. L'AI lavora 24/7 mentre tu ti concentri sul business.",
  },
];

const plans = [
  {
    name: "Basic",
    price: "29",
    agencies: "3",
    jobs: "100",
    features: ["3 agenzie", "100 job AI/mese", "Blog + Social", "Calendario editoriale", "Supporto email"],
  },
  {
    name: "Pro",
    price: "79",
    agencies: "10",
    jobs: "500",
    popular: true,
    features: ["10 agenzie", "500 job AI/mese", "Tutti i canali social", "Bot Telegram", "Newsletter", "Immagini AI", "Supporto prioritario"],
  },
  {
    name: "Enterprise",
    price: "199",
    agencies: "Illimitate",
    jobs: "2000",
    features: ["Agenzie illimitate", "2000 job AI/mese", "API personalizzate", "Account manager", "SLA garantito", "White-label"],
  },
];

const blogPosts = [
  {
    title: "Come l'AI sta rivoluzionando il content marketing",
    excerpt: "Scopri come gli agenti AI possono generare contenuti di qualita mantenendo il tono di voce del tuo brand.",
    category: "wepostai.com",
    date: "15 Apr 2026",
    href: "/blog/ai-content-marketing",
  },
  {
    title: "Guida completa al Social Media Marketing automatizzato",
    excerpt: "Dalla pianificazione alla pubblicazione: come automatizzare il tuo flusso di lavoro sui social media.",
    category: "Social Media",
    date: "12 Apr 2026",
    href: "/blog/social-media-automatizzato",
  },
  {
    title: "Newsletter che convertono: strategie AI-powered",
    excerpt: "Come usare l'intelligenza artificiale per creare newsletter che i tuoi iscritti vogliono davvero leggere.",
    category: "Newsletter",
    date: "10 Apr 2026",
    href: "/blog/newsletter-ai-powered",
  },
];

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

function NewsletterForm() {
  const [email, setEmail] = useState("");
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      const res = await fetch(`${API_URL}/api/v1/public/newsletter/subscribe`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email }),
      });
      if (!res.ok) throw new Error("Errore durante l'iscrizione");
      setSuccess(true);
      setEmail("");
    } catch {
      setError("Errore durante l'iscrizione. Riprova.");
    } finally {
      setLoading(false);
    }
  };

  if (success) {
    return (
      <div className="flex items-center gap-2 justify-center text-emerald-600 dark:text-emerald-400">
        <CheckCircle2 className="size-5" />
        <span className="font-medium">Iscrizione completata! Riceverai le nostre novita.</span>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-col sm:flex-row gap-3 max-w-md mx-auto">
      <Input
        type="email"
        required
        placeholder="La tua email"
        value={email}
        onChange={(e) => setEmail(e.target.value)}
        className="flex-1 h-11 bg-background"
      />
      <Button type="submit" disabled={loading} className="h-11 px-6">
        {loading ? (
          <><Loader2 className="size-4 animate-spin" /> Iscrizione...</>
        ) : (
          <><Mail className="size-4" /> Iscriviti</>
        )}
      </Button>
      {error && <p className="text-destructive text-sm">{error}</p>}
    </form>
  );
}

export default function LandingPage() {
  return (
    <div>
      {/* Hero */}
      <section className="relative overflow-hidden py-24 px-4">
        <div className="absolute inset-0 bg-gradient-to-b from-primary/5 via-background to-background" />
        <div className="relative max-w-4xl mx-auto text-center">
          <Badge variant="secondary" className="mb-6">
            <Logo variant="mark" size="sm" />
            Prova gratuita 14 giorni
          </Badge>
          <h1 className="text-4xl sm:text-5xl lg:text-6xl font-bold tracking-tight leading-tight">
            La tua agenzia di marketing gestita da{" "}
            <span className="text-primary">agenti AI</span>
          </h1>
          <p className="text-lg sm:text-xl text-muted-foreground mt-6 max-w-2xl mx-auto leading-relaxed">
            Crea contenuti blog, gestisci i social media, invia newsletter e analizza le performance.
            Tutto automatizzato con intelligenza artificiale.
          </p>
          <div className="mt-10 flex flex-col sm:flex-row gap-3 justify-center">
            <Link href="/register" className={cn(buttonVariants({ size: "lg" }), "gap-1")}>
              Inizia gratis per 14 giorni
              <ArrowRight className="size-4" />
            </Link>
            <Link href="#features" className={cn(buttonVariants({ variant: "outline", size: "lg" }))}>
              Scopri le funzionalita
            </Link>
          </div>
          <div className="mt-8 flex justify-center gap-6 text-sm text-muted-foreground">
            <span className="flex items-center gap-1"><Check className="size-3.5 text-primary" /> No carta di credito</span>
            <span className="flex items-center gap-1"><Check className="size-3.5 text-primary" /> Setup in 2 minuti</span>
            <span className="flex items-center gap-1"><Check className="size-3.5 text-primary" /> Cancella quando vuoi</span>
          </div>
        </div>
      </section>

      {/* Features */}
      <section id="features" className="py-24 px-4">
        <div className="max-w-7xl mx-auto">
          <div className="text-center mb-16">
            <h2 className="text-3xl sm:text-4xl font-bold tracking-tight">
              4 agenti AI al tuo servizio
            </h2>
            <p className="text-muted-foreground mt-3 max-w-xl mx-auto">
              Ogni agente e specializzato in un aspetto del marketing digitale e lavora 24/7 per il tuo brand.
            </p>
          </div>
          <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {features.map((f) => (
              <Card key={f.title} className="group hover:shadow-lg transition-all duration-200 hover:-translate-y-0.5">
                <CardHeader>
                  <div className={`size-12 rounded-xl flex items-center justify-center mb-2 ${f.color}`}>
                    <f.icon className="size-6" />
                  </div>
                  <CardTitle className="text-lg">{f.title}</CardTitle>
                </CardHeader>
                <CardContent>
                  <p className="text-muted-foreground text-sm leading-relaxed">{f.desc}</p>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      </section>

      {/* How it works */}
      <section className="py-20 px-4 border-y bg-muted/30">
        <div className="max-w-5xl mx-auto">
          <div className="text-center mb-16">
            <h2 className="text-3xl sm:text-4xl font-bold tracking-tight">Come funziona</h2>
            <p className="text-muted-foreground mt-3">Inizia in pochi minuti, automatizza in pochi click</p>
          </div>
          <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-8">
            {howItWorks.map((item) => (
              <div key={item.step} className="text-center">
                <div className="size-12 rounded-full bg-primary text-primary-foreground flex items-center justify-center text-xl font-bold mx-auto mb-4">
                  {item.step}
                </div>
                <h3 className="font-semibold text-lg mb-2">{item.title}</h3>
                <p className="text-sm text-muted-foreground leading-relaxed">{item.desc}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Capabilities */}
      <section className="py-24 px-4">
        <div className="max-w-7xl mx-auto">
          <div className="text-center mb-16">
            <h2 className="text-3xl sm:text-4xl font-bold tracking-tight">Tutto cio di cui hai bisogno</h2>
            <p className="text-muted-foreground mt-3">Funzionalita avanzate per gestire il tuo marketing</p>
          </div>
          <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
            {capabilities.map((c) => (
              <Card key={c.title} className="hover:shadow-md transition-shadow">
                <CardHeader className="pb-3">
                  <div className="flex items-center gap-3">
                    <div className="size-10 rounded-lg bg-primary/10 flex items-center justify-center">
                      <c.icon className="size-5 text-primary" />
                    </div>
                    <CardTitle className="text-base">{c.title}</CardTitle>
                  </div>
                </CardHeader>
                <CardContent>
                  <p className="text-sm text-muted-foreground leading-relaxed">{c.desc}</p>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      </section>

      {/* BYO Keys */}
      <section className="py-20 px-4 border-y bg-muted/30">
        <div className="max-w-4xl mx-auto text-center">
          <div className="flex justify-center gap-3 mb-6">
            <Zap className="size-6 text-primary" />
            <Shield className="size-6 text-primary" />
          </div>
          <h2 className="text-3xl font-bold tracking-tight">Usa il tuo provider AI preferito</h2>
          <p className="text-lg text-muted-foreground mt-4 max-w-2xl mx-auto">
            Supportiamo OpenAI, Claude (Anthropic), NanoBanana e qualsiasi endpoint compatibile.
            Inserisci le tue chiavi API e sei pronto.
          </p>
          <div className="flex justify-center gap-8 items-center mt-8 text-muted-foreground/60">
            <span className="text-xl font-bold">OpenAI</span>
            <span className="text-xl font-bold">Claude</span>
            <span className="text-xl font-bold">NanoBanana</span>
            <span className="text-xl font-bold">Custom</span>
          </div>
        </div>
      </section>

      {/* Mobile App */}
      <section className="py-20 px-4">
        <div className="max-w-5xl mx-auto">
          <div className="text-center mb-12">
            <h2 className="text-3xl sm:text-4xl font-bold tracking-tight">Gestisci tutto dal tuo smartphone</h2>
            <p className="text-muted-foreground mt-3 max-w-2xl mx-auto">
              Scarica l&apos;app wepostai.com per iOS e Android. Approva contenuti, avvia generazioni AI e monitora le performance ovunque tu sia.
            </p>
          </div>
          <div className="grid sm:grid-cols-3 gap-6 max-w-3xl mx-auto">
            <div className="text-center p-6 rounded-xl border bg-card">
              <div className="size-12 rounded-full bg-primary/10 flex items-center justify-center mx-auto mb-3">
                <Smartphone className="size-6 text-primary" />
              </div>
              <h3 className="font-semibold mb-1">App Nativa</h3>
              <p className="text-sm text-muted-foreground">Esperienza fluida su iOS e Android con notifiche push in tempo reale</p>
            </div>
            <div className="text-center p-6 rounded-xl border bg-card">
              <div className="size-12 rounded-full bg-primary/10 flex items-center justify-center mx-auto mb-3">
                <Bell className="size-6 text-primary" />
              </div>
              <h3 className="font-semibold mb-1">Notifiche Push</h3>
              <p className="text-sm text-muted-foreground">Ricevi alert quando i contenuti sono pronti per l&apos;approvazione</p>
            </div>
            <div className="text-center p-6 rounded-xl border bg-card">
              <div className="size-12 rounded-full bg-primary/10 flex items-center justify-center mx-auto mb-3">
                <MessageSquare className="size-6 text-primary" />
              </div>
              <h3 className="font-semibold mb-1">Bot Telegram</h3>
              <p className="text-sm text-muted-foreground">In alternativa, gestisci tutto via Telegram senza installare nulla</p>
            </div>
          </div>
          <div className="flex justify-center gap-4 mt-8">
            <span className="inline-flex items-center gap-2 px-5 py-2.5 rounded-lg bg-foreground text-background text-sm font-medium">
              <Download className="size-4" /> App Store
            </span>
            <span className="inline-flex items-center gap-2 px-5 py-2.5 rounded-lg bg-foreground text-background text-sm font-medium">
              <Download className="size-4" /> Google Play
            </span>
          </div>
          <p className="text-center text-xs text-muted-foreground mt-3">Disponibile prossimamente su App Store e Google Play</p>
        </div>
      </section>

      {/* Pricing */}
      <section className="py-24 px-4 border-t">
        <div className="max-w-6xl mx-auto">
          <div className="text-center mb-16">
            <h2 className="text-3xl sm:text-4xl font-bold tracking-tight">Piani e prezzi</h2>
            <p className="text-muted-foreground mt-3">Scegli il piano giusto per il tuo business</p>
          </div>
          <div className="grid md:grid-cols-3 gap-6 max-w-4xl mx-auto">
            {plans.map((plan) => (
              <Card
                key={plan.name}
                className={`relative flex flex-col ${plan.popular ? "border-primary shadow-lg ring-1 ring-primary/20 scale-105" : ""}`}
              >
                {plan.popular && (
                  <div className="absolute -top-3 left-1/2 -translate-x-1/2 z-10">
                    <Badge className="px-3 py-1 whitespace-nowrap shadow-sm">Piu popolare</Badge>
                  </div>
                )}
                <CardHeader className="pt-6">
                  <CardTitle>{plan.name}</CardTitle>
                  <div className="mt-2">
                    <span className="text-4xl font-bold">&euro;{plan.price}</span>
                    <span className="text-muted-foreground">/mese</span>
                  </div>
                </CardHeader>
                <CardContent className="flex-1">
                  <ul className="space-y-2.5 text-sm">
                    {plan.features.map((f) => (
                      <li key={f} className="flex items-center gap-2">
                        <Check className="size-4 text-primary shrink-0" />
                        {f}
                      </li>
                    ))}
                  </ul>
                </CardContent>
                <CardFooter>
                  <Link
                    href="/register"
                    className={cn(buttonVariants({ variant: plan.popular ? "default" : "outline" }), "w-full")}
                  >
                    Inizia ora
                  </Link>
                </CardFooter>
              </Card>
            ))}
          </div>
        </div>
      </section>

      {/* Blog */}
      <section id="blog" className="py-24 px-4 border-t bg-muted/30">
        <div className="max-w-6xl mx-auto">
          <div className="text-center mb-16">
            <h2 className="text-3xl sm:text-4xl font-bold tracking-tight">Blog</h2>
            <p className="text-muted-foreground mt-3">Guide, strategie e novita dal mondo dell'AI marketing</p>
          </div>
          <div className="grid md:grid-cols-3 gap-6">
            {blogPosts.map((post) => (
              <Link key={post.title} href={post.href}>
                <Card className="hover:shadow-md transition-shadow cursor-pointer h-full">
                  <CardHeader>
                    <div className="flex items-center gap-2 mb-2">
                      <Badge variant="secondary" className="text-xs">{post.category}</Badge>
                      <span className="text-xs text-muted-foreground">{post.date}</span>
                    </div>
                    <CardTitle className="text-lg leading-snug">{post.title}</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <p className="text-sm text-muted-foreground leading-relaxed">{post.excerpt}</p>
                  </CardContent>
                  <CardFooter>
                    <span className="text-sm text-primary flex items-center gap-1 hover:underline">
                      Leggi di piu <ArrowRight className="size-3" />
                    </span>
                  </CardFooter>
                </Card>
              </Link>
            ))}
          </div>
        </div>
      </section>

      {/* Newsletter */}
      <section className="py-20 px-4 border-t">
        <div className="max-w-2xl mx-auto text-center space-y-6">
          <div className="size-14 rounded-full bg-primary/10 flex items-center justify-center mx-auto">
            <Mail className="size-7 text-primary" />
          </div>
          <h2 className="text-3xl font-bold tracking-tight">Resta aggiornato</h2>
          <p className="text-muted-foreground max-w-lg mx-auto">
            Iscriviti alla newsletter per ricevere guide, strategie di AI marketing e novita sulla piattaforma. Niente spam, solo contenuti utili.
          </p>
          <NewsletterForm />
          <p className="text-xs text-muted-foreground">
            Puoi cancellarti in qualsiasi momento. Leggi la nostra privacy policy.
          </p>
        </div>
      </section>

      {/* CTA */}
      <section className="py-24 px-4">
        <div className="max-w-3xl mx-auto text-center">
          <h2 className="text-3xl sm:text-4xl font-bold tracking-tight">
            Pronto a trasformare il tuo marketing?
          </h2>
          <p className="text-lg text-muted-foreground mt-4">
            Unisciti a centinaia di aziende che hanno gia automatizzato il loro content marketing con l'AI.
          </p>
          <div className="mt-8">
            <Link href="/register" className={cn(buttonVariants({ size: "lg" }), "gap-1")}>
              Inizia gratis per 14 giorni
              <ArrowRight className="size-4" />
            </Link>
          </div>
        </div>
      </section>
    </div>
  );
}
