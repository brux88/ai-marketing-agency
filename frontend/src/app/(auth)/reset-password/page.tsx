"use client";

import { Suspense, useState } from "react";
import { useSearchParams } from "next/navigation";
import Link from "next/link";
import { authApi } from "@/lib/api/auth.api";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Button, buttonVariants } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Lock, Loader2, CheckCircle2, XCircle } from "lucide-react";

export default function ResetPasswordPage() {
  return (
    <Suspense fallback={<Card><CardContent className="py-10 text-center"><Loader2 className="size-8 animate-spin mx-auto" /></CardContent></Card>}>
      <ResetPasswordContent />
    </Suspense>
  );
}

function ResetPasswordContent() {
  const searchParams = useSearchParams();
  const token = searchParams.get("token");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState("");

  if (!token) {
    return (
      <Card>
        <CardContent className="text-center py-10 space-y-4">
          <XCircle className="size-12 text-destructive mx-auto" />
          <p className="text-lg font-medium">Link non valido</p>
          <p className="text-muted-foreground">Il link di reset password non è valido o è scaduto.</p>
          <Link href="/forgot-password" className={buttonVariants({ variant: "outline" })}>
            Richiedi un nuovo link
          </Link>
        </CardContent>
      </Card>
    );
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    if (password.length < 6) {
      setError("La password deve essere di almeno 6 caratteri.");
      return;
    }
    if (password !== confirmPassword) {
      setError("Le password non corrispondono.");
      return;
    }

    setLoading(true);
    try {
      await authApi.resetPassword(token, password);
      setSuccess(true);
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error;
      setError(msg || "Si è verificato un errore. Il link potrebbe essere scaduto.");
    } finally {
      setLoading(false);
    }
  };

  if (success) {
    return (
      <Card>
        <CardHeader className="text-center">
          <CardTitle className="text-2xl">Password reimpostata</CardTitle>
        </CardHeader>
        <CardContent className="text-center space-y-4 py-6">
          <CheckCircle2 className="size-12 text-green-500 mx-auto" />
          <p className="text-muted-foreground">La tua password è stata aggiornata con successo.</p>
          <Link href="/login" className={buttonVariants({ variant: "default" }) + " mt-4"}>
            Accedi con la nuova password
          </Link>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader className="text-center">
        <CardTitle className="text-2xl">Nuova password</CardTitle>
        <CardDescription>Inserisci la tua nuova password</CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="space-y-4">
          {error && (
            <div className="bg-destructive/10 text-destructive text-sm p-3 rounded-lg">{error}</div>
          )}
          <div className="space-y-2">
            <Label htmlFor="password">Nuova password</Label>
            <Input
              id="password"
              type="password"
              required
              minLength={6}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Minimo 6 caratteri"
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="confirmPassword">Conferma password</Label>
            <Input
              id="confirmPassword"
              type="password"
              required
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              placeholder="Ripeti la password"
            />
          </div>
          <Button type="submit" disabled={loading} className="w-full">
            {loading ? (
              <><Loader2 className="size-4 animate-spin" /> Salvataggio...</>
            ) : (
              <><Lock className="size-4" /> Reimposta password</>
            )}
          </Button>
        </form>
      </CardContent>
      <CardFooter className="justify-center">
        <Link href="/login" className="text-sm text-primary font-medium hover:underline">
          Torna al login
        </Link>
      </CardFooter>
    </Card>
  );
}
