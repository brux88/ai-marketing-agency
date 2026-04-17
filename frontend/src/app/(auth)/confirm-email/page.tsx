"use client";

import { Suspense, useEffect, useState } from "react";
import { useSearchParams } from "next/navigation";
import Link from "next/link";
import { authApi } from "@/lib/api/auth.api";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { buttonVariants } from "@/components/ui/button";
import { CheckCircle2, XCircle, Loader2 } from "lucide-react";

export default function ConfirmEmailPage() {
  return (
    <Suspense fallback={<Card><CardContent className="py-10 text-center"><Loader2 className="size-8 animate-spin mx-auto" /></CardContent></Card>}>
      <ConfirmEmailContent />
    </Suspense>
  );
}

function ConfirmEmailContent() {
  const searchParams = useSearchParams();
  const token = searchParams.get("token");
  const [status, setStatus] = useState<"loading" | "success" | "error">("loading");
  const [errorMsg, setErrorMsg] = useState("");

  useEffect(() => {
    if (!token) {
      setStatus("error");
      setErrorMsg("Token di conferma mancante.");
      return;
    }

    authApi
      .confirmEmail(token)
      .then(() => setStatus("success"))
      .catch((err) => {
        setStatus("error");
        setErrorMsg(err?.response?.data?.error || "Token non valido o scaduto.");
      });
  }, [token]);

  return (
    <Card>
      <CardHeader className="text-center">
        <CardTitle className="text-2xl">Conferma Email</CardTitle>
      </CardHeader>
      <CardContent className="text-center space-y-4">
        {status === "loading" && (
          <div className="flex flex-col items-center gap-3 py-6">
            <Loader2 className="size-10 animate-spin text-primary" />
            <p className="text-muted-foreground">Conferma in corso...</p>
          </div>
        )}
        {status === "success" && (
          <div className="flex flex-col items-center gap-3 py-6">
            <CheckCircle2 className="size-12 text-green-500" />
            <p className="text-lg font-medium">Email confermata con successo!</p>
            <p className="text-muted-foreground">Ora puoi accedere a tutte le funzionalità.</p>
            <Link href="/login" className={buttonVariants({ variant: "default" }) + " mt-4"}>
              Accedi
            </Link>
          </div>
        )}
        {status === "error" && (
          <div className="flex flex-col items-center gap-3 py-6">
            <XCircle className="size-12 text-destructive" />
            <p className="text-lg font-medium">Errore</p>
            <p className="text-muted-foreground">{errorMsg}</p>
            <Link href="/login" className={buttonVariants({ variant: "outline" }) + " mt-4"}>
              Torna al login
            </Link>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
