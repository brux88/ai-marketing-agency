import Image from "next/image";
import Link from "next/link";
import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import {
  ArrowRight, PenLine, Share2, Mail, BarChart3, Smartphone,
  Calendar, Clock, Bell, Image as ImageIcon, Users, MessageSquare,
  CheckCircle2, ChevronRight, Zap, Settings, Key, CreditCard
} from "lucide-react";
import { WePostAILogo } from "@/components/ui/wepostai-logo";

const steps = [
  {
    id: "landing",
    title: "Landing Page",
    desc: "La homepage presenta le funzionalita principali, i piani di abbonamento e tutto cio che serve per iniziare.",
    screenshot: "/screenshots/01-landing-hero.png",
  },
  {
    id: "register",
    title: "Registrazione",
    desc: "Crea il tuo account in pochi secondi. Nessuna carta di credito richiesta per la prova gratuita di 14 giorni.",
    screenshot: "/screenshots/02-login.png",
  },
  {
    id: "dashboard",
    title: "Dashboard",
    desc: "La tua area di controllo. Visualizza tutte le agenzie, crea nuove agenzie e accedi rapidamente ai tuoi progetti.",
    screenshot: "/screenshots/03-dashboard.png",
  },
  {
    id: "agency",
    title: "Agenzia",
    desc: "Ogni agenzia ha una panoramica completa: progetti, fonti, contenuti generati, impostazioni globali e gestione team.",
    screenshot: "/screenshots/04-agency-overview.png",
  },
  {
    id: "project",
    title: "Progetto - Panoramica",
    desc: "Il cuore della piattaforma. Da qui lanci i 4 agenti AI: Content Writer, Social Manager, Newsletter e Analytics.",
    screenshot: "/screenshots/05-project-panoramica.png",
  },
  {
    id: "content",
    title: "Contenuti Generati",
    desc: "Visualizza, modifica e approva tutti i contenuti generati dall'AI. Filtro per tipo: Social, Blog e Newsletter.",
    screenshot: "/screenshots/06-project-contenuti.png",
  },
  {
    id: "calendar",
    title: "Calendario Editoriale",
    desc: "Pianifica la pubblicazione dei contenuti con vista lista o mensile. Legenda colorata per stato: bozza, programmato, pubblicato.",
    screenshot: "/screenshots/07-project-calendario.png",
  },
  {
    id: "schedule",
    title: "Programmazione Automatica",
    desc: "Configura scheduler per generare e pubblicare contenuti automaticamente. Imposta orari e frequenze personalizzate.",
    screenshot: "/screenshots/08-project-programmazione.png",
  },
  {
    id: "settings",
    title: "Impostazioni Progetto",
    desc: "Configura approvazione (manuale/automatica), logo overlay per le immagini, notifiche email e connettori social.",
    screenshot: "/screenshots/09-project-impostazioni.png",
  },
  {
    id: "apikeys",
    title: "Chiavi API LLM",
    desc: "Aggiungi le chiavi API del tuo provider AI preferito: OpenAI, Claude, NanoBanana o endpoint custom compatibili.",
    screenshot: "/screenshots/10-chiavi-api.png",
  },
  {
    id: "billing",
    title: "Abbonamento e Fatturazione",
    desc: "Gestisci il tuo piano, monitora l'utilizzo (agenzie e job AI) e passa a un piano superiore quando serve.",
    screenshot: "/screenshots/11-abbonamento.png",
  },
  {
    id: "notifications",
    title: "Centro Notifiche e Job",
    desc: "Monitora tutti i job AI in background e le notifiche. Vedi lo stato di ogni generazione in tempo reale.",
    screenshot: "/screenshots/12-notifiche-jobs.png",
  },
];

const agentDetails = [
  {
    name: "Content Writer",
    icon: PenLine,
    color: "text-blue-600 bg-blue-100 dark:bg-blue-950",
    features: [
      "Genera articoli blog SEO-optimized",
      "Fact-checking automatico dalle fonti",
      "Brand voice personalizzata",
      "Formattazione Markdown",
    ],
  },
  {
    name: "Social Manager",
    icon: Share2,
    color: "text-violet-600 bg-violet-100 dark:bg-violet-950",
    features: [
      "Post per Instagram, LinkedIn, X, Facebook",
      "Hashtag intelligenti",
      "Immagini AI generate automaticamente",
      "Pubblicazione diretta o programmata",
    ],
  },
  {
    name: "Newsletter",
    icon: Mail,
    color: "text-emerald-600 bg-emerald-100 dark:bg-emerald-950",
    features: [
      "Scansione fonti e content curation",
      "Template email professionale",
      "Invio tramite SMTP configurabile",
      "Brand voice e tono personalizzato",
    ],
  },
  {
    name: "Analytics",
    icon: BarChart3,
    color: "text-amber-600 bg-amber-100 dark:bg-amber-950",
    features: [
      "Report performance automatici",
      "Insight AI con suggerimenti",
      "Metriche per canale",
      "Trend e confronti temporali",
    ],
  },
];

export default function GuidePage() {
  return (
    <div>
      {/* Hero */}
      <section className="py-16 px-4 border-b bg-gradient-to-b from-primary/5 to-background">
        <div className="max-w-4xl mx-auto text-center">
          <Badge variant="secondary" className="mb-4">
            <WePostAILogo className="size-4 mr-1" />
            Guida completa
          </Badge>
          <h1 className="text-3xl sm:text-4xl lg:text-5xl font-bold tracking-tight">
            Come funziona WePostAI
          </h1>
          <p className="text-lg text-muted-foreground mt-4 max-w-2xl mx-auto">
            Scopri tutte le funzionalita della piattaforma: dalla configurazione iniziale alla pubblicazione automatica dei contenuti.
          </p>
        </div>
      </section>

      {/* Quick nav */}
      <section className="py-8 px-4 border-b bg-muted/30 sticky top-16 z-40 backdrop-blur-sm">
        <div className="max-w-6xl mx-auto">
          <div className="flex gap-2 overflow-x-auto pb-2 scrollbar-hide">
            {steps.map((step, i) => (
              <a
                key={step.id}
                href={`#${step.id}`}
                className="shrink-0 px-3 py-1.5 rounded-full text-xs font-medium border hover:bg-primary hover:text-primary-foreground transition-colors"
              >
                {i + 1}. {step.title}
              </a>
            ))}
          </div>
        </div>
      </section>

      {/* Steps */}
      <section className="py-16 px-4">
        <div className="max-w-5xl mx-auto space-y-24">
          {steps.map((step, i) => (
            <div key={step.id} id={step.id} className="scroll-mt-32">
              <div className={`flex flex-col ${i % 2 === 0 ? "lg:flex-row" : "lg:flex-row-reverse"} gap-8 items-center`}>
                <div className="lg:w-2/5 space-y-4">
                  <div className="flex items-center gap-3">
                    <span className="size-8 rounded-full bg-primary text-primary-foreground flex items-center justify-center text-sm font-bold">
                      {i + 1}
                    </span>
                    <h2 className="text-2xl font-bold">{step.title}</h2>
                  </div>
                  <p className="text-muted-foreground leading-relaxed">{step.desc}</p>
                </div>
                <div className="lg:w-3/5">
                  <div className="rounded-xl border shadow-lg overflow-hidden bg-muted/50">
                    <div className="flex items-center gap-1.5 px-4 py-2 border-b bg-muted/80">
                      <span className="size-3 rounded-full bg-red-400" />
                      <span className="size-3 rounded-full bg-yellow-400" />
                      <span className="size-3 rounded-full bg-green-400" />
                      <span className="ml-2 text-xs text-muted-foreground">wepostai.com</span>
                    </div>
                    <Image
                      src={step.screenshot}
                      alt={step.title}
                      width={800}
                      height={450}
                      className="w-full h-auto"
                    />
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* Agents deep dive */}
      <section className="py-20 px-4 border-t bg-muted/30">
        <div className="max-w-5xl mx-auto">
          <div className="text-center mb-12">
            <h2 className="text-3xl font-bold tracking-tight">I 4 Agenti AI nel dettaglio</h2>
            <p className="text-muted-foreground mt-3">Ogni agente e specializzato e lavora autonomamente per il tuo brand</p>
          </div>
          <div className="grid sm:grid-cols-2 gap-6">
            {agentDetails.map((agent) => (
              <Card key={agent.name} className="overflow-hidden">
                <CardContent className="p-6">
                  <div className="flex items-center gap-3 mb-4">
                    <div className={`size-10 rounded-lg flex items-center justify-center ${agent.color}`}>
                      <agent.icon className="size-5" />
                    </div>
                    <h3 className="text-lg font-semibold">{agent.name}</h3>
                  </div>
                  <ul className="space-y-2">
                    {agent.features.map((f) => (
                      <li key={f} className="flex items-start gap-2 text-sm">
                        <CheckCircle2 className="size-4 text-primary shrink-0 mt-0.5" />
                        <span className="text-muted-foreground">{f}</span>
                      </li>
                    ))}
                  </ul>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      </section>

      {/* Mobile & Telegram */}
      <section className="py-20 px-4 border-t">
        <div className="max-w-4xl mx-auto text-center">
          <div className="flex justify-center gap-3 mb-6">
            <Smartphone className="size-6 text-primary" />
            <MessageSquare className="size-6 text-primary" />
          </div>
          <h2 className="text-3xl font-bold tracking-tight">Disponibile ovunque</h2>
          <p className="text-lg text-muted-foreground mt-4 max-w-2xl mx-auto">
            Gestisci WePostAI dalla web app, dall&apos;app mobile per iOS e Android, o direttamente dal tuo bot Telegram.
            Approva contenuti, avvia generazioni e ricevi notifiche ovunque tu sia.
          </p>
          <div className="flex justify-center gap-4 mt-8">
            <div className="flex items-center gap-2 px-4 py-2 rounded-lg border">
              <Smartphone className="size-5" />
              <div className="text-left">
                <p className="text-xs text-muted-foreground">App per</p>
                <p className="text-sm font-semibold">iOS &amp; Android</p>
              </div>
            </div>
            <div className="flex items-center gap-2 px-4 py-2 rounded-lg border">
              <MessageSquare className="size-5" />
              <div className="text-left">
                <p className="text-xs text-muted-foreground">Integrazione</p>
                <p className="text-sm font-semibold">Telegram Bot</p>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="py-20 px-4 border-t bg-muted/30">
        <div className="max-w-3xl mx-auto text-center">
          <h2 className="text-3xl font-bold tracking-tight">
            Pronto a iniziare?
          </h2>
          <p className="text-lg text-muted-foreground mt-4">
            Prova WePostAI gratis per 14 giorni. Nessuna carta di credito richiesta.
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
