"use client";

import { useState } from "react";
import { useParams } from "next/navigation";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { approvalsApi, type ApprovalHistoryItem } from "@/lib/api/approvals.api";
import { resolveImageUrl } from "@/lib/utils";
import type { PendingApproval } from "@/types/api";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { ClipboardCheck, Check, X, Star } from "lucide-react";
import { toast } from "sonner";

const contentTypeLabels: Record<number, string> = {
  1: "Blog",
  2: "Social",
  3: "Newsletter",
  4: "Report",
};

export default function ApprovalsPage() {
  const { agencyId } = useParams<{ agencyId: string }>();
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: ["approvals", agencyId],
    queryFn: () => approvalsApi.list(agencyId),
    refetchInterval: 30000,
  });

  const approve = useMutation({
    mutationFn: (contentId: string) => approvalsApi.approve(agencyId, contentId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["approvals", agencyId] });
      toast.success("Contenuto approvato");
    },
    onError: (err: any) => toast.error(err?.message || "Errore"),
  });

  const reject = useMutation({
    mutationFn: (contentId: string) => approvalsApi.reject(agencyId, contentId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["approvals", agencyId] });
      toast.success("Contenuto rifiutato");
    },
    onError: (err: any) => toast.error(err?.message || "Errore"),
  });

  const approvals = data?.data || [];
  const [historyFilter, setHistoryFilter] = useState<"all" | "approved" | "rejected">("all");

  const { data: historyData, isLoading: historyLoading } = useQuery({
    queryKey: ["approvals", agencyId, "history", historyFilter],
    queryFn: async () => (await approvalsApi.history(agencyId, historyFilter)).data ?? [],
    refetchInterval: 60000,
  });

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Approvazioni</h1>
        <p className="text-muted-foreground mt-1">
          Rivedi e approva i contenuti generati dagli agenti AI
        </p>
      </div>

      <Tabs defaultValue="pending">
        <TabsList>
          <TabsTrigger value="pending">
            In attesa{approvals.length > 0 ? ` · ${approvals.length}` : ""}
          </TabsTrigger>
          <TabsTrigger value="history">Storico</TabsTrigger>
        </TabsList>

        <TabsContent value="pending" className="mt-4">

      {isLoading ? (
        <div className="space-y-4">
          {[1, 2, 3].map((i) => (
            <Card key={i}>
              <CardContent className="pt-6">
                <Skeleton className="h-5 w-64" />
                <Skeleton className="h-4 w-full mt-3" />
                <Skeleton className="h-4 w-3/4 mt-2" />
              </CardContent>
            </Card>
          ))}
        </div>
      ) : approvals.length === 0 ? (
        <Card className="text-center py-16">
          <CardContent className="space-y-4">
            <div className="size-16 rounded-full bg-green-500/10 flex items-center justify-center mx-auto">
              <ClipboardCheck className="size-8 text-green-500" />
            </div>
            <h3 className="text-lg font-semibold">Nessun contenuto in attesa</h3>
            <p className="text-muted-foreground max-w-sm mx-auto">
              Tutti i contenuti sono stati revisionati. I nuovi contenuti appariranno qui quando gli agenti li genereranno.
            </p>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-4">
          <p className="text-sm text-muted-foreground">
            {approvals.length} contenut{approvals.length === 1 ? "o" : "i"} in attesa di revisione
          </p>
          {approvals.map((item) => (
            <ApprovalCard
              key={item.id}
              item={item}
              onApprove={() => approve.mutate(item.id)}
              onReject={() => reject.mutate(item.id)}
              isPending={approve.isPending || reject.isPending}
            />
          ))}
        </div>
      )}

        </TabsContent>

        <TabsContent value="history" className="mt-4 space-y-4">
          <div className="inline-flex rounded-md border text-xs">
            {(["all", "approved", "rejected"] as const).map((f) => (
              <button
                key={f}
                type="button"
                className={`px-3 py-1 ${historyFilter === f ? "bg-muted" : ""} ${f !== "all" ? "border-l" : ""}`}
                onClick={() => setHistoryFilter(f)}
              >
                {f === "all" ? "Tutti" : f === "approved" ? "Approvati" : "Rifiutati"}
              </button>
            ))}
          </div>
          {historyLoading ? (
            <div className="space-y-3">
              {[1, 2, 3].map((i) => (
                <Skeleton key={i} className="h-20" />
              ))}
            </div>
          ) : !historyData?.length ? (
            <p className="text-sm text-muted-foreground">Nessun elemento nello storico.</p>
          ) : (
            <div className="space-y-3">
              {historyData.map((item) => (
                <HistoryCard key={item.id} item={item} />
              ))}
            </div>
          )}
        </TabsContent>
      </Tabs>
    </div>
  );
}

function HistoryCard({ item }: { item: ApprovalHistoryItem }) {
  const statusBadge = () => {
    if (item.status === 4) return <Badge className="bg-emerald-500 text-white">Pubblicato</Badge>;
    if (item.status === 3) return <Badge className="bg-blue-500 text-white">Approvato</Badge>;
    if (item.status === 5) return <Badge className="bg-red-500 text-white">Rifiutato</Badge>;
    return <Badge variant="outline">{item.status}</Badge>;
  };
  const when = item.publishedAt ?? item.approvedAt ?? item.createdAt;
  return (
    <Card>
      <CardContent className="pt-5">
        <div className="flex items-start gap-3">
          {item.imageUrl && (
            <img src={resolveImageUrl(item.imageUrl)} alt={item.title} className="w-16 h-16 rounded object-cover shrink-0" />
          )}
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2 flex-wrap">
              <span className="font-medium text-sm truncate">{item.title}</span>
              {statusBadge()}
              <Badge variant="outline" className="text-[10px]">
                {contentTypeLabels[item.contentType] ?? "Altro"}
              </Badge>
              {item.projectName && (
                <Badge variant="secondary" className="text-[10px]">{item.projectName}</Badge>
              )}
              {item.autoApproved && (
                <Badge variant="outline" className="text-[10px]">Auto</Badge>
              )}
              <span className="text-xs text-muted-foreground flex items-center gap-1">
                <Star className="size-3" />
                {item.overallScore.toFixed(1)}/10
              </span>
            </div>
            <p className="text-xs text-muted-foreground line-clamp-2 mt-1">{item.body}</p>
            <p className="text-[10px] text-muted-foreground mt-1">
              {new Date(when).toLocaleString("it-IT")}
            </p>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

function ApprovalCard({
  item,
  onApprove,
  onReject,
  isPending,
}: {
  item: PendingApproval;
  onApprove: () => void;
  onReject: () => void;
  isPending: boolean;
}) {
  return (
    <Card>
      <CardHeader className="pb-3">
        <div className="flex items-start justify-between">
          <div>
            <CardTitle className="text-base">{item.title}</CardTitle>
            <CardDescription className="flex items-center gap-2 mt-1">
              <Badge variant="outline">{contentTypeLabels[item.contentType] || "Altro"}</Badge>
              {item.projectName && <Badge variant="secondary">{item.projectName}</Badge>}
              <span className="flex items-center gap-1">
                <Star className="size-3" />
                {item.overallScore.toFixed(1)}/10
              </span>
              <span>{new Date(item.createdAt).toLocaleString("it-IT")}</span>
            </CardDescription>
          </div>
          <div className="flex gap-2">
            <Button
              size="sm"
              variant="outline"
              className="text-destructive hover:text-destructive"
              onClick={onReject}
              disabled={isPending}
            >
              <X className="size-4" /> Rifiuta
            </Button>
            <Button size="sm" onClick={onApprove} disabled={isPending}>
              <Check className="size-4" /> Approva
            </Button>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <div className="prose prose-sm max-w-none dark:prose-invert">
          <div className="bg-muted/50 rounded-lg p-4 max-h-64 overflow-y-auto text-sm whitespace-pre-wrap">
            {item.body.length > 500 ? item.body.substring(0, 500) + "..." : item.body}
          </div>
        </div>
        {item.imageUrl && (
          <div className="mt-3">
            <img
              src={resolveImageUrl(item.imageUrl)}
              alt={item.title}
              className="rounded-lg max-h-48 object-cover"
            />
          </div>
        )}
        {item.scoreExplanation && (
          <p className="text-xs text-muted-foreground mt-3">{item.scoreExplanation}</p>
        )}
      </CardContent>
    </Card>
  );
}
