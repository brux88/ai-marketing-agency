import Link from "next/link";
import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { PenLine, Share2, Mail, BarChart3, Check, ArrowRight, Sparkles, Zap, Shield } from "lucide-react";

const features = [
  {
    title: "Content Writer",
    desc: "Genera articoli blog SEO-optimized da fonti reali, con fact-checking automatico e brand voice personalizzata.",
    icon: PenLine,
    color: "text-blue-600 bg-blue-100 dark:bg-blue-950",
  },
  {
    title: "Social Manager",
    desc: "Crea post per Instagram, LinkedIn, X e Facebook con calendario editoriale intelligente.",
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

const plans = [
  { name: "Basic", price: "29", agencies: "3", jobs: "100" },
  { name: "Pro", price: "79", agencies: "10", jobs: "500", popular: true },
  { name: "Enterprise", price: "199", agencies: "Illimitate", jobs: "2000" },
];

export default function LandingPage() {
  return (
    <div>
      {/* Hero */}
      <section className="relative overflow-hidden py-24 px-4">
        <div className="absolute inset-0 bg-gradient-to-b from-primary/5 via-background to-background" />
        <div className="relative max-w-4xl mx-auto text-center">
          <Badge variant="secondary" className="mb-6">
            <Sparkles className="size-3 mr-1" />
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

      {/* Pricing */}
      <section className="py-24 px-4">
        <div className="max-w-6xl mx-auto">
          <div className="text-center mb-16">
            <h2 className="text-3xl sm:text-4xl font-bold tracking-tight">Piani e prezzi</h2>
            <p className="text-muted-foreground mt-3">Scegli il piano giusto per il tuo business</p>
          </div>
          <div className="grid md:grid-cols-3 gap-6 max-w-4xl mx-auto">
            {plans.map((plan) => (
              <Card
                key={plan.name}
                className={`relative flex flex-col ${plan.popular ? "border-primary shadow-lg scale-105" : ""}`}
              >
                {plan.popular && (
                  <div className="absolute -top-3 left-1/2 -translate-x-1/2">
                    <Badge>Piu popolare</Badge>
                  </div>
                )}
                <CardHeader>
                  <CardTitle>{plan.name}</CardTitle>
                  <div className="mt-2">
                    <span className="text-4xl font-bold">&euro;{plan.price}</span>
                    <span className="text-muted-foreground">/mese</span>
                  </div>
                </CardHeader>
                <CardContent className="flex-1">
                  <ul className="space-y-3 text-sm">
                    <li className="flex items-center gap-2">
                      <Check className="size-4 text-primary shrink-0" />
                      Fino a {plan.agencies} agenzie
                    </li>
                    <li className="flex items-center gap-2">
                      <Check className="size-4 text-primary shrink-0" />
                      {plan.jobs} job AI / mese
                    </li>
                    <li className="flex items-center gap-2">
                      <Check className="size-4 text-primary shrink-0" />
                      Tutti e 4 gli agenti AI
                    </li>
                    <li className="flex items-center gap-2">
                      <Check className="size-4 text-primary shrink-0" />
                      Supporto via email
                    </li>
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
    </div>
  );
}
