"use client";

import { useParams } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import type { ApiResponse, GeneratedContent } from "@/types/api";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { ChevronLeft, ChevronRight, CalendarDays } from "lucide-react";
import { useState } from "react";

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

export default function CalendarPage() {
  const { agencyId } = useParams();
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

  const contents = data?.data || [];

  const getContentsForDate = (date: Date) =>
    contents.filter((c) => isSameDay(new Date(c.createdAt), date));

  const selectedContents = selectedDate ? getContentsForDate(selectedDate) : [];

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
                if (!date) return <div key={i} className="h-24 bg-muted/20" />;
                const dayContents = getContentsForDate(date);
                const isToday = isSameDay(date, today);
                const isSelected = selectedDate && isSameDay(date, selectedDate);
                const isWeekend = date.getDay() === 0 || date.getDay() === 6;
                return (
                  <div
                    key={i}
                    className={`h-24 p-1.5 cursor-pointer transition-colors ${
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
                      {dayContents.slice(0, 3).map((c) => (
                        <div key={c.id} className="flex items-center gap-1">
                          <div className={`size-1.5 rounded-full shrink-0 ${contentTypeColors[c.contentType] || "bg-gray-400"}`} />
                          <span className="text-[10px] truncate">{c.title}</span>
                        </div>
                      ))}
                      {dayContents.length > 3 && (
                        <span className="text-[10px] text-muted-foreground">+{dayContents.length - 3} altri</span>
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
        <Card>
          <CardContent className="pt-6">
            <h3 className="font-semibold mb-3">
              {selectedDate.getDate()} {MONTHS[selectedDate.getMonth()]} {selectedDate.getFullYear()}
              <span className="text-muted-foreground font-normal ml-2">
                ({selectedContents.length} contenuti)
              </span>
            </h3>
            {selectedContents.length === 0 ? (
              <div className="text-center py-8 text-muted-foreground">
                <CalendarDays className="size-8 mx-auto mb-2 opacity-50" />
                <p className="text-sm">Nessun contenuto per questa data</p>
              </div>
            ) : (
              <div className="space-y-3">
                {selectedContents.map((c) => (
                  <div key={c.id} className="flex items-center justify-between p-3 rounded-lg border">
                    <div className="flex items-center gap-3 min-w-0">
                      <div className={`size-2 rounded-full ${statusColors[c.status]}`} />
                      <div className="min-w-0">
                        <p className="text-sm font-medium truncate">{c.title}</p>
                        <p className="text-xs text-muted-foreground">Score: {c.overallScore}/10</p>
                      </div>
                    </div>
                    <Badge variant="outline">{contentTypeLabels[c.contentType] || "Altro"}</Badge>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      )}
    </div>
  );
}
