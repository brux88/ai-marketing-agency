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
import { Users, UserPlus, Mail, Trash2, Loader2, Crown, Shield, User } from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";

interface TeamMember {
  id: string;
  email: string;
  fullName: string;
  role: string;
  createdAt: string;
}

interface Invitation {
  id: string;
  email: string;
  role: string;
  status: string;
  expiresAt: string;
  createdAt: string;
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
  const queryClient = useQueryClient();
  const [inviteEmail, setInviteEmail] = useState("");
  const [inviteRole, setInviteRole] = useState("Member");
  const [showInviteForm, setShowInviteForm] = useState(false);

  const { data: membersData, isLoading: membersLoading } = useQuery({
    queryKey: ["team-members"],
    queryFn: () => apiClient.get<ApiResponse<TeamMember[]>>("/api/v1/team/members"),
  });

  const { data: invitationsData } = useQuery({
    queryKey: ["team-invitations"],
    queryFn: () => apiClient.get<ApiResponse<Invitation[]>>("/api/v1/team/invitations"),
  });

  const inviteMutation = useMutation({
    mutationFn: () =>
      apiClient.post<ApiResponse<Invitation>>("/api/v1/team/invite", {
        email: inviteEmail,
        role: inviteRole,
      }),
    onSuccess: () => {
      toast.success("Invito inviato!");
      setInviteEmail("");
      setShowInviteForm(false);
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

  const members = membersData?.data || [];
  const invitations = invitationsData?.data || [];

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
            <p className="text-xs text-muted-foreground mt-2">
              L&apos;invitato ricevera un link per unirsi al workspace. L&apos;invito scade dopo 7 giorni.
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
                return (
                  <div key={member.id} className="flex items-center justify-between p-3 rounded-lg border">
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
                        <Button
                          variant="ghost"
                          size="icon-sm"
                          onClick={() => removeMutation.mutate(member.id)}
                          className="text-destructive hover:text-destructive"
                        >
                          <Trash2 className="size-3.5" />
                        </Button>
                      )}
                    </div>
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
