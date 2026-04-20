"use client";

import { useEffect, useState } from "react";
import { useSearchParams } from "next/navigation";
import Link from "next/link";
import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

type Status = "loading" | "success" | "missing" | "error";

export default function UnsubscribePage() {
  const searchParams = useSearchParams();
  const token = searchParams.get("token");
  const [status, setStatus] = useState<Status>("loading");
  const [message, setMessage] = useState<string | null>(null);

  useEffect(() => {
    if (!token) {
      setStatus("missing");
      return;
    }

    const url = `${API_URL}/api/v1/public/newsletter/unsubscribe-token?token=${encodeURIComponent(token)}`;
    fetch(url, { method: "GET" })
      .then(async (res) => {
        if (!res.ok) {
          const text = await res.text();
          throw new Error(text || `HTTP ${res.status}`);
        }
        setStatus("success");
      })
      .catch((err: Error) => {
        setStatus("error");
        setMessage(err.message);
      });
  }, [token]);

  return (
    <div className="max-w-xl mx-auto px-4 py-16 sm:py-24">
      <div className="rounded-2xl border bg-card p-8 sm:p-10 shadow-sm">
        {status === "loading" && (
          <>
            <h1 className="text-2xl font-semibold">Disiscrizione in corso…</h1>
            <p className="mt-3 text-muted-foreground">
              Stiamo elaborando la tua richiesta.
            </p>
          </>
        )}
        {status === "missing" && (
          <>
            <h1 className="text-2xl font-semibold">Link non valido</h1>
            <p className="mt-3 text-muted-foreground">
              Il link non contiene un token di disiscrizione. Controlla di aver
              aperto il link corretto ricevuto via email.
            </p>
          </>
        )}
        {status === "success" && (
          <>
            <h1 className="text-2xl font-semibold">Disiscrizione completata</h1>
            <p className="mt-3 text-muted-foreground">
              Non riceverai più email dalla newsletter. Se cambi idea puoi
              sempre iscriverti di nuovo dalla home.
            </p>
          </>
        )}
        {status === "error" && (
          <>
            <h1 className="text-2xl font-semibold">Impossibile disiscriversi</h1>
            <p className="mt-3 text-muted-foreground">
              Il link potrebbe essere scaduto o già usato.
              {message ? ` (${message})` : null}
            </p>
          </>
        )}
        <div className="mt-8">
          <Link
            href="/"
            className={cn(buttonVariants({ variant: "outline", size: "sm" }))}
          >
            Torna alla home
          </Link>
        </div>
      </div>
    </div>
  );
}
