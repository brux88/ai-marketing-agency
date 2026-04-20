import { Suspense } from "react";
import UnsubscribeContent from "./unsubscribe-content";

export const dynamic = "force-dynamic";

export default function UnsubscribePage() {
  return (
    <Suspense fallback={<UnsubscribeFallback />}>
      <UnsubscribeContent />
    </Suspense>
  );
}

function UnsubscribeFallback() {
  return (
    <div className="max-w-xl mx-auto px-4 py-16 sm:py-24">
      <div className="rounded-2xl border bg-card p-8 sm:p-10 shadow-sm">
        <h1 className="text-2xl font-semibold">Disiscrizione in corso…</h1>
        <p className="mt-3 text-muted-foreground">
          Stiamo elaborando la tua richiesta.
        </p>
      </div>
    </div>
  );
}
