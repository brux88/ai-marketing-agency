import { notFound } from "next/navigation";
import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { Badge } from "@/components/ui/badge";

const blogPosts: Record<string, { title: string; category: string; date: string; content: string }> = {
  "ai-content-marketing": {
    title: "Come l'AI sta rivoluzionando il content marketing",
    category: "wepostai.com",
    date: "15 Apr 2026",
    content: `L'intelligenza artificiale sta trasformando radicalmente il modo in cui le aziende creano e distribuiscono contenuti di marketing. Gli agenti AI di wepostai.com rappresentano l'evoluzione naturale di questo trend, offrendo la possibilita di generare contenuti originali e di qualita in modo completamente automatizzato.

## Il problema del content marketing tradizionale

Creare contenuti di qualita richiede tempo, competenze e risorse. Un team di marketing tipico dedica ore alla ricerca, scrittura, revisione e pubblicazione di ogni singolo post. Con la crescente domanda di contenuti su piu piattaforme social, questo approccio diventa insostenibile.

## Come gli agenti AI risolvono il problema

Gli agenti AI di wepostai.com analizzano il tono di voce del tuo brand, le tendenze del settore e le fonti di contenuto che configuri. Da queste informazioni, generano post originali che mantengono coerenza con la tua identita aziendale.

### Vantaggi chiave:

- **Coerenza del brand**: L'AI apprende e replica il tuo tono di voce
- **Velocita**: Contenuti generati in secondi, non in ore
- **Multicanalita**: Un singolo contenuto adattato per ogni piattaforma
- **Ottimizzazione continua**: L'AI migliora nel tempo basandosi sui risultati

## Il flusso di lavoro con wepostai.com

1. **Configura le fonti**: Aggiungi feed RSS, siti web e argomenti di interesse
2. **Definisci il brand**: Imposta tono di voce, lingua e stile comunicativo
3. **Programma la pubblicazione**: Scegli frequenza e orari ottimali
4. **Approva o automatizza**: Decidi se revisionare ogni post o pubblicare automaticamente

## Risultati concreti

Le aziende che utilizzano wepostai.com registrano un aumento medio del 300% nella frequenza di pubblicazione, mantenendo o migliorando la qualita percepita dei contenuti. Il tempo risparmiato puo essere reinvestito in strategie di alto livello.`,
  },
  "social-media-automatizzato": {
    title: "Guida completa al Social Media Marketing automatizzato",
    category: "Social Media",
    date: "12 Apr 2026",
    content: `L'automazione del social media marketing non significa perdere l'autenticita. Con gli strumenti giusti, puoi mantenere una presenza costante e professionale su tutti i canali, dedicando il tuo tempo alle attivita che richiedono davvero il tocco umano.

## Dalla pianificazione alla pubblicazione

Il processo di social media marketing comprende diverse fasi, ognuna delle quali puo beneficiare dell'automazione:

### 1. Ricerca e ispirazione
wepostai.com monitora automaticamente le fonti che configuri — feed RSS, blog di settore, siti di news — e identifica gli argomenti piu rilevanti per il tuo pubblico.

### 2. Creazione dei contenuti
L'agente AI genera post adattati per ogni piattaforma: testi brevi per Twitter/X, post piu articolati per LinkedIn, contenuti visivi per Instagram e Facebook.

### 3. Pianificazione intelligente
Lo scheduler di wepostai.com permette di configurare:
- **Frequenza di pubblicazione**: Giornaliera, settimanale o personalizzata
- **Fasce orarie**: Pubblica quando il tuo pubblico e piu attivo
- **Distribuzione multi-piattaforma**: Un contenuto, piu canali

### 4. Approvazione e controllo
Puoi scegliere tra due modalita:
- **Con approvazione**: Ogni contenuto generato richiede il tuo OK prima della pubblicazione
- **Automatica**: I contenuti vengono pubblicati direttamente secondo la programmazione

## Connettori supportati

wepostai.com supporta la pubblicazione diretta su:
- Facebook (pagine aziendali)
- Instagram (post e storie)
- LinkedIn (profili e pagine aziendali)
- Twitter/X
- Telegram (canali e gruppi)

## Best practices per l'automazione

- **Non automatizzare tutto**: Le risposte ai commenti e le interazioni dovrebbero restare umane
- **Monitora i risultati**: Usa le analytics per capire cosa funziona
- **Aggiorna le fonti**: Mantieni fresche le fonti di ispirazione dell'AI
- **Rivedi periodicamente**: Controlla che il tono resti allineato al brand`,
  },
  "newsletter-ai-powered": {
    title: "Newsletter che convertono: strategie AI-powered",
    category: "Newsletter",
    date: "10 Apr 2026",
    content: `Le newsletter restano uno degli strumenti di marketing piu efficaci, con un ROI medio di 36:1. Ma creare newsletter che i tuoi iscritti vogliono davvero leggere richiede costanza e creativita. Ecco come l'AI puo aiutarti.

## Perche le newsletter falliscono

La maggior parte delle newsletter viene ignorata o cancellata perche:
- Contenuti ripetitivi e poco interessanti
- Frequenza di invio irregolare
- Mancanza di personalizzazione
- Design poco curato

## Come l'AI trasforma le newsletter

### Contenuti sempre freschi
wepostai.com genera contenuti per le tue newsletter basandosi sulle fonti che configuri. Ogni edizione e unica e rilevante per il tuo settore.

### Frequenza costante
Con lo scheduler automatico, le tue newsletter partono sempre puntuali. Configura la frequenza — settimanale, bisettimanale o mensile — e wepostai.com si occupa del resto.

### Personalizzazione del messaggio
L'AI adatta il tono e lo stile in base al tuo brand. Che tu preferisca un approccio formale o colloquiale, il risultato sara sempre coerente.

## Configurare la newsletter in wepostai.com

1. **Configura il connettore email**: Collega il tuo server SMTP o account SendGrid
2. **Gestisci gli iscritti**: Importa o aggiungi manualmente i contatti
3. **Genera il contenuto**: Usa l'agente AI per creare il testo della newsletter
4. **Invia**: Con un click, la newsletter raggiunge tutti i tuoi iscritti

## Metriche che contano

Monitora queste metriche per ottimizzare le tue newsletter:
- **Tasso di apertura**: Obiettivo > 20%
- **Tasso di click**: Obiettivo > 3%
- **Tasso di disiscrizione**: Mantieni < 0.5%
- **Conversioni**: Traccia le azioni generate dalla newsletter

## Suggerimenti per newsletter efficaci

- **Oggetto accattivante**: L'AI puo generare varianti dell'oggetto da testare
- **Contenuto di valore**: Offri informazioni utili, non solo promozioni
- **Call-to-action chiara**: Ogni newsletter deve avere un obiettivo specifico
- **Design responsive**: Assicurati che la newsletter sia leggibile su mobile`,
  },
};

export function generateStaticParams() {
  return Object.keys(blogPosts).map((slug) => ({ slug }));
}

export default async function BlogPostPage({ params }: { params: Promise<{ slug: string }> }) {
  const { slug } = await params;
  const post = blogPosts[slug];

  if (!post) notFound();

  const paragraphs = post.content.split("\n\n");

  return (
    <div className="min-h-screen bg-background">
      <div className="max-w-3xl mx-auto px-4 py-16">
        <Link
          href="/#blog"
          className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground mb-8"
        >
          <ArrowLeft className="size-4" />
          Torna al blog
        </Link>

        <article>
          <div className="flex items-center gap-3 mb-4">
            <Badge variant="secondary">{post.category}</Badge>
            <span className="text-sm text-muted-foreground">{post.date}</span>
          </div>

          <h1 className="text-3xl sm:text-4xl font-bold tracking-tight mb-8 leading-tight">
            {post.title}
          </h1>

          <div className="prose prose-neutral max-w-none">
            {paragraphs.map((p, i) => {
              if (p.startsWith("## ")) {
                return (
                  <h2 key={i} className="text-2xl font-bold mt-10 mb-4">
                    {p.replace("## ", "")}
                  </h2>
                );
              }
              if (p.startsWith("### ")) {
                return (
                  <h3 key={i} className="text-xl font-semibold mt-8 mb-3">
                    {p.replace("### ", "")}
                  </h3>
                );
              }
              if (p.includes("\n- ")) {
                const lines = p.split("\n");
                const title = lines[0];
                const items = lines.filter((l) => l.startsWith("- "));
                return (
                  <div key={i} className="my-4">
                    {title && !title.startsWith("- ") && (
                      <p className="text-muted-foreground leading-relaxed mb-2">{title}</p>
                    )}
                    <ul className="list-disc pl-6 space-y-2">
                      {items.map((item, j) => {
                        const text = item.replace("- ", "");
                        const boldMatch = text.match(/^\*\*(.+?)\*\*:?\s*(.*)/);
                        return (
                          <li key={j} className="text-muted-foreground leading-relaxed">
                            {boldMatch ? (
                              <>
                                <strong className="text-foreground">{boldMatch[1]}</strong>
                                {boldMatch[2] && `: ${boldMatch[2]}`}
                              </>
                            ) : (
                              text
                            )}
                          </li>
                        );
                      })}
                    </ul>
                  </div>
                );
              }
              if (p.match(/^\d+\./)) {
                const items = p.split("\n").filter((l) => l.match(/^\d+\./));
                return (
                  <ol key={i} className="list-decimal pl-6 space-y-2 my-4">
                    {items.map((item, j) => {
                      const text = item.replace(/^\d+\.\s*/, "");
                      const boldMatch = text.match(/^\*\*(.+?)\*\*:?\s*(.*)/);
                      return (
                        <li key={j} className="text-muted-foreground leading-relaxed">
                          {boldMatch ? (
                            <>
                              <strong className="text-foreground">{boldMatch[1]}</strong>
                              {boldMatch[2] && `: ${boldMatch[2]}`}
                            </>
                          ) : (
                            text
                          )}
                        </li>
                      );
                    })}
                  </ol>
                );
              }
              return (
                <p key={i} className="text-muted-foreground leading-relaxed my-4">
                  {p}
                </p>
              );
            })}
          </div>
        </article>

        <div className="border-t mt-16 pt-8">
          <Link
            href="/#blog"
            className="inline-flex items-center gap-1 text-sm text-primary hover:underline"
          >
            <ArrowLeft className="size-4" />
            Torna al blog
          </Link>
        </div>
      </div>
    </div>
  );
}
