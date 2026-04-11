"use client";

import { useParams } from "next/navigation";
import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { projectsApi } from "@/lib/api/projects.api";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Badge } from "@/components/ui/badge";
import { FolderKanban, Plus, FileText, Rss, ArrowRight } from "lucide-react";
import { cn, } from "@/lib/utils";
import { buttonVariants } from "@/components/ui/button";

export default function ProjectsPage() {
  const { agencyId } = useParams();

  const { data: projects, isLoading } = useQuery({
    queryKey: ["projects", agencyId],
    queryFn: () => projectsApi.list(agencyId as string),
  });

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-64" />
        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {[1, 2, 3].map((i) => (
            <Skeleton key={i} className="h-40" />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold tracking-tight">Progetti</h2>
          <p className="text-muted-foreground text-sm mt-1">
            Organizza i tuoi prodotti e servizi in progetti separati
          </p>
        </div>
        <Link
          href={`/agencies/${agencyId}/projects/new`}
          className={cn(buttonVariants({ variant: "default" }))}
        >
          <Plus className="size-4" /> Nuovo Progetto
        </Link>
      </div>

      {!projects || projects.length === 0 ? (
        <Card className="text-center py-16">
          <CardContent className="space-y-4">
            <div className="size-16 rounded-full bg-primary/10 flex items-center justify-center mx-auto">
              <FolderKanban className="size-8 text-primary" />
            </div>
            <h3 className="text-lg font-semibold">Nessun progetto</h3>
            <p className="text-muted-foreground max-w-sm mx-auto">
              Crea il primo progetto per organizzare i tuoi prodotti.
            </p>
          </CardContent>
        </Card>
      ) : (
        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {projects.map((project) => (
            <Link key={project.id} href={`/agencies/${agencyId}/projects/${project.id}`}>
              <Card className="hover:shadow-md transition-all duration-200 hover:-translate-y-0.5 h-full group">
                <CardContent className="pt-6">
                  <div className="flex items-start justify-between mb-3">
                    <div className="size-10 rounded-lg bg-primary/10 flex items-center justify-center">
                      <FolderKanban className="size-5 text-primary" />
                    </div>
                    {!project.isActive && (
                      <Badge variant="secondary">Inattivo</Badge>
                    )}
                  </div>
                  <h4 className="font-semibold">{project.name}</h4>
                  {project.description && (
                    <p className="text-muted-foreground text-sm mt-1 line-clamp-2">{project.description}</p>
                  )}
                  <div className="flex items-center gap-4 mt-4 text-xs text-muted-foreground">
                    <span className="flex items-center gap-1">
                      <FileText className="size-3" /> {project.generatedContentsCount} contenuti
                    </span>
                    <span className="flex items-center gap-1">
                      <Rss className="size-3" /> {project.contentSourcesCount} fonti
                    </span>
                  </div>
                  <div className="flex items-center gap-1 text-primary text-sm font-medium mt-3 opacity-0 group-hover:opacity-100 transition-opacity">
                    Apri <ArrowRight className="size-3" />
                  </div>
                </CardContent>
              </Card>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
