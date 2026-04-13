"use client";

import { useState } from "react";
import { useParams } from "next/navigation";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { agenciesApi } from "@/lib/api/agencies.api";
import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Mic2, Target, Rss, Brain, Shield, ImageIcon } from "lucide-react";
import { EditBrandVoiceDialog } from "@/components/agency/settings/edit-brand-voice-dialog";
import { EditTargetAudienceDialog } from "@/components/agency/settings/edit-target-audience-dialog";
import { EditApprovalModeDialog } from "@/components/agency/settings/edit-approval-mode-dialog";
import { EditDefaultLlmDialog } from "@/components/agency/settings/edit-default-llm-dialog";
import { EditContentSourcesDialog } from "@/components/agency/settings/edit-content-sources-dialog";
import { EditImageSettingsDialog } from "@/components/agency/settings/edit-image-settings-dialog";

const sections = [
  { key: "brandVoice", title: "Brand Voice", desc: "Tono, stile e parole chiave del brand", icon: Mic2, color: "text-blue-600 bg-blue-100 dark:bg-blue-950" },
  { key: "targetAudience", title: "Target Audience", desc: "Profili audience e pain points", icon: Target, color: "text-violet-600 bg-violet-100 dark:bg-violet-950" },
  { key: "contentSources", title: "Fonti Contenuto", desc: "RSS, siti web e social da monitorare", icon: Rss, color: "text-emerald-600 bg-emerald-100 dark:bg-emerald-950" },
  { key: "defaultLlm", title: "Provider LLM", desc: "Configurazione AI e modelli", icon: Brain, color: "text-amber-600 bg-amber-100 dark:bg-amber-950" },
  { key: "approvalMode", title: "Approvazione", desc: "Modalita di approvazione contenuti", icon: Shield, color: "text-red-600 bg-red-100 dark:bg-red-950" },
  { key: "imageSettings", title: "Logo Overlay", desc: "Applica logo alle immagini generate", icon: ImageIcon, color: "text-pink-600 bg-pink-100 dark:bg-pink-950" },
];

export default function AgencySettingsPage() {
  const { agencyId } = useParams();
  const queryClient = useQueryClient();
  const [editDialog, setEditDialog] = useState<string | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ["agency", agencyId],
    queryFn: () => agenciesApi.get(agencyId as string),
  });

  const agency = data?.data;

  const handleSaved = () => {
    queryClient.invalidateQueries({ queryKey: ["agency", agencyId] });
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-64" />
        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {[1, 2, 3, 4, 5].map((i) => (
            <Skeleton key={i} className="h-32" />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-xl font-bold tracking-tight">Impostazioni Agenzia</h2>
        <p className="text-muted-foreground text-sm mt-1">
          Configura brand voice, audience, fonti e modalita di approvazione
        </p>
      </div>

      <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {sections.map((s) => (
          <Card
            key={s.key}
            className="hover:shadow-sm transition-shadow cursor-pointer"
            onClick={() => setEditDialog(s.key)}
          >
            <CardContent className="pt-6">
              <div className={`size-10 rounded-lg flex items-center justify-center mb-3 ${s.color}`}>
                <s.icon className="size-5" />
              </div>
              <h4 className="font-semibold">{s.title}</h4>
              <p className="text-muted-foreground text-sm mt-1">{s.desc}</p>
            </CardContent>
          </Card>
        ))}
      </div>

      {agency && (
        <>
          <EditBrandVoiceDialog
            open={editDialog === "brandVoice"}
            onOpenChange={(open) => !open && setEditDialog(null)}
            agency={agency}
            onSaved={handleSaved}
          />
          <EditTargetAudienceDialog
            open={editDialog === "targetAudience"}
            onOpenChange={(open) => !open && setEditDialog(null)}
            agency={agency}
            onSaved={handleSaved}
          />
          <EditApprovalModeDialog
            open={editDialog === "approvalMode"}
            onOpenChange={(open) => !open && setEditDialog(null)}
            agency={agency}
            onSaved={handleSaved}
          />
          <EditDefaultLlmDialog
            open={editDialog === "defaultLlm"}
            onOpenChange={(open) => !open && setEditDialog(null)}
            agency={agency}
            onSaved={handleSaved}
          />
          <EditContentSourcesDialog
            open={editDialog === "contentSources"}
            onOpenChange={(open) => !open && setEditDialog(null)}
            agencyId={agencyId as string}
            onSaved={handleSaved}
          />
          <EditImageSettingsDialog
            open={editDialog === "imageSettings"}
            onOpenChange={(open) => !open && setEditDialog(null)}
            agency={agency}
            onSaved={handleSaved}
          />
        </>
      )}
    </div>
  );
}
