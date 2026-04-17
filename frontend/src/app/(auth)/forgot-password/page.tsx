"use client";

import { useState } from "react";
import Link from "next/link";
import { authApi } from "@/lib/api/auth.api";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Mail, Loader2, CheckCircle2 } from "lucide-react";

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [loading, setLoading] = useState(false);
  const [sent, setSent] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      await authApi.forgotPassword(email);
      setSent(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Si è verificato un errore.");
    } finally {
      setLoading(false);
    }
  };

  if (sent) {
    return (
      <Card>
        <CardHeader className="text-center">
          <CardTitle className="text-2xl">Controlla la tua email</CardTitle>
        </CardHeader>
        <CardContent className="text-center space-y-4 py-6">
          <CheckCircle2 className="size-12 text-green-500 mx-auto" />
          <p className="text-muted-foreground">
            Se l&apos;indirizzo <strong>{email}</strong> è associato a un account, riceverai un link per reimpostare la password.
          </p>
        </CardContent>
        <CardFooter className="justify-center">
          <Link href="/login" className="text-sm text-primary font-medium hover:underline">
            Torna al login
          </Link>
        </CardFooter>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader className="text-center">
        <CardTitle className="text-2xl">Password dimenticata?</CardTitle>
        <CardDescription>Inserisci la tua email per ricevere un link di reset</CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="space-y-4">
          {error && (
            <div className="bg-destructive/10 text-destructive text-sm p-3 rounded-lg">{error}</div>
          )}
          <div className="space-y-2">
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="tu@esempio.com"
            />
          </div>
          <Button type="submit" disabled={loading} className="w-full">
            {loading ? (
              <><Loader2 className="size-4 animate-spin" /> Invio in corso...</>
            ) : (
              <><Mail className="size-4" /> Invia link di reset</>
            )}
          </Button>
        </form>
      </CardContent>
      <CardFooter className="justify-center">
        <p className="text-sm text-muted-foreground">
          Ricordi la password?{" "}
          <Link href="/login" className="text-primary font-medium hover:underline">
            Accedi
          </Link>
        </p>
      </CardFooter>
    </Card>
  );
}
