"use client";

import { Suspense, useState } from "react";
import { useSearchParams } from "next/navigation";
import Link from "next/link";
import { authApi } from "@/lib/api/auth.api";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button, buttonVariants } from "@/components/ui/button";
import { Trash2, Loader2, CheckCircle2, XCircle, AlertTriangle } from "lucide-react";

export default function ConfirmDeleteAccountPage() {
  return (
    <Suspense fallback={<Card><CardContent className="py-10 text-center"><Loader2 className="size-8 animate-spin mx-auto" /></CardContent></Card>}>
      <ConfirmDeleteAccountContent />
    </Suspense>
  );
}

function ConfirmDeleteAccountContent() {
  const searchParams = useSearchParams();
  const token = searchParams.get("token");
  const [status, setStatus] = useState<"pending" | "loading" | "success" | "error">("pending");
  const [errorMsg, setErrorMsg] = useState("");

  if (!token) {
    return (
      <Card>
        <CardContent className="text-center py-10 space-y-4">
          <XCircle className="size-12 text-destructive mx-auto" />
          <p className="text-lg font-medium">Link non valido</p>
          <p className="text-muted-foreground">Il link di eliminazione non è valido o è scaduto.</p>
          <Link href="/login" className={buttonVariants({ variant: "outline" })}>
            Torna al login
          </Link>
        </CardContent>
      </Card>
    );
  }

  const handleDelete = async () => {
    setStatus("loading");
    try {
      await authApi.confirmAccountDeletion(token);
      setStatus("success");
      localStorage.clear();
    } catch (err: unknown) {
      setStatus("error");
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error;
      setErrorMsg(msg || "Si è verificato un errore. Il link potrebbe essere scaduto.");
    }
  };

  if (status === "success") {
    return (
      <Card>
        <CardHeader className="text-center">
          <CardTitle className="text-2xl">Account eliminato</CardTitle>
        </CardHeader>
        <CardContent className="text-center space-y-4 py-6">
          <CheckCircle2 className="size-12 text-green-500 mx-auto" />
          <p className="text-muted-foreground">Il tuo account è stato eliminato con successo.</p>
          <p className="text-sm text-muted-foreground">Ci dispiace vederti andare via. Puoi sempre creare un nuovo account in futuro.</p>
          <Link href="/" className={buttonVariants({ variant: "default" }) + " mt-4"}>
            Torna alla home
          </Link>
        </CardContent>
      </Card>
    );
  }

  if (status === "error") {
    return (
      <Card>
        <CardContent className="text-center py-10 space-y-4">
          <XCircle className="size-12 text-destructive mx-auto" />
          <p className="text-lg font-medium">Errore</p>
          <p className="text-muted-foreground">{errorMsg}</p>
          <Link href="/login" className={buttonVariants({ variant: "outline" })}>
            Torna al login
          </Link>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader className="text-center">
        <CardTitle className="text-2xl text-destructive">Eliminazione Account</CardTitle>
      </CardHeader>
      <CardContent className="space-y-6">
        <div className="flex items-start gap-3 p-4 bg-destructive/5 rounded-lg border border-destructive/20">
          <AlertTriangle className="size-5 text-destructive shrink-0 mt-0.5" />
          <div className="text-sm">
            <p className="font-medium text-destructive">Questa azione è irreversibile.</p>
            <p className="mt-1 text-muted-foreground">Cliccando il pulsante qui sotto, il tuo account e tutti i dati associati verranno eliminati definitivamente.</p>
          </div>
        </div>
        <div className="flex gap-3 justify-center">
          <Button
            variant="destructive"
            disabled={status === "loading"}
            onClick={handleDelete}
          >
            {status === "loading" ? (
              <><Loader2 className="size-4 animate-spin" /> Eliminazione...</>
            ) : (
              <><Trash2 className="size-4" /> Elimina definitivamente</>
            )}
          </Button>
          <Link href="/dashboard" className={buttonVariants({ variant: "outline" })}>
            Annulla
          </Link>
        </div>
      </CardContent>
    </Card>
  );
}
