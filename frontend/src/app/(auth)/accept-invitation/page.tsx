"use client";

import { Suspense, useState } from "react";
import { useSearchParams } from "next/navigation";
import Link from "next/link";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button, buttonVariants } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { CheckCircle2, XCircle, Loader2, UserPlus } from "lucide-react";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

export default function AcceptInvitationPage() {
  return (
    <Suspense fallback={<Card><CardContent className="py-10 text-center"><Loader2 className="size-8 animate-spin mx-auto" /></CardContent></Card>}>
      <AcceptInvitationContent />
    </Suspense>
  );
}

function AcceptInvitationContent() {
  const searchParams = useSearchParams();
  const token = searchParams.get("token");
  const [fullName, setFullName] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [status, setStatus] = useState<"form" | "loading" | "success" | "error">("form");
  const [errorMsg, setErrorMsg] = useState("");

  if (!token) {
    return (
      <Card>
        <CardContent className="text-center py-10 space-y-4">
          <XCircle className="size-12 text-destructive mx-auto" />
          <p className="text-lg font-medium">Link non valido</p>
          <p className="text-muted-foreground">Il link di invito non è valido o è scaduto.</p>
          <Link href="/login" className={buttonVariants({ variant: "outline" })}>
            Torna al login
          </Link>
        </CardContent>
      </Card>
    );
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (password.length < 6) {
      setErrorMsg("La password deve avere almeno 6 caratteri.");
      return;
    }

    if (password !== confirmPassword) {
      setErrorMsg("Le password non corrispondono.");
      return;
    }

    setErrorMsg("");
    setStatus("loading");

    try {
      const res = await fetch(`${API_URL}/api/v1/team/accept-invitation`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ token, fullName: fullName || undefined, password }),
      });

      if (!res.ok) {
        const data = await res.json().catch(() => ({ error: "Errore durante l'accettazione." }));
        throw new Error(data.error || `Errore ${res.status}`);
      }

      setStatus("success");
    } catch (err: unknown) {
      setStatus("error");
      setErrorMsg(err instanceof Error ? err.message : "Si è verificato un errore.");
    }
  };

  if (status === "success") {
    return (
      <Card>
        <CardHeader className="text-center">
          <CardTitle className="text-2xl">Invito accettato!</CardTitle>
        </CardHeader>
        <CardContent className="text-center space-y-4 py-6">
          <CheckCircle2 className="size-12 text-green-500 mx-auto" />
          <p className="text-muted-foreground">
            Il tuo account è stato creato con successo. Ora puoi accedere con le tue credenziali.
          </p>
          <Link href="/login" className={buttonVariants({ variant: "default" }) + " mt-4"}>
            Vai al login
          </Link>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader className="text-center">
        <CardTitle className="text-2xl">Accetta invito</CardTitle>
        <p className="text-muted-foreground mt-2">
          Completa la registrazione per unirti al team.
        </p>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="fullName">Nome completo</Label>
            <Input
              id="fullName"
              value={fullName}
              onChange={(e) => setFullName(e.target.value)}
              placeholder="Mario Rossi"
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="password">Password</Label>
            <Input
              id="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              placeholder="Minimo 6 caratteri"
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="confirmPassword">Conferma password</Label>
            <Input
              id="confirmPassword"
              type="password"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              required
            />
          </div>
          {errorMsg && (
            <p className="text-sm text-destructive">{errorMsg}</p>
          )}
          <Button type="submit" className="w-full" disabled={status === "loading"}>
            {status === "loading" ? (
              <><Loader2 className="size-4 animate-spin" /> Accettazione in corso...</>
            ) : (
              <><UserPlus className="size-4" /> Accetta e crea account</>
            )}
          </Button>
        </form>
        {status === "error" && (
          <div className="mt-4 text-center">
            <Link href="/login" className={buttonVariants({ variant: "outline", size: "sm" })}>
              Torna al login
            </Link>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
