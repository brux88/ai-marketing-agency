import Link from "next/link";
import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import {
  ArrowRight, PenLine, Share2, Mail, BarChart3, Smartphone,
  Clock, MessageSquare, CheckCircle2,
  Play, Repeat, Target, FileText, Globe
} from "lucide-react";
import { Logo } from "@/components/shared/Logo";
import { GuideScreenshots } from "./guide-screenshots";

const steps = [
  {
    id: "landing",
    title: "Landing Page",
    desc: "La homepage presenta in modo chiaro tutte le funzionalita della piattaforma. Troverai una panoramica dei piani di abbonamento con i relativi limiti (agenzie, progetti, job AI mensili), le testimonianze degli utenti e una sezione FAQ. Da qui puoi registrarti o accedere al tuo account.",
    screenshot: "/screenshots/01-landing-hero.png",
    details: [
      "Panoramica completa dei 4 agenti AI disponibili",
      "Confronto piani: Free Trial, Basic, Pro, Enterprise",
      "Link diretto alla guida e alla documentazione",
    ],
  },
  {
    id: "register",
    title: "Registrazione e Accesso",
    desc: "Crea il tuo account inserendo email, nome completo e password. Riceverai un'email di conferma che dovrai verificare prima di accedere. La prova gratuita di 14 giorni include 1 agenzia, 3 progetti e 50 job AI mensili senza carta di credito.",
    screenshot: "/screenshots/02-login.png",
    details: [
      "Conferma email obbligatoria per attivare l'account",
      "Password dimenticata con link di reset via email",
      "Accesso diretto dopo la conferma email",
    ],
  },
  {
    id: "dashboard",
    title: "Dashboard",
    desc: "La dashboard e il tuo centro di controllo. Visualizza tutte le agenzie create, il numero di progetti attivi per ciascuna, e le statistiche di utilizzo del piano. Da qui puoi creare nuove agenzie, accedere ai progetti esistenti e monitorare i job AI in background.",
    screenshot: "/screenshots/03-dashboard.png",
    details: [
      "Card per ogni agenzia con conteggio progetti e contenuti",
      "Pulsante rapido per creare una nuova agenzia",
      "Barra laterale con navigazione a tutte le sezioni",
      "Centro notifiche per job in tempo reale",
    ],
  },
  {
    id: "agency",
    title: "Panoramica Agenzia",
    desc: "Ogni agenzia rappresenta un brand o un cliente. La pagina mostra tutti i progetti collegati, le fonti di contenuto configurate, le statistiche aggregate e le impostazioni globali. Puoi anche estrarre automaticamente il brand (colori, tono, descrizione) dal sito web del cliente.",
    screenshot: "/screenshots/04-agency-overview.png",
    details: [
      "Estrazione automatica del brand dal sito web del cliente",
      "Gestione fonti di contenuto (RSS, siti web, documenti)",
      "Statistiche aggregate di tutti i progetti dell'agenzia",
      "Configurazione team e permessi per collaboratori",
    ],
  },
  {
    id: "project",
    title: "Progetto - Panoramica",
    desc: "Il cuore della piattaforma. Ogni progetto ha 4 tab principali: Panoramica, Contenuti, Calendario e Programmazione. Dalla panoramica puoi lanciare manualmente ciascuno dei 4 agenti AI (Content Writer, Social Manager, Newsletter, Analytics) e vedere le statistiche del progetto.",
    screenshot: "/screenshots/05-project-panoramica.png",
    details: [
      "4 agenti AI lanciabili con un click",
      "Statistiche: contenuti generati, pubblicati, in attesa",
      "Configurazione rapida dei connettori social",
      "Anteprima degli ultimi contenuti generati",
    ],
  },
  {
    id: "content",
    title: "Contenuti Generati",
    desc: "La sezione contenuti mostra tutti i post, articoli e newsletter generati dall'AI. Puoi filtrare per tipo (Social, Blog, Newsletter), per stato (bozza, approvato, pubblicato, rifiutato) e per data. Ogni contenuto e modificabile prima della pubblicazione.",
    screenshot: "/screenshots/06-project-contenuti.png",
    details: [
      "Filtri avanzati: tipo, stato, data, piattaforma",
      "Modifica testo e immagini prima della pubblicazione",
      "Approvazione manuale o automatica (configurabile)",
      "Anteprima del post come apparira su ogni piattaforma",
      "Azioni rapide: approva, rifiuta, modifica, elimina",
    ],
  },
  {
    id: "calendar",
    title: "Calendario Editoriale",
    desc: "Il calendario offre due viste: lista cronologica e vista mensile. Ogni contenuto e colorato in base al suo stato: grigio per le bozze, blu per i programmati, verde per i pubblicati, rosso per i rifiutati. Puoi trascinare i contenuti per riprogrammarli e cliccare per vederne il dettaglio.",
    screenshot: "/screenshots/07-project-calendario.png",
    details: [
      "Vista lista e vista calendario mensile",
      "Legenda colori: bozza, programmato, pubblicato, rifiutato",
      "Dettaglio completo cliccando su ogni contenuto",
      "Filtri per tipo di contenuto e piattaforma",
    ],
  },
  {
    id: "schedule",
    title: "Programmazione e Scheduler",
    desc: "La sezione programmazione ti permette di automatizzare completamente la generazione e pubblicazione dei contenuti. Puoi creare piu scheduler, ognuno con il proprio agente AI, frequenza e orario. Lo scheduler esegue automaticamente il job secondo la configurazione impostata.",
    screenshot: "/screenshots/08-project-programmazione.png",
    details: null,
  },
  {
    id: "settings",
    title: "Impostazioni Progetto",
    desc: "Configura ogni aspetto del progetto. L'approvazione manuale richiede il tuo ok prima della pubblicazione. Puoi aggiungere un logo overlay sulle immagini generate, configurare notifiche email per nuovi contenuti e gestire i connettori social (Instagram, LinkedIn, X, Facebook, Telegram).",
    screenshot: "/screenshots/09-project-impostazioni.png",
    details: [
      "Modalita approvazione: manuale (rivedi tutto) o automatica (pubblica subito)",
      "Logo overlay personalizzato sulle immagini AI",
      "Notifiche email per nuovi contenuti generati",
      "Connettori social: Instagram, LinkedIn, X, Facebook",
      "Integrazione Telegram per notifiche e approvazioni",
      "Configurazione email SMTP per newsletter",
    ],
  },
  {
    id: "apikeys",
    title: "Chiavi API e Provider LLM",
    desc: "wepostai.com supporta diversi provider AI. Puoi configurare le tue chiavi API per OpenAI (GPT-4), Anthropic (Claude), NanoBanana o qualsiasi endpoint compatibile con l'API OpenAI. Ogni chiave viene testata automaticamente al salvataggio per verificarne la validita.",
    screenshot: "/screenshots/10-chiavi-api.png",
    details: [
      "Provider supportati: OpenAI, Anthropic Claude, NanoBanana",
      "Endpoint custom compatibili con API OpenAI",
      "Test automatico della chiave al salvataggio",
      "Selezione del modello specifico per ogni provider",
      "Le chiavi sono crittografate e non visibili dopo il salvataggio",
    ],
  },
  {
    id: "billing",
    title: "Abbonamento e Fatturazione",
    desc: "La sezione billing mostra il tuo piano attuale, l'utilizzo corrente (agenzie, progetti e job AI usati vs disponibili) e ti permette di fare upgrade. I pagamenti sono gestiti tramite Stripe con fatturazione mensile o annuale.",
    screenshot: "/screenshots/11-abbonamento.png",
    details: [
      "Monitoraggio utilizzo: agenzie, progetti, job AI mensili",
      "Upgrade/downgrade del piano in qualsiasi momento",
      "Fatturazione tramite Stripe (carte, SEPA)",
      "Piano Free Trial: 1 agenzia, 3 progetti, 50 job/mese",
      "Piano Pro: 10 agenzie, 50 progetti, 500 job/mese",
    ],
  },
  {
    id: "notifications",
    title: "Centro Notifiche e Job",
    desc: "Il centro notifiche mostra in tempo reale lo stato di tutti i job AI. Ogni generazione (contenuto, newsletter, analisi) viene tracciata con stato, durata e risultato. Puoi vedere i job completati, in corso, falliti e in coda.",
    screenshot: "/screenshots/12-notifiche-jobs.png",
    details: [
      "Job in tempo reale con aggiornamento automatico",
      "Stati: in coda, in esecuzione, completato, fallito",
      "Dettaglio errore per i job falliti",
      "Storico completo di tutte le generazioni",
      "Filtri per tipo di job e stato",
    ],
  },
  {
    id: "team",
    title: "Gestione Team",
    desc: "Invita collaboratori al tuo workspace via email. Ogni membro puo avere un ruolo (Owner, Admin, Member) e permessi granulari: accesso limitato a specifiche agenzie o progetti, possibilita di creare progetti e gestire chiavi API.",
    screenshot: null,
    details: [
      "Inviti via email con link sicuro (scadenza 7 giorni)",
      "Ruoli: Owner (tutto), Admin (gestione), Member (operativo)",
      "Permessi granulari per agenzia e progetto",
      "Revoca inviti e rimozione membri",
      "Il membro invitato crea il suo account dal link email",
    ],
  },
  {
    id: "profile",
    title: "Profilo e Sicurezza",
    desc: "Dalla sezione profilo puoi modificare il tuo nome, cambiare la password e richiedere l'eliminazione dell'account. L'eliminazione richiede una conferma via email ed e irreversibile: tutti i dati, agenzie, progetti e contenuti vengono eliminati.",
    screenshot: null,
    details: [
      "Modifica nome completo",
      "Cambio password con verifica della password attuale",
      "Eliminazione account con doppia conferma via email",
      "Tutti i dati vengono eliminati definitivamente",
    ],
  },
];

const agentDetails = [
  {
    name: "Content Writer",
    icon: PenLine,
    color: "text-blue-600 bg-blue-100 dark:bg-blue-950",
    desc: "Genera articoli blog ottimizzati per la SEO partendo dalle fonti di contenuto configurate. Analizza le fonti, verifica i fatti e produce contenuti originali con il tono di voce del brand.",
    features: [
      "Articoli blog SEO-optimized con meta description",
      "Fact-checking automatico dalle fonti configurate",
      "Brand voice personalizzata per ogni progetto",
      "Formattazione Markdown con titoli, liste, link",
      "Lunghezza e stile configurabili",
    ],
  },
  {
    name: "Social Manager",
    icon: Share2,
    color: "text-violet-600 bg-violet-100 dark:bg-violet-950",
    desc: "Crea post pronti per la pubblicazione su Instagram, LinkedIn, X e Facebook. Genera automaticamente immagini AI, hashtag intelligenti e adatta il formato a ogni piattaforma.",
    features: [
      "Post per Instagram, LinkedIn, X, Facebook",
      "Hashtag intelligenti e rilevanti",
      "Immagini AI generate automaticamente (DALL-E, Higgsfield)",
      "Pubblicazione diretta tramite connettori social",
      "Adattamento formato per ogni piattaforma",
    ],
  },
  {
    name: "Newsletter",
    icon: Mail,
    color: "text-emerald-600 bg-emerald-100 dark:bg-emerald-950",
    desc: "Scansiona le fonti di contenuto, seleziona le notizie piu rilevanti e compone una newsletter professionale. Invio automatico tramite il server SMTP configurato.",
    features: [
      "Scansione automatica delle fonti di contenuto",
      "Content curation intelligente",
      "Template email professionale e responsive",
      "Invio tramite SMTP configurabile (es. Aruba, Gmail)",
      "Brand voice e tono personalizzato",
    ],
  },
  {
    name: "Analytics",
    icon: BarChart3,
    color: "text-amber-600 bg-amber-100 dark:bg-amber-950",
    desc: "Analizza le performance dei contenuti pubblicati e genera report con insight e suggerimenti per migliorare la strategia.",
    features: [
      "Report performance automatici",
      "Insight AI con suggerimenti operativi",
      "Metriche per canale e piattaforma",
      "Trend e confronti temporali",
      "Suggerimenti per ottimizzare la strategia",
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
            <Logo variant="mark" size="sm" />
            Guida completa
          </Badge>
          <h1 className="text-3xl sm:text-4xl lg:text-5xl font-bold tracking-tight">
            Come funziona wepostai.com
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
              <div className={`flex flex-col ${step.screenshot ? (i % 2 === 0 ? "lg:flex-row" : "lg:flex-row-reverse") : ""} gap-8 items-start`}>
                <div className={step.screenshot ? "lg:w-2/5" : "w-full max-w-3xl mx-auto"}>
                  <div className="space-y-4">
                    <div className="flex items-center gap-3">
                      <span className="size-8 rounded-full bg-primary text-primary-foreground flex items-center justify-center text-sm font-bold shrink-0">
                        {i + 1}
                      </span>
                      <h2 className="text-2xl font-bold">{step.title}</h2>
                    </div>
                    <p className="text-muted-foreground leading-relaxed">{step.desc}</p>
                    {step.details && (
                      <ul className="space-y-2 mt-4">
                        {step.details.map((d) => (
                          <li key={d} className="flex items-start gap-2 text-sm">
                            <CheckCircle2 className="size-4 text-primary shrink-0 mt-0.5" />
                            <span className="text-muted-foreground">{d}</span>
                          </li>
                        ))}
                      </ul>
                    )}
                  </div>

                  {/* Scheduler detail section */}
                  {step.id === "schedule" && (
                    <div className="mt-6 space-y-4">
                      <h3 className="text-lg font-semibold">Opzioni dello Scheduler</h3>
                      <div className="grid gap-3">
                        <div className="flex items-start gap-3 p-3 rounded-lg border bg-muted/50">
                          <Target className="size-5 text-primary shrink-0 mt-0.5" />
                          <div>
                            <p className="font-medium text-sm">Agente AI</p>
                            <p className="text-xs text-muted-foreground">Scegli quale agente eseguire: Content Writer, Social Manager, Newsletter o Analytics</p>
                          </div>
                        </div>
                        <div className="flex items-start gap-3 p-3 rounded-lg border bg-muted/50">
                          <Repeat className="size-5 text-primary shrink-0 mt-0.5" />
                          <div>
                            <p className="font-medium text-sm">Frequenza</p>
                            <p className="text-xs text-muted-foreground">Giornaliero, settimanale (scegli i giorni), mensile o con espressione CRON personalizzata</p>
                          </div>
                        </div>
                        <div className="flex items-start gap-3 p-3 rounded-lg border bg-muted/50">
                          <Clock className="size-5 text-primary shrink-0 mt-0.5" />
                          <div>
                            <p className="font-medium text-sm">Orario di esecuzione</p>
                            <p className="text-xs text-muted-foreground">Imposta l&apos;ora e il fuso orario. Es: ogni giorno alle 09:00 CET</p>
                          </div>
                        </div>
                        <div className="flex items-start gap-3 p-3 rounded-lg border bg-muted/50">
                          <Play className="size-5 text-primary shrink-0 mt-0.5" />
                          <div>
                            <p className="font-medium text-sm">Stato attivo/disattivo</p>
                            <p className="text-xs text-muted-foreground">Attiva o metti in pausa ogni scheduler indipendentemente. Lo storico delle esecuzioni resta visibile</p>
                          </div>
                        </div>
                        <div className="flex items-start gap-3 p-3 rounded-lg border bg-muted/50">
                          <FileText className="size-5 text-primary shrink-0 mt-0.5" />
                          <div>
                            <p className="font-medium text-sm">Prompt personalizzato</p>
                            <p className="text-xs text-muted-foreground">Aggiungi istruzioni specifiche per la generazione: tono, argomenti, lunghezza, lingua</p>
                          </div>
                        </div>
                        <div className="flex items-start gap-3 p-3 rounded-lg border bg-muted/50">
                          <Globe className="size-5 text-primary shrink-0 mt-0.5" />
                          <div>
                            <p className="font-medium text-sm">Pubblicazione automatica</p>
                            <p className="text-xs text-muted-foreground">Se l&apos;approvazione automatica e attiva, il contenuto viene pubblicato subito sui canali configurati</p>
                          </div>
                        </div>
                      </div>
                      <div className="p-4 rounded-lg border border-primary/20 bg-primary/5">
                        <p className="text-sm font-medium text-primary mb-1">Esempio di configurazione</p>
                        <p className="text-xs text-muted-foreground">
                          Scheduler &quot;Post Social giornaliero&quot; → Agente: Social Manager → Frequenza: Ogni giorno → Ore: 09:00 CET → Approvazione: Automatica → Il post viene generato e pubblicato su Instagram e LinkedIn ogni mattina alle 9.
                        </p>
                      </div>
                    </div>
                  )}
                </div>

                {step.screenshot && (
                  <div className="lg:w-3/5">
                    <GuideScreenshots src={step.screenshot} alt={step.title} />
                  </div>
                )}
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
                  <div className="flex items-center gap-3 mb-3">
                    <div className={`size-10 rounded-lg flex items-center justify-center ${agent.color}`}>
                      <agent.icon className="size-5" />
                    </div>
                    <h3 className="text-lg font-semibold">{agent.name}</h3>
                  </div>
                  <p className="text-sm text-muted-foreground mb-4">{agent.desc}</p>
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
      <section className="py-20 px-4 border-t bg-muted/30">
        <div className="max-w-4xl mx-auto text-center">
          <div className="flex justify-center gap-3 mb-6">
            <Smartphone className="size-6 text-primary" />
            <MessageSquare className="size-6 text-primary" />
          </div>
          <h2 className="text-3xl font-bold tracking-tight">Disponibile ovunque</h2>
          <p className="text-lg text-muted-foreground mt-4 max-w-2xl mx-auto">
            Gestisci wepostai.com dalla web app, dall&apos;app mobile per iOS e Android, o direttamente dal tuo bot Telegram.
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
      <section className="py-20 px-4 border-t">
        <div className="max-w-3xl mx-auto text-center">
          <h2 className="text-3xl font-bold tracking-tight">
            Pronto a iniziare?
          </h2>
          <p className="text-lg text-muted-foreground mt-4">
            Prova wepostai.com gratis per 14 giorni. Nessuna carta di credito richiesta.
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
