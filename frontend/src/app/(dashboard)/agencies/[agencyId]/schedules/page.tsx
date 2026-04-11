"use client";

import { useState } from "react";
import { useParams } from "next/navigation";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { schedulesApi } from "@/lib/api/schedules.api";
import type { ContentSchedule } from "@/types/api";
import { DayOfWeekFlag } from "@/types/api";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { Skeleton } from "@/components/ui/skeleton";
import { Calendar, Plus, Trash2, X, Loader2, Pause, Play } from "lucide-react";
import { toast } from "sonner";

const agentTypeLabels: Record<number, string> = {
  1: "Content Writer",
  2: "Social Manager",
  3: "Newsletter",
  4: "Analytics",
};

const dayFlags = [
  { flag: DayOfWeekFlag.Monday, label: "Lun" },
  { flag: DayOfWeekFlag.Tuesday, label: "Mar" },
  { flag: DayOfWeekFlag.Wednesday, label: "Mer" },
  { flag: DayOfWeekFlag.Thursday, label: "Gio" },
  { flag: DayOfWeekFlag.Friday, label: "Ven" },
  { flag: DayOfWeekFlag.Saturday, label: "Sab" },
  { flag: DayOfWeekFlag.Sunday, label: "Dom" },
];

type FormState = {
  name: string;
  days: number;
  timeOfDay: string;
  timeZone: string;
  agentType: number;
  input: string;
};

const defaultForm: FormState = {
  name: "",
  days: DayOfWeekFlag.Weekdays,
  timeOfDay: "09:00",
  timeZone: "Europe/Rome",
  agentType: 1,
  input: "",
};

export default function SchedulesPage() {
  const { agencyId } = useParams<{ agencyId: string }>();
  const queryClient = useQueryClient();
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState<FormState>(defaultForm);

  const { data, isLoading } = useQuery({
    queryKey: ["schedules", agencyId],
    queryFn: () => schedulesApi.list(agencyId),
  });

  const createSchedule = useMutation({
    mutationFn: (data: FormState) =>
      schedulesApi.create(agencyId, {
        ...data,
        input: data.input || null,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["schedules", agencyId] });
      setShowForm(false);
      setForm(defaultForm);
      toast.success("Programmazione creata");
    },
    onError: (err: any) => toast.error(err?.message || "Errore"),
  });

  const toggleSchedule = useMutation({
    mutationFn: (schedule: ContentSchedule) =>
      schedulesApi.update(agencyId, schedule.id, {
        name: schedule.name,
        days: schedule.days,
        timeOfDay: schedule.timeOfDay,
        timeZone: schedule.timeZone,
        agentType: schedule.agentType,
        input: schedule.input,
        isActive: !schedule.isActive,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["schedules", agencyId] });
      toast.success("Stato aggiornato");
    },
    onError: (err: any) => toast.error(err?.message || "Errore"),
  });

  const deleteSchedule = useMutation({
    mutationFn: (id: string) => schedulesApi.delete(agencyId, id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["schedules", agencyId] });
      toast.success("Programmazione eliminata");
    },
    onError: (err: any) => toast.error(err?.message || "Errore"),
  });

  const toggleDay = (flag: number) => {
    setForm((f) => ({ ...f, days: f.days ^ flag }));
  };

  const schedules = data?.data || [];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Programmazione Contenuti</h1>
          <p className="text-muted-foreground mt-1">
            Configura la generazione automatica di contenuti
          </p>
        </div>
        <Button onClick={() => setShowForm(!showForm)}>
          {showForm ? (
            <>
              <X className="size-4" /> Chiudi
            </>
          ) : (
            <>
              <Plus className="size-4" /> Nuova programmazione
            </>
          )}
        </Button>
      </div>

      {showForm && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Nuova programmazione</CardTitle>
            <CardDescription>
              Configura quando e cosa generare automaticamente
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label>Nome</Label>
              <Input
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
                placeholder="es. Articoli settimanali"
              />
            </div>

            <div className="space-y-2">
              <Label>Tipo agente</Label>
              <select
                value={form.agentType}
                onChange={(e) =>
                  setForm({ ...form, agentType: Number(e.target.value) })
                }
                className="flex h-9 w-full rounded-lg border border-input bg-transparent px-3 py-1 text-sm shadow-xs transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
              >
                {Object.entries(agentTypeLabels).map(([key, label]) => (
                  <option key={key} value={key}>
                    {label}
                  </option>
                ))}
              </select>
            </div>

            <div className="space-y-2">
              <Label>Giorni</Label>
              <div className="flex gap-2">
                {dayFlags.map(({ flag, label }) => (
                  <button
                    key={flag}
                    type="button"
                    onClick={() => toggleDay(flag)}
                    className={`px-3 py-1.5 rounded-md text-sm font-medium border transition-colors ${
                      form.days & flag
                        ? "bg-primary text-primary-foreground border-primary"
                        : "bg-muted text-muted-foreground border-transparent hover:bg-muted/80"
                    }`}
                  >
                    {label}
                  </button>
                ))}
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Orario</Label>
                <Input
                  type="time"
                  value={form.timeOfDay}
                  onChange={(e) =>
                    setForm({ ...form, timeOfDay: e.target.value })
                  }
                />
              </div>
              <div className="space-y-2">
                <Label>Fuso orario</Label>
                <select
                  value={form.timeZone}
                  onChange={(e) =>
                    setForm({ ...form, timeZone: e.target.value })
                  }
                  className="flex h-9 w-full rounded-lg border border-input bg-transparent px-3 py-1 text-sm shadow-xs transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
                >
                  <option value="Europe/Rome">Europe/Rome (CET)</option>
                  <option value="Europe/London">Europe/London (GMT)</option>
                  <option value="America/New_York">America/New York (EST)</option>
                  <option value="America/Los_Angeles">America/Los Angeles (PST)</option>
                  <option value="Asia/Tokyo">Asia/Tokyo (JST)</option>
                </select>
              </div>
            </div>

            <div className="space-y-2">
              <Label>Input aggiuntivo (opzionale)</Label>
              <Input
                value={form.input}
                onChange={(e) => setForm({ ...form, input: e.target.value })}
                placeholder="es. Focus su AI e tecnologia"
              />
            </div>

            <Separator />
            <div className="flex gap-2">
              <Button
                onClick={() => createSchedule.mutate(form)}
                disabled={createSchedule.isPending || !form.name}
              >
                {createSchedule.isPending ? (
                  <>
                    <Loader2 className="size-4 animate-spin" /> Salvataggio...
                  </>
                ) : (
                  <>
                    <Calendar className="size-4" /> Crea programmazione
                  </>
                )}
              </Button>
              <Button variant="ghost" onClick={() => setShowForm(false)}>
                Annulla
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {isLoading ? (
        <div className="space-y-4">
          {[1, 2].map((i) => (
            <Card key={i}>
              <CardContent className="pt-6 flex items-center gap-4">
                <Skeleton className="size-10 rounded-lg" />
                <div className="flex-1">
                  <Skeleton className="h-4 w-48" />
                  <Skeleton className="h-3 w-32 mt-2" />
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      ) : schedules.length === 0 ? (
        <Card className="text-center py-16">
          <CardContent className="space-y-4">
            <div className="size-16 rounded-full bg-primary/10 flex items-center justify-center mx-auto">
              <Calendar className="size-8 text-primary" />
            </div>
            <h3 className="text-lg font-semibold">Nessuna programmazione</h3>
            <p className="text-muted-foreground max-w-sm mx-auto">
              Crea una programmazione per generare contenuti automaticamente
            </p>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-3">
          {schedules.map((schedule) => (
            <Card key={schedule.id}>
              <CardContent className="pt-6">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <div
                      className={`size-10 rounded-lg flex items-center justify-center ${
                        schedule.isActive ? "bg-primary/10" : "bg-muted"
                      }`}
                    >
                      <Calendar
                        className={`size-4 ${
                          schedule.isActive ? "text-primary" : "text-muted-foreground"
                        }`}
                      />
                    </div>
                    <div>
                      <h4 className="font-medium">{schedule.name}</h4>
                      <p className="text-sm text-muted-foreground">
                        {agentTypeLabels[schedule.agentType]} &middot;{" "}
                        {formatDays(schedule.days)} alle {schedule.timeOfDay}
                        {schedule.projectName && ` · ${schedule.projectName}`}
                      </p>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    {schedule.nextRunAt && (
                      <span className="text-xs text-muted-foreground">
                        Prossimo: {new Date(schedule.nextRunAt).toLocaleString("it-IT")}
                      </span>
                    )}
                    <Badge variant={schedule.isActive ? "default" : "secondary"}>
                      {schedule.isActive ? "Attiva" : "In pausa"}
                    </Badge>
                    <Button
                      variant="ghost"
                      size="icon-sm"
                      onClick={() => toggleSchedule.mutate(schedule)}
                      disabled={toggleSchedule.isPending}
                    >
                      {schedule.isActive ? (
                        <Pause className="size-4" />
                      ) : (
                        <Play className="size-4" />
                      )}
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon-sm"
                      className="text-destructive hover:text-destructive"
                      onClick={() => deleteSchedule.mutate(schedule.id)}
                      disabled={deleteSchedule.isPending}
                    >
                      <Trash2 className="size-4" />
                    </Button>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}

function formatDays(days: number): string {
  if (days === DayOfWeekFlag.EveryDay) return "Ogni giorno";
  if (days === DayOfWeekFlag.Weekdays) return "Lun-Ven";
  if (days === DayOfWeekFlag.Weekend) return "Sab-Dom";

  const names: string[] = [];
  for (const { flag, label } of [
    { flag: DayOfWeekFlag.Monday, label: "Lun" },
    { flag: DayOfWeekFlag.Tuesday, label: "Mar" },
    { flag: DayOfWeekFlag.Wednesday, label: "Mer" },
    { flag: DayOfWeekFlag.Thursday, label: "Gio" },
    { flag: DayOfWeekFlag.Friday, label: "Ven" },
    { flag: DayOfWeekFlag.Saturday, label: "Sab" },
    { flag: DayOfWeekFlag.Sunday, label: "Dom" },
  ]) {
    if (days & flag) names.push(label);
  }
  return names.join(", ");
}
