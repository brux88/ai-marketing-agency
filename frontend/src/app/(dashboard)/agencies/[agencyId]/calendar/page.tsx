"use client";

import { useParams } from "next/navigation";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import { schedulesApi } from "@/lib/api/schedules.api";
import type { ApiResponse, GeneratedContent, ContentSchedule } from "@/types/api";
import { DayOfWeekFlag } from "@/types/api";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { ChevronLeft, ChevronRight, CalendarDays, Clock, Eye, ChevronUp, CalendarClock, Trash2 } from "lucide-react";
import { useState, useMemo } from "react";
import { toast } from "sonner";
import { resolveImageUrl } from "@/lib/utils";

interface CalendarEntry {
  id: string;
  contentId: string;
  contentTitle: string;
  contentType: string;
  platform?: string | null;
  scheduledAt: string;
  publishedAt?: string | null;
  status: string;
  errorMessage?: string | null;
  postUrl?: string | null;
}

const DAYS = ["Lun", "Mar", "Mer", "Gio", "Ven", "Sab", "Dom"];
const MONTHS = ["Gennaio", "Febbraio", "Marzo", "Aprile", "Maggio", "Giugno", "Luglio", "Agosto", "Settembre", "Ottobre", "Novembre", "Dicembre"];

const statusColors: Record<number, string> = {
  1: "bg-gray-400", 2: "bg-yellow-400", 3: "bg-blue-400", 4: "bg-green-400", 5: "bg-red-400",
};
const contentTypeColors: Record<number, string> = {
  1: "bg-blue-500", 2: "bg-green-500", 3: "bg-purple-500", 4: "bg-orange-500",
};
const contentTypeLabels: Record<number, string> = {
  1: "Blog", 2: "Social", 3: "Newsletter", 4: "Report",
};

function getMonthDays(year: number, month: number) {
  const firstDay = new Date(year, month, 1);
  const lastDay = new Date(year, month + 1, 0);
  const startPad = (firstDay.getDay() + 6) % 7; // Monday = 0
  const days: (Date | null)[] = [];
  for (let i = 0; i < startPad; i++) days.push(null);
  for (let d = 1; d <= lastDay.getDate(); d++) days.push(new Date(year, month, d));
  while (days.length % 7 !== 0) days.push(null);
  return days;
}

function isSameDay(a: Date, b: Date) {
  return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();
}

// Map JS getDay() (0=Sun..6=Sat) to server DayOfWeekFlag
function dayFlagForDate(date: Date): number {
  const map = [
    DayOfWeekFlag.Sunday,
    DayOfWeekFlag.Monday,
    DayOfWeekFlag.Tuesday,
    DayOfWeekFlag.Wednesday,
    DayOfWeekFlag.Thursday,
    DayOfWeekFlag.Friday,
    DayOfWeekFlag.Saturday,
  ];
  return map[date.getDay()];
}

const projectPalette = [
  "bg-blue-500", "bg-emerald-500", "bg-violet-500", "bg-orange-500",
  "bg-pink-500", "bg-cyan-500", "bg-amber-500", "bg-rose-500",
];

function colorForProject(projectId: string | null | undefined, projectIndex: Map<string, number>): string {
  const key = projectId || "__agency__";
  if (!projectIndex.has(key)) projectIndex.set(key, projectIndex.size);
  return projectPalette[projectIndex.get(key)! % projectPalette.length];
}

export default function CalendarPage() {
  const { agencyId } = useParams();
  const queryClient = useQueryClient();
  const [currentDate, setCurrentDate] = useState(new Date());
  const [selectedDate, setSelectedDate] = useState<Date | null>(null);

  const year = currentDate.getFullYear();
  const month = currentDate.getMonth();
  const days = getMonthDays(year, month);
  const today = new Date();

  const { data, isLoading } = useQuery({
    queryKey: ["content", agencyId],
    queryFn: () => apiClient.get<ApiResponse<GeneratedContent[]>>(`/api/v1/agencies/${agencyId}/content`),
  });

  const { data: schedulesRes } = useQuery({
    queryKey: ["schedules", agencyId],
    queryFn: () => schedulesApi.list(agencyId as string),
  });

  const { data: calendarRes } = useQuery({
    queryKey: ["agency-calendar", agencyId],
    queryFn: () => apiClient.get<ApiResponse<CalendarEntry[]>>(`/api/v1/agencies/${agencyId}/calendar`),
  });

  const removeEntry = useMutation({
    mutationFn: (entryId: string) =>
      apiClient.delete(`/api/v1/agencies/${agencyId}/calendar/${entryId}`),
    onSuccess: () => {
      toast.success("Programmazione rimossa");
      queryClient.invalidateQueries({ queryKey: ["agency-calendar", agencyId] });
      queryClient.invalidateQueries({ queryKey: ["content", agencyId] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const contents = data?.data || [];
  const schedules = (schedulesRes?.data || []) as ContentSchedule[];
  const calendarEntries = calendarRes?.data || [];
  const projectIndex = useMemo(() => new Map<string, number>(), [schedules.length]);

  const getContentsForDate = (date: Date) =>
    contents.filter((c) => isSameDay(new Date(c.createdAt), date));

  const getSchedulesForDate = (date: Date) => {
    const flag = dayFlagForDate(date);
    return schedules.filter((s) => s.isActive && (s.days & flag) !== 0);
  };

  const getCalendarEntriesForDate = (date: Date) =>
    calendarEntries.filter((e) => isSameDay(new Date(e.scheduledAt), date));

  const selectedContents = selectedDate ? getContentsForDate(selectedDate) : [];
  const selectedSchedules = selectedDate ? getSchedulesForDate(selectedDate) : [];
  const selectedEntries = selectedDate ? getCalendarEntriesForDate(selectedDate) : [];

  const prevMonth = () => setCurrentDate(new Date(year, month - 1, 1));
  const nextMonth = () => setCurrentDate(new Date(year, month + 1, 1));
  const goToday = () => { setCurrentDate(new Date()); setSelectedDate(new Date()); };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold tracking-tight">Calendario Editoriale</h2>
          <p className="text-muted-foreground text-sm mt-1">
            Visualizza e gestisci i tuoi contenuti nel tempo
          </p>
        </div>
      </div>

      {/* Calendar Header */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex items-center justify-between mb-4">
            <Button variant="outline" size="icon-sm" onClick={prevMonth}>
              <ChevronLeft className="size-4" />
            </Button>
            <div className="flex items-center gap-2">
              <h3 className="text-lg font-semibold">{MONTHS[month]} {year}</h3>
              <Button variant="ghost" size="sm" onClick={goToday}>Oggi</Button>
            </div>
            <Button variant="outline" size="icon-sm" onClick={nextMonth}>
              <ChevronRight className="size-4" />
            </Button>
          </div>

          {/* Day headers */}
          <div className="grid grid-cols-7 gap-px mb-1">
            {DAYS.map((d) => (
              <div key={d} className="text-center text-xs font-medium text-muted-foreground py-2">{d}</div>
            ))}
          </div>

          {/* Calendar grid */}
          {isLoading ? (
            <div className="grid grid-cols-7 gap-px">
              {Array.from({ length: 35 }).map((_, i) => (
                <Skeleton key={i} className="h-20 rounded" />
              ))}
            </div>
          ) : (
            <div className="grid grid-cols-7 gap-px bg-border rounded-lg overflow-hidden">
              {days.map((date, i) => {
                if (!date) return <div key={i} className="h-28 bg-muted/20" />;
                const dayContents = getContentsForDate(date);
                const daySchedules = getSchedulesForDate(date);
                const dayEntries = getCalendarEntriesForDate(date);
                const isToday = isSameDay(date, today);
                const isSelected = selectedDate && isSameDay(date, selectedDate);
                const isWeekend = date.getDay() === 0 || date.getDay() === 6;
                return (
                  <div
                    key={i}
                    className={`h-28 p-1.5 cursor-pointer transition-colors ${
                      isSelected ? "bg-primary/10 ring-2 ring-primary" :
                      isToday ? "bg-blue-50 dark:bg-blue-950/30" :
                      isWeekend ? "bg-muted/30" : "bg-background"
                    } hover:bg-accent/50`}
                    onClick={() => setSelectedDate(date)}
                  >
                    <div className={`text-xs font-medium mb-1 ${
                      isToday ? "text-primary font-bold" : "text-foreground"
                    }`}>
                      {date.getDate()}
                    </div>
                    <div className="space-y-0.5 overflow-hidden">
                      {dayEntries.slice(0, 2).map((e) => (
                        <div key={e.id} className="flex items-center gap-1" title={`${e.contentTitle} · ${new Date(e.scheduledAt).toLocaleTimeString("it-IT", { hour: "2-digit", minute: "2-digit" })}`}>
                          <CalendarClock className="size-2.5 shrink-0 text-amber-600" />
                          <span className="text-[10px] truncate">{new Date(e.scheduledAt).toLocaleTimeString("it-IT", { hour: "2-digit", minute: "2-digit" })} {e.contentTitle}</span>
                        </div>
                      ))}
                      {dayEntries.length > 2 && (
                        <span className="text-[10px] text-muted-foreground">+{dayEntries.length - 2} programm.</span>
                      )}
                      {daySchedules.slice(0, 1).map((s) => (
                        <div key={s.id} className="flex items-center gap-1" title={`${s.name} · ${s.timeOfDay}`}>
                          <div className={`size-1.5 rounded-full shrink-0 ${colorForProject(s.projectId, projectIndex)}`} />
                          <span className="text-[10px] truncate">{s.timeOfDay} {s.name}</span>
                        </div>
                      ))}
                      {daySchedules.length > 1 && (
                        <span className="text-[10px] text-muted-foreground">+{daySchedules.length - 1} sched.</span>
                      )}
                      {dayContents.length > 0 && dayEntries.length === 0 && daySchedules.length === 0 && (
                        <div className="flex items-center gap-1">
                          <div className={`size-1.5 rounded-full shrink-0 ${contentTypeColors[dayContents[0].contentType] || "bg-gray-400"}`} />
                          <span className="text-[10px] truncate">{dayContents[0].title}</span>
                        </div>
                      )}
                      {dayContents.length > 1 && dayEntries.length === 0 && daySchedules.length === 0 && (
                        <span className="text-[10px] text-muted-foreground">+{dayContents.length - 1} contenuti</span>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Selected date details */}
      {selectedDate && (
        <>
          {selectedEntries.length > 0 && (
            <Card>
              <CardContent className="pt-6">
                <h3 className="font-semibold mb-3 flex items-center gap-2">
                  <CalendarClock className="size-4 text-amber-600" />
                  Pubblicazioni programmate ({selectedEntries.length})
                </h3>
                <div className="space-y-2">
                  {selectedEntries.map((e) => (
                    <div key={e.id} className="flex items-center gap-3 p-3 rounded-lg border">
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2 flex-wrap">
                          <span className="text-sm font-medium truncate">{e.contentTitle}</span>
                          <Badge variant="outline" className="text-[10px]">{e.contentType}</Badge>
                          {e.platform && <Badge variant="outline" className="text-[10px]">{e.platform}</Badge>}
                          <Badge variant="outline" className="text-[10px]">{e.status}</Badge>
                        </div>
                        <p className="text-xs text-muted-foreground mt-1">
                          {new Date(e.scheduledAt).toLocaleString("it-IT", { day: "2-digit", month: "short", year: "numeric", hour: "2-digit", minute: "2-digit" })}
                          {e.publishedAt && ` · Pubblicato: ${new Date(e.publishedAt).toLocaleString("it-IT", { day: "2-digit", month: "short", hour: "2-digit", minute: "2-digit" })}`}
                        </p>
                        {e.postUrl && (
                          <a href={e.postUrl} target="_blank" rel="noreferrer" className="text-xs text-primary hover:underline">
                            Apri post →
                          </a>
                        )}
                      </div>
                      {e.status !== "Published" && (
                        <Button
                          variant="ghost"
                          size="icon-sm"
                          className="text-destructive shrink-0"
                          onClick={() => {
                            if (confirm("Eliminare questa pubblicazione programmata?")) {
                              removeEntry.mutate(e.id);
                            }
                          }}
                          disabled={removeEntry.isPending}
                          title="Elimina programmazione"
                        >
                          <Trash2 className="size-4" />
                        </Button>
                      )}
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          )}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          <Card>
            <CardContent className="pt-6">
              <h3 className="font-semibold mb-3 flex items-center gap-2">
                <Clock className="size-4" />
                Programmazioni ({selectedSchedules.length})
              </h3>
              {selectedSchedules.length === 0 ? (
                <div className="text-center py-8 text-muted-foreground">
                  <Clock className="size-8 mx-auto mb-2 opacity-50" />
                  <p className="text-sm">Nessuna programmazione attiva</p>
                </div>
              ) : (
                <div className="space-y-2">
                  {selectedSchedules.map((s) => (
                    <div key={s.id} className="flex items-center justify-between p-3 rounded-lg border">
                      <div className="flex items-center gap-3 min-w-0">
                        <div className={`size-2 rounded-full shrink-0 ${colorForProject(s.projectId, projectIndex)}`} />
                        <div className="min-w-0">
                          <p className="text-sm font-medium truncate">{s.name}</p>
                          <p className="text-xs text-muted-foreground">
                            {s.timeOfDay} · {s.projectName || "Agenzia"}
                          </p>
                        </div>
                      </div>
                      <Badge variant="outline">{s.timeOfDay}</Badge>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardContent className="pt-6">
              <h3 className="font-semibold mb-3 flex items-center gap-2">
                <CalendarDays className="size-4" />
                Contenuti ({selectedContents.length})
              </h3>
              {selectedContents.length === 0 ? (
                <div className="text-center py-8 text-muted-foreground">
                  <CalendarDays className="size-8 mx-auto mb-2 opacity-50" />
                  <p className="text-sm">Nessun contenuto</p>
                </div>
              ) : (
                <div className="space-y-2">
                  {selectedContents.map((c) => (
                    <CalendarContentCard key={c.id} content={c} />
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>
        </>
      )}
    </div>
  );
}

function CalendarContentCard({ content: c }: { content: GeneratedContent }) {
  const [expanded, setExpanded] = useState(false);

  return (
    <div className="rounded-lg border overflow-hidden">
      <div className="flex items-center justify-between p-3">
        <div className="flex items-center gap-3 min-w-0">
          <div className={`size-2 rounded-full shrink-0 ${statusColors[c.status]}`} />
          <div className="min-w-0">
            <p className="text-sm font-medium truncate">{c.title}</p>
            <p className="text-xs text-muted-foreground">Score: {c.overallScore}/10</p>
          </div>
        </div>
        <div className="flex items-center gap-2 shrink-0">
          <Badge variant="outline">{contentTypeLabels[c.contentType] || "Altro"}</Badge>
          <Button
            variant="ghost"
            size="icon-sm"
            onClick={() => setExpanded(!expanded)}
            title={expanded ? "Chiudi dettaglio" : "Vedi dettaglio"}
          >
            {expanded ? <ChevronUp className="size-4" /> : <Eye className="size-4" />}
          </Button>
        </div>
      </div>
      {expanded && (
        <div className="border-t px-3 pb-3 pt-2 space-y-2">
          <div className="bg-muted/50 rounded p-3 text-sm whitespace-pre-wrap max-h-80 overflow-y-auto">
            {c.body}
          </div>
          {c.imageUrl && (
            <img
              src={resolveImageUrl(c.imageUrl)}
              alt={c.title}
              className="rounded-lg max-h-48 object-cover"
            />
          )}
          {c.scoreExplanation && (
            <p className="text-xs text-muted-foreground">{c.scoreExplanation}</p>
          )}
        </div>
      )}
    </div>
  );
}
