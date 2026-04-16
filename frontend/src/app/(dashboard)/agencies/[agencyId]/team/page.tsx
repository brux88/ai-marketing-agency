"use client";

import { useParams } from "next/navigation";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import type { ApiResponse } from "@/types/api";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { Users, UserPlus, Mail, Trash2, Loader2, Crown, Shield, User, Settings2, ChevronDown, ChevronUp } from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";

interface TeamMember {
  id: string;
  email: string;
  fullName: string;
  role: string;
  createdAt: string;
  allowedAgencyIds?: string | null;
  allowedProjectIds?: string | null;
  canCreateProjects: boolean;
  canCreateApiKeys: boolean;
}

interface Invitation {
  id: string;
  email: string;
  role: string;
  status: string;
  expiresAt: string;
  createdAt: string;
}

interface Agency {
  id: string;
  name: string;
}

interface Project {
  id: string;
  name: string;
}

const roleIcons: Record<string, typeof Crown> = {
  Owner: Crown,
  Admin: Shield,
  Member: User,
};

const roleColors: Record<string, "default" | "secondary" | "outline"> = {
  Owner: "default",
  Admin: "secondary",
  Member: "outline",
};

export default function TeamPage() {
  const { agencyId } = useParams();
  const queryClient = useQueryClient();
  const [inviteEmail, setInviteEmail] = useState("");
  const [inviteRole, setInviteRole] = useState("Member");
  const [showInviteForm, setShowInviteForm] = useState(false);
  const [inviteAgencyIds, setInviteAgencyIds] = useState<string[]>([]);
  const [inviteProjectIds, setInviteProjectIds] = useState<string[]>([]);
  const [inviteCanCreateProjects, setInviteCanCreateProjects] = useState(false);
  const [inviteCanCreateApiKeys, setInviteCanCreateApiKeys] = useState(false);
  const [editingPermsId, setEditingPermsId] = useState<string | null>(null);
  const [editAgencyIds, setEditAgencyIds] = useState<string[]>([]);
  const [editProjectIds, setEditProjectIds] = useState<string[]>([]);
  const [editCanCreateProjects, setEditCanCreateProjects] = useState(false);
  const [editCanCreateApiKeys, setEditCanCreateApiKeys] = useState(false);

  const { data: membersData, isLoading: membersLoading } = useQuery({
    queryKey: ["team-members"],
    queryFn: () => apiClient.get<ApiResponse<TeamMember[]>>("/api/v1/team/members"),
  });

  const { data: invitationsData } = useQuery({
    queryKey: ["team-invitations"],
    queryFn: () => apiClient.get<ApiResponse<Invitation[]>>("/api/v1/team/invitations"),
  });

  const { data: agenciesData } = useQuery({
    queryKey: ["agencies"],
    queryFn: () => apiClient.get<ApiResponse<Agency[]>>("/api/v1/agencies"),
  });

  const { data: projectsData } = useQuery({
    queryKey: ["projects", agencyId],
    queryFn: () => apiClient.get<ApiResponse<Project[]>>(`/api/v1/agencies/${agencyId}/projects`),
    enabled: !!agencyId,
  });

  const agencies = agenciesData?.data || [];
  const projects = projectsData?.data || [];

  const inviteMutation = useMutation({
    mutationFn: () =>
      apiClient.post<ApiResponse<Invitation>>("/api/v1/team/invite", {
        email: inviteEmail,
        role: inviteRole,
        allowedAgencyIds: inviteAgencyIds.length > 0 ? inviteAgencyIds.join(",") : null,
        allowedProjectIds: inviteProjectIds.length > 0 ? inviteProjectIds.join(",") : null,
        canCreateProjects: inviteCanCreateProjects,
        canCreateApiKeys: inviteCanCreateApiKeys,
      }),
    onSuccess: () => {
      toast.success("Invito inviato!");
      setInviteEmail("");
      setShowInviteForm(false);
      setInviteAgencyIds([]);
      setInviteProjectIds([]);
      setInviteCanCreateProjects(false);
      setInviteCanCreateApiKeys(false);
      queryClient.invalidateQueries({ queryKey: ["team-invitations"] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  const revokeMutation = useMutation({
    mutationFn: (id: string) =>
      apiClient.post(`/api/v1/team/invitations/${id}/revoke`),
    onSuccess: () => {
      toast.success("Invito revocato");
      queryClient.invalidateQueries({ queryKey: ["team-invitations"] });
    },
  });

  const removeMutation = useMutation({
    mutationFn: (id: string) =>
      apiClient.delete(`/api/v1/team/members/${id}`),
    onSuccess: () => {
      toast.success("Membro rimosso");
      queryClient.invalidateQueries({ queryKey: ["team-members"] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  const updatePermsMutation = useMutation({
    mutationFn: (userId: string) =>
      apiClient.put(`/api/v1/team/members/${userId}/permissions`, {
        allowedAgencyIds: editAgencyIds.length > 0 ? editAgencyIds.join(",") : null,
        allowedProjectIds: editProjectIds.length > 0 ? editProjectIds.join(",") : null,
        canCreateProjects: editCanCreateProjects,
        canCreateApiKeys: editCanCreateApiKeys,
      }),
    onSuccess: () => {
      toast.success("Permessi aggiornati");
      setEditingPermsId(null);
      queryClient.invalidateQueries({ queryKey: ["team-members"] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  const members = membersData?.data || [];
  const invitations = invitationsData?.data || [];

  const openEditPerms = (member: TeamMember) => {
    setEditingPermsId(member.id);
    setEditAgencyIds(member.allowedAgencyIds ? member.allowedAgencyIds.split(",") : []);
    setEditProjectIds(member.allowedProjectIds ? member.allowedProjectIds.split(",") : []);
    setEditCanCreateProjects(member.canCreateProjects);
    setEditCanCreateApiKeys(member.canCreateApiKeys);
  };

  const toggleId = (list: string[], id: string, setter: (v: string[]) => void) => {
    setter(list.includes(id) ? list.filter((x) => x !== id) : [...list, id]);
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold tracking-tight">Team</h2>
          <p className="text-muted-foreground text-sm mt-1">
            Gestisci i membri del tuo workspace e invita collaboratori
          </p>
        </div>
        <Button onClick={() => setShowInviteForm(!showInviteForm)}>
          <UserPlus className="size-4" /> Invita
        </Button>
      </div>

      {/* Invite form */}
      {showInviteForm && (
        <Card>
          <CardContent className="pt-6">
            <h3 className="font-semibold mb-3">Invita un collaboratore</h3>
            <div className="flex gap-3">
              <Input
                type="email"
                placeholder="email@esempio.com"
                value={inviteEmail}
                onChange={(e) => setInviteEmail(e.target.value)}
                className="flex-1"
              />
              <select
                value={inviteRole}
                onChange={(e) => setInviteRole(e.target.value)}
                className="border rounded-lg px-3 py-2 text-sm bg-background"
              >
                <option value="Member">Membro</option>
                <option value="Admin">Admin</option>
              </select>
              <Button
                onClick={() => inviteMutation.mutate()}
                disabled={!inviteEmail || inviteMutation.isPending}
              >
                {inviteMutation.isPending ? <Loader2 className="size-4 animate-spin" /> : <Mail className="size-4" />}
                Invia
              </Button>
            </div>

            {/* Permission settings for invite */}
            <div className="mt-4 space-y-3 border-t pt-4">
              <h4 className="text-sm font-medium text-muted-foreground">Permessi</h4>

              {agencies.length > 0 && (
                <div>
                  <p className="text-xs font-medium mb-1.5">Agenzie visibili <span className="text-muted-foreground">(vuoto = tutte)</span></p>
                  <div className="flex flex-wrap gap-2">
                    {agencies.map((a) => (
                      <button
                        key={a.id}
                        type="button"
                        onClick={() => toggleId(inviteAgencyIds, a.id, setInviteAgencyIds)}
                        className={`px-2.5 py-1 text-xs rounded-full border transition-colors ${
                          inviteAgencyIds.includes(a.id) ? "bg-primary text-primary-foreground border-primary" : "hover:bg-accent"
                        }`}
                      >
                        {a.name}
                      </button>
                    ))}
                  </div>
                </div>
              )}

              {projects.length > 0 && (
                <div>
                  <p className="text-xs font-medium mb-1.5">Progetti visibili <span className="text-muted-foreground">(vuoto = tutti)</span></p>
                  <div className="flex flex-wrap gap-2">
                    {projects.map((p) => (
                      <button
                        key={p.id}
                        type="button"
                        onClick={() => toggleId(inviteProjectIds, p.id, setInviteProjectIds)}
                        className={`px-2.5 py-1 text-xs rounded-full border transition-colors ${
                          inviteProjectIds.includes(p.id) ? "bg-primary text-primary-foreground border-primary" : "hover:bg-accent"
                        }`}
                      >
                        {p.name}
                      </button>
                    ))}
                  </div>
                </div>
              )}

              <div className="flex gap-4">
                <label className="flex items-center gap-2 text-xs cursor-pointer">
                  <input
                    type="checkbox"
                    checked={inviteCanCreateProjects}
                    onChange={(e) => setInviteCanCreateProjects(e.target.checked)}
                    className="rounded border"
                  />
                  Può creare progetti
                </label>
                <label className="flex items-center gap-2 text-xs cursor-pointer">
                  <input
                    type="checkbox"
                    checked={inviteCanCreateApiKeys}
                    onChange={(e) => setInviteCanCreateApiKeys(e.target.checked)}
                    className="rounded border"
                  />
                  Può creare chiavi API
                </label>
              </div>
            </div>

            <p className="text-xs text-muted-foreground mt-3">
              L&apos;invitato riceverà un link per unirsi al workspace. L&apos;invito scade dopo 7 giorni.
            </p>
          </CardContent>
        </Card>
      )}

      {/* Members */}
      <Card>
        <CardContent className="pt-6">
          <h3 className="font-semibold mb-4 flex items-center gap-2">
            <Users className="size-4" /> Membri ({members.length})
          </h3>
          {membersLoading ? (
            <div className="space-y-3">
              {[1, 2].map((i) => <Skeleton key={i} className="h-14" />)}
            </div>
          ) : (
            <div className="space-y-2">
              {members.map((member) => {
                const RoleIcon = roleIcons[member.role] || User;
                const isEditing = editingPermsId === member.id;
                return (
                  <div key={member.id} className="rounded-lg border">
                    <div className="flex items-center justify-between p-3">
                      <div className="flex items-center gap-3">
                        <div className="size-9 rounded-full bg-primary/10 flex items-center justify-center">
                          <RoleIcon className="size-4 text-primary" />
                        </div>
                        <div>
                          <p className="text-sm font-medium">{member.fullName}</p>
                          <p className="text-xs text-muted-foreground">{member.email}</p>
                        </div>
                      </div>
                      <div className="flex items-center gap-2">
                        <Badge variant={roleColors[member.role] || "outline"}>{member.role}</Badge>
                        {member.role !== "Owner" && (
                          <>
                            <Button
                              variant="ghost"
                              size="icon-sm"
                              onClick={() => isEditing ? setEditingPermsId(null) : openEditPerms(member)}
                              title="Gestisci permessi"
                            >
                              {isEditing ? <ChevronUp className="size-3.5" /> : <Settings2 className="size-3.5" />}
                            </Button>
                            <Button
                              variant="ghost"
                              size="icon-sm"
                              onClick={() => removeMutation.mutate(member.id)}
                              className="text-destructive hover:text-destructive"
                            >
                              <Trash2 className="size-3.5" />
                            </Button>
                          </>
                        )}
                      </div>
                    </div>

                    {/* Inline permissions editor */}
                    {isEditing && (
                      <div className="px-3 pb-3 pt-1 border-t space-y-3">
                        {agencies.length > 0 && (
                          <div>
                            <p className="text-xs font-medium mb-1.5">Agenzie visibili <span className="text-muted-foreground">(vuoto = tutte)</span></p>
                            <div className="flex flex-wrap gap-2">
                              {agencies.map((a) => (
                                <button
                                  key={a.id}
                                  type="button"
                                  onClick={() => toggleId(editAgencyIds, a.id, setEditAgencyIds)}
                                  className={`px-2.5 py-1 text-xs rounded-full border transition-colors ${
                                    editAgencyIds.includes(a.id) ? "bg-primary text-primary-foreground border-primary" : "hover:bg-accent"
                                  }`}
                                >
                                  {a.name}
                                </button>
                              ))}
                            </div>
                          </div>
                        )}

                        {projects.length > 0 && (
                          <div>
                            <p className="text-xs font-medium mb-1.5">Progetti visibili <span className="text-muted-foreground">(vuoto = tutti)</span></p>
                            <div className="flex flex-wrap gap-2">
                              {projects.map((p) => (
                                <button
                                  key={p.id}
                                  type="button"
                                  onClick={() => toggleId(editProjectIds, p.id, setEditProjectIds)}
                                  className={`px-2.5 py-1 text-xs rounded-full border transition-colors ${
                                    editProjectIds.includes(p.id) ? "bg-primary text-primary-foreground border-primary" : "hover:bg-accent"
                                  }`}
                                >
                                  {p.name}
                                </button>
                              ))}
                            </div>
                          </div>
                        )}

                        <div className="flex gap-4">
                          <label className="flex items-center gap-2 text-xs cursor-pointer">
                            <input
                              type="checkbox"
                              checked={editCanCreateProjects}
                              onChange={(e) => setEditCanCreateProjects(e.target.checked)}
                              className="rounded border"
                            />
                            Può creare progetti
                          </label>
                          <label className="flex items-center gap-2 text-xs cursor-pointer">
                            <input
                              type="checkbox"
                              checked={editCanCreateApiKeys}
                              onChange={(e) => setEditCanCreateApiKeys(e.target.checked)}
                              className="rounded border"
                            />
                            Può creare chiavi API
                          </label>
                        </div>

                        <Button
                          size="sm"
                          onClick={() => updatePermsMutation.mutate(member.id)}
                          disabled={updatePermsMutation.isPending}
                        >
                          {updatePermsMutation.isPending ? <Loader2 className="size-4 animate-spin" /> : null}
                          Salva permessi
                        </Button>
                      </div>
                    )}
                  </div>
                );
              })}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Pending invitations */}
      {invitations.length > 0 && (
        <Card>
          <CardContent className="pt-6">
            <h3 className="font-semibold mb-4 flex items-center gap-2">
              <Mail className="size-4" /> Inviti in sospeso ({invitations.length})
            </h3>
            <div className="space-y-2">
              {invitations.map((inv) => (
                <div key={inv.id} className="flex items-center justify-between p-3 rounded-lg border">
                  <div>
                    <p className="text-sm font-medium">{inv.email}</p>
                    <p className="text-xs text-muted-foreground">
                      Ruolo: {inv.role} &middot; Scade: {new Date(inv.expiresAt).toLocaleDateString("it")}
                    </p>
                  </div>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => revokeMutation.mutate(inv.id)}
                    className="text-destructive"
                  >
                    Revoca
                  </Button>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
