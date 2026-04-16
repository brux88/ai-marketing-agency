"use client";

import { useQuery } from "@tanstack/react-query";
import { billingApi, type BillingUsage } from "@/lib/api/billing.api";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { Skeleton } from "@/components/ui/skeleton";
import { CreditCard, Check, Crown, Zap, Building2, Rocket, Loader2 } from "lucide-react";

const plans = [
  {
    name: "Free Trial",
    tier: "FreeTrial",
    price: "0",
    period: "14 giorni",
    agencies: "1",
    jobs: "50",
    features: ["1 agenzia", "50 job AI/mese", "Generazione contenuti", "Calendario editoriale"],
    icon: Rocket,
  },
  {
    name: "Basic",
    tier: "Basic",
    price: "29",
    period: "mese",
    agencies: "3",
    jobs: "100",
    features: ["3 agenzie", "100 job AI/mese", "Pubblicazione social", "Newsletter", "Supporto email"],
    icon: Zap,
  },
  {
    name: "Pro",
    tier: "Pro",
    price: "79",
    period: "mese",
    agencies: "10",
    jobs: "500",
    features: ["10 agenzie", "500 job AI/mese", "Tutti i canali social", "Analitiche avanzate", "Supporto prioritario"],
    icon: Crown,
    popular: true,
  },
  {
    name: "Enterprise",
    tier: "Enterprise",
    price: "199",
    period: "mese",
    agencies: "Illimitate",
    jobs: "2000",
    features: ["Agenzie illimitate", "2000 job AI/mese", "API personalizzate", "Account manager dedicato", "SLA garantito"],
    icon: Building2,
  },
];

export default function BillingPage() {
  const { data: usage, isLoading } = useQuery({
    queryKey: ["billing", "usage"],
    queryFn: () => billingApi.getUsage(),
    refetchInterval: 30000,
  });

  const u = usage;
  const agenciesPct = u ? Math.round((u.agenciesUsed / u.maxAgencies) * 100) : 0;
  const jobsPct = u ? Math.round((u.jobsUsed / u.maxJobs) * 100) : 0;

  const handlePortal = async () => {
    try {
      const result = await billingApi.createPortalSession(window.location.href);
      if (result?.url) window.location.href = result.url;
    } catch { /* noop */ }
  };

  const handleCheckout = async (priceId: string) => {
    try {
      const result = await billingApi.createCheckoutSession(
        priceId,
        `${window.location.origin}/settings/billing?success=true`,
        `${window.location.origin}/settings/billing?cancelled=true`
      );
      if (result?.url) window.location.href = result.url;
    } catch { /* noop */ }
  };

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Abbonamento</h1>
        <p className="text-muted-foreground mt-1">Gestisci il tuo piano e la fatturazione</p>
      </div>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle className="flex items-center gap-2">
                <CreditCard className="size-5" />
                Piano attuale: {isLoading ? <Skeleton className="h-5 w-24 inline-block" /> : (u?.plan ?? "Free Trial")}
              </CardTitle>
              <CardDescription className="mt-1">
                {u?.status === "FreeTrial" || !u?.status
                  ? "Prova gratuita"
                  : u.status === "Active"
                    ? "Abbonamento attivo"
                    : u.status}
                {u?.currentPeriodEnd && ` · Rinnovo: ${new Date(u.currentPeriodEnd).toLocaleDateString("it-IT")}`}
                {u?.trialEndsAt && ` · Scadenza trial: ${new Date(u.trialEndsAt).toLocaleDateString("it-IT")}`}
              </CardDescription>
            </div>
            <Button onClick={handlePortal}>Gestisci fatturazione</Button>
          </div>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="space-y-3">
              <Skeleton className="h-8 w-full" />
              <Skeleton className="h-8 w-full" />
            </div>
          ) : (
            <div className="space-y-3">
              <div>
                <div className="flex justify-between text-sm mb-1.5">
                  <span className="text-muted-foreground">Agenzie utilizzate</span>
                  <span className="font-medium">{u?.agenciesUsed ?? 0} / {u?.maxAgencies ?? 1}</span>
                </div>
                <Progress value={agenciesPct} />
              </div>
              <div>
                <div className="flex justify-between text-sm mb-1.5">
                  <span className="text-muted-foreground">Job AI questo mese</span>
                  <span className="font-medium">{u?.jobsUsed ?? 0} / {u?.maxJobs ?? 50}</span>
                </div>
                <Progress value={jobsPct} />
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      <div>
        <h2 className="text-lg font-semibold tracking-tight mb-4">Piani disponibili</h2>
        <div className="grid sm:grid-cols-2 xl:grid-cols-4 gap-6">
          {plans.map((plan) => {
            const isCurrent = u?.plan === plan.tier;
            return (
              <Card
                key={plan.name}
                className={`relative flex flex-col ${plan.popular ? "border-primary shadow-lg ring-1 ring-primary/20" : ""} ${isCurrent ? "bg-muted/30" : ""}`}
              >
                {plan.popular && (
                  <div className="absolute -top-3 left-1/2 -translate-x-1/2 z-10">
                    <Badge className="px-3 py-1 text-xs whitespace-nowrap shadow-sm">
                      Consigliato
                    </Badge>
                  </div>
                )}
                <CardHeader className="pb-4 pt-6">
                  <div className="flex items-center gap-2">
                    <plan.icon className="size-5 text-primary" />
                    <CardTitle className="text-lg">{plan.name}</CardTitle>
                  </div>
                  <div className="mt-3">
                    <span className="text-4xl font-bold">&euro;{plan.price}</span>
                    <span className="text-muted-foreground text-sm">/{plan.period}</span>
                  </div>
                </CardHeader>
                <CardContent className="flex-1 pb-4">
                  <ul className="space-y-2.5 text-sm">
                    {plan.features.map((f) => (
                      <li key={f} className="flex items-center gap-2">
                        <Check className="size-4 text-primary shrink-0" />
                        {f}
                      </li>
                    ))}
                  </ul>
                </CardContent>
                <CardFooter className="pt-0">
                  {isCurrent ? (
                    <Button className="w-full" variant="outline" disabled>
                      Piano attuale
                    </Button>
                  ) : (
                    <Button
                      className="w-full"
                      variant={plan.popular ? "default" : "outline"}
                      onClick={() => handleCheckout(plan.tier)}
                    >
                      {plan.price === "0" ? "Inizia gratis" : "Scegli piano"}
                    </Button>
                  )}
                </CardFooter>
              </Card>
            );
          })}
        </div>
      </div>
    </div>
  );
}
