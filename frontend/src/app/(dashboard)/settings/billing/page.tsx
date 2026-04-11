"use client";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { CreditCard, Check, Crown, Zap, Building2, Rocket } from "lucide-react";

const plans = [
  { name: "Free Trial", price: "0", period: "14 giorni", agencies: "1", jobs: "20", icon: Rocket },
  { name: "Basic", price: "29", period: "mese", agencies: "3", jobs: "100", icon: Zap },
  { name: "Pro", price: "79", period: "mese", agencies: "10", jobs: "500", icon: Crown, popular: true },
  { name: "Enterprise", price: "199", period: "mese", agencies: "Illimitate", jobs: "2000", icon: Building2 },
];

export default function BillingPage() {
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
                Piano attuale: Free Trial
              </CardTitle>
              <CardDescription className="mt-1">14 giorni di prova gratuita - 1 agenzia, 20 job/mese</CardDescription>
            </div>
            <Button>Gestisci fatturazione</Button>
          </div>
        </CardHeader>
        <CardContent>
          <div className="space-y-3">
            <div>
              <div className="flex justify-between text-sm mb-1.5">
                <span className="text-muted-foreground">Agenzie utilizzate</span>
                <span className="font-medium">0 / 1</span>
              </div>
              <Progress value={0} />
            </div>
            <div>
              <div className="flex justify-between text-sm mb-1.5">
                <span className="text-muted-foreground">Job AI questo mese</span>
                <span className="font-medium">0 / 20</span>
              </div>
              <Progress value={0} />
            </div>
          </div>
        </CardContent>
      </Card>

      <div>
        <h2 className="text-lg font-semibold tracking-tight mb-4">Piani disponibili</h2>
        <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-4">
          {plans.map((plan) => (
            <Card key={plan.name} className={`relative flex flex-col ${plan.popular ? "border-primary shadow-md" : ""}`}>
              {plan.popular && (
                <div className="absolute -top-2.5 left-1/2 -translate-x-1/2">
                  <Badge>Consigliato</Badge>
                </div>
              )}
              <CardHeader className="pb-3">
                <div className="flex items-center gap-2">
                  <plan.icon className="size-4 text-primary" />
                  <CardTitle className="text-base">{plan.name}</CardTitle>
                </div>
                <div className="mt-2">
                  <span className="text-3xl font-bold">&euro;{plan.price}</span>
                  <span className="text-muted-foreground text-sm">/{plan.period}</span>
                </div>
              </CardHeader>
              <CardContent className="flex-1">
                <ul className="space-y-2 text-sm">
                  <li className="flex items-center gap-2">
                    <Check className="size-3.5 text-primary shrink-0" />
                    {plan.agencies} agenzie
                  </li>
                  <li className="flex items-center gap-2">
                    <Check className="size-3.5 text-primary shrink-0" />
                    {plan.jobs} job/mese
                  </li>
                </ul>
              </CardContent>
              <CardFooter>
                <Button className="w-full" variant={plan.popular ? "default" : "outline"} size="sm">
                  Scegli piano
                </Button>
              </CardFooter>
            </Card>
          ))}
        </div>
      </div>
    </div>
  );
}
