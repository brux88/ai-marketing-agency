"use client";

import Link from "next/link";
import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { jobsApi, type JobListItem } from "@/lib/api/jobs.api";
import { notificationsApi, type NotificationItem } from "@/lib/api/notifications.api";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { CheckCheck, Loader2, RefreshCw } from "lucide-react";

function statusVariant(status: string): "default" | "secondary" | "destructive" | "outline" {
  switch (status) {
    case "Completed": return "default";
    case "Failed": return "destructive";
    case "Running": return "secondary";
    default: return "outline";
  }
}

export default function JobsPage() {
  const qc = useQueryClient();
  const [expanded, setExpanded] = useState<Record<string, boolean>>({});

  const { data: jobs, isLoading: jobsLoading } = useQuery({
    queryKey: ["jobs", "all"],
    queryFn: async () => (await jobsApi.list()).data,
    refetchInterval: 4000,
  });

  const { data: notifData } = useQuery({
    queryKey: ["notifications", "list"],
    queryFn: async () => (await notificationsApi.list(false, 50)).data,
    refetchInterval: 4000,
  });

  const markAllRead = useMutation({
    mutationFn: () => notificationsApi.markAllRead(),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["notifications"] });
    },
  });

  const markOneRead = useMutation({
    mutationFn: (id: string) => notificationsApi.markRead(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["notifications"] });
    },
  });

  return (
    <div className="space-y-8" data-testid="jobs-page">
      <div>
        <h1 className="text-3xl font-bold">Centro lavori e notifiche</h1>
        <p className="text-muted-foreground">Monitora i job in background e le notifiche recenti.</p>
      </div>

      <Card>
        <CardHeader className="flex-row items-center justify-between space-y-0">
          <CardTitle>Notifiche ({notifData?.unreadCount ?? 0} non lette)</CardTitle>
          <div className="flex items-center gap-2">
            <Button
              size="sm"
              variant="outline"
              onClick={() => {
                qc.invalidateQueries({ queryKey: ["jobs"] });
                qc.invalidateQueries({ queryKey: ["notifications"] });
              }}
              data-testid="refresh-all"
            >
              <RefreshCw className="size-4" /> Aggiorna
            </Button>
            <Button
              size="sm"
              variant="outline"
              onClick={() => markAllRead.mutate()}
              disabled={!notifData?.unreadCount}
              data-testid="mark-all-read"
            >
              <CheckCheck className="size-4" /> Segna tutte come lette
            </Button>
          </div>
        </CardHeader>
        <CardContent className="space-y-2">
          {!notifData?.items?.length && (
            <p className="text-sm text-muted-foreground">Nessuna notifica.</p>
          )}
          {notifData?.items?.map((n: NotificationItem) => (
            <div
              key={n.id}
              className={`flex items-start gap-3 p-3 rounded-md border ${!n.read ? "bg-muted/50" : ""}`}
              data-testid={`notification-${n.id}`}
            >
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2">
                  <span className="font-medium text-sm">{n.title}</span>
                  {!n.read && <Badge variant="secondary" className="text-[10px]">Nuova</Badge>}
                </div>
                {n.body && <p className="text-xs text-muted-foreground mt-1 line-clamp-2">{n.body}</p>}
                <p className="text-[10px] text-muted-foreground mt-1">
                  {new Date(n.createdAt).toLocaleString("it-IT")}
                </p>
              </div>
              {(() => {
                const safeLink = n.link && n.link.startsWith("/jobs") ? n.link : "/jobs";
                return (
                  <Link href={safeLink} className="text-xs text-primary hover:underline shrink-0">
                    Apri
                  </Link>
                );
              })()}
              {!n.read && (
                <Button
                  size="sm"
                  variant="ghost"
                  onClick={() => markOneRead.mutate(n.id)}
                  className="shrink-0"
                >
                  OK
                </Button>
              )}
            </div>
          ))}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Job recenti</CardTitle>
        </CardHeader>
        <CardContent className="space-y-2">
          {jobsLoading && <Loader2 className="size-5 animate-spin" />}
          {!jobsLoading && !jobs?.length && (
            <p className="text-sm text-muted-foreground">Nessun job.</p>
          )}
          {jobs?.map((j: JobListItem) => {
            const isOpen = !!expanded[j.id];
            return (
              <div
                key={j.id}
                className="p-3 rounded-md border"
                data-testid={`job-${j.id}`}
              >
                <div className="flex items-start gap-3">
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2">
                      <span className="font-medium text-sm">{j.agentType}</span>
                      <Badge variant={statusVariant(j.status)}>{j.status}</Badge>
                    </div>
                    {j.input && !isOpen && <p className="text-xs text-muted-foreground mt-1 line-clamp-1">{j.input}</p>}
                    {j.errorMessage && !isOpen && <p className="text-xs text-destructive mt-1 line-clamp-1">{j.errorMessage}</p>}
                    <p className="text-[10px] text-muted-foreground mt-1">
                      Creato: {new Date(j.createdAt).toLocaleString("it-IT")}
                      {j.completedAt && ` · Completato: ${new Date(j.completedAt).toLocaleString("it-IT")}`}
                    </p>
                  </div>
                  <button
                    type="button"
                    onClick={() => setExpanded((s) => ({ ...s, [j.id]: !s[j.id] }))}
                    className="text-xs text-primary hover:underline shrink-0"
                  >
                    {isOpen ? "Nascondi" : "Dettagli"}
                  </button>
                </div>
                {isOpen && (
                  <div className="mt-3 space-y-2 border-t pt-3">
                    {j.input && (
                      <div>
                        <p className="text-[10px] font-semibold uppercase text-muted-foreground">Input</p>
                        <pre className="text-xs whitespace-pre-wrap break-words bg-muted/50 rounded p-2 mt-1">{j.input}</pre>
                      </div>
                    )}
                    {j.output && (
                      <div>
                        <p className="text-[10px] font-semibold uppercase text-muted-foreground">Output</p>
                        <pre className="text-xs whitespace-pre-wrap break-words bg-muted/50 rounded p-2 mt-1 max-h-64 overflow-auto">{j.output}</pre>
                      </div>
                    )}
                    {j.errorMessage && (
                      <div>
                        <p className="text-[10px] font-semibold uppercase text-destructive">Errore</p>
                        <pre className="text-xs whitespace-pre-wrap break-words bg-destructive/10 text-destructive rounded p-2 mt-1">{j.errorMessage}</pre>
                      </div>
                    )}
                  </div>
                )}
              </div>
            );
          })}
        </CardContent>
      </Card>
    </div>
  );
}
