"use client";

import { LightboxImage } from "@/components/ui/lightbox";

export function GuideScreenshots({ src, alt }: { src: string; alt: string }) {
  return (
    <div className="rounded-xl border shadow-lg overflow-hidden bg-muted/50">
      <div className="flex items-center gap-1.5 px-4 py-2 border-b bg-muted/80">
        <span className="size-3 rounded-full bg-red-400" />
        <span className="size-3 rounded-full bg-yellow-400" />
        <span className="size-3 rounded-full bg-green-400" />
        <span className="ml-2 text-xs text-muted-foreground">wepostai.com</span>
      </div>
      <LightboxImage src={src} alt={alt} className="w-full h-auto" />
    </div>
  );
}
