"use client";

import { useAuth } from "@/lib/providers/auth-provider";
import { useQuery } from "@tanstack/react-query";
import { agenciesApi } from "@/lib/api/agencies.api";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect } from "react";
import type { Agency } from "@/types/api";
import { Button, buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { PlusCircle, Rocket, FileText, Rss, CheckCircle2, Clock } from "lucide-react";

export default function DashboardPage() {
  const { user } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (user?.role === "SuperAdmin") {
      router.replace("/admin");
    }
  }, [user, router]);

  if (user?.role === "SuperAdmin") return null;
  const { data, isLoading } = useQuery({
    queryKey: ["agencies"],
    queryFn: agenciesApi.list,
  });

  const agencies: Agency[] = data?.data || [];

  return (
    <div>
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Dashboard</h1>
          <p className="text-muted-foreground mt-1">Ciao {user?.fullName}, ecco le tue agenzie</p>
        </div>
        <Link href="/agencies/new" className={cn(buttonVariants(), "gap-1.5")}>
          <PlusCircle className="size-4" />
          Nuova Agenzia
        </Link>
      </div>

      {isLoading ? (
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
          {[1, 2, 3].map((i) => (
            <Card key={i}>
              <CardHeader>
                <Skeleton className="h-5 w-3/4" />
                <Skeleton className="h-4 w-1/2 mt-2" />
              </CardHeader>
              <CardContent>
                <Skeleton className="h-4 w-full" />
                <Skeleton className="h-4 w-2/3 mt-2" />
              </CardContent>
            </Card>
          ))}
        </div>
      ) : agencies.length === 0 ? (
        <Card className="text-center py-16">
          <CardContent className="space-y-4">
            <div className="size-16 rounded-full bg-primary/10 flex items-center justify-center mx-auto">
              <Rocket className="size-8 text-primary" />
            </div>
            <h3 className="text-lg font-semibold">Nessuna agenzia</h3>
            <p className="text-muted-foreground max-w-sm mx-auto">
              Crea la tua prima agenzia di marketing AI e inizia a generare contenuti
            </p>
            <Link href="/agencies/new" className={cn(buttonVariants(), "gap-1.5")}>
              <PlusCircle className="size-4" />
              Crea Agenzia
            </Link>
          </CardContent>
        </Card>
      ) : (
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
          {agencies.map((agency) => (
            <Link key={agency.id} href={`/agencies/${agency.id}`}>
              <Card className="hover:shadow-md transition-all duration-200 hover:-translate-y-0.5 h-full">
                <CardHeader className="pb-3">
                  <CardTitle className="text-lg">{agency.name}</CardTitle>
                  <CardDescription>{agency.productName}</CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="flex gap-4 text-sm text-muted-foreground">
                    <span className="flex items-center gap-1">
                      <Rss className="size-3.5" />
                      {agency.contentSourcesCount} fonti
                    </span>
                    <span className="flex items-center gap-1">
                      <FileText className="size-3.5" />
                      {agency.generatedContentsCount} contenuti
                    </span>
                  </div>
                  <Badge variant={agency.approvalMode === 1 ? "outline" : "secondary"}>
                    {agency.approvalMode === 1 ? (
                      <><Clock className="size-3" /> Manuale</>
                    ) : (
                      <><CheckCircle2 className="size-3" /> Auto-approvazione</>
                    )}
                  </Badge>
                </CardContent>
              </Card>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
