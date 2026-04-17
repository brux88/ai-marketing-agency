"use client";

import { useCallback, useEffect, useState } from "react";
import Image from "next/image";
import { X, ChevronLeft, ChevronRight } from "lucide-react";

interface LightboxProps {
  images: { src: string; alt: string }[];
}

export function LightboxImage({ src, alt, className }: { src: string; alt: string; className?: string }) {
  const [open, setOpen] = useState(false);

  useEffect(() => {
    if (!open) return;
    const handler = (e: KeyboardEvent) => {
      if (e.key === "Escape") setOpen(false);
    };
    document.addEventListener("keydown", handler);
    return () => document.removeEventListener("keydown", handler);
  }, [open]);

  useEffect(() => {
    if (open) {
      document.body.style.overflow = "hidden";
    } else {
      document.body.style.overflow = "";
    }
    return () => { document.body.style.overflow = ""; };
  }, [open]);

  return (
    <>
      <Image
        src={src}
        alt={alt}
        width={800}
        height={450}
        className={`${className || ""} cursor-zoom-in`}
        onClick={() => setOpen(true)}
      />
      {open && (
        <div
          className="fixed inset-0 z-[100] flex items-center justify-center bg-black/80 backdrop-blur-sm"
          onClick={() => setOpen(false)}
        >
          <button
            className="absolute top-4 right-4 text-white/80 hover:text-white p-2 rounded-full bg-black/40 hover:bg-black/60 transition-colors"
            onClick={() => setOpen(false)}
          >
            <X className="size-6" />
          </button>
          <div className="max-w-[90vw] max-h-[90vh]" onClick={(e) => e.stopPropagation()}>
            <Image
              src={src}
              alt={alt}
              width={1600}
              height={900}
              className="w-auto h-auto max-w-[90vw] max-h-[90vh] rounded-lg shadow-2xl"
              quality={95}
            />
          </div>
        </div>
      )}
    </>
  );
}

export function LightboxGallery({ images }: LightboxProps) {
  const [openIdx, setOpenIdx] = useState<number | null>(null);

  const close = useCallback(() => setOpenIdx(null), []);
  const prev = useCallback(() => setOpenIdx((i) => (i !== null && i > 0 ? i - 1 : i)), []);
  const next = useCallback(() => setOpenIdx((i) => (i !== null && i < images.length - 1 ? i + 1 : i)), [images.length]);

  useEffect(() => {
    if (openIdx === null) return;
    const handler = (e: KeyboardEvent) => {
      if (e.key === "Escape") close();
      if (e.key === "ArrowLeft") prev();
      if (e.key === "ArrowRight") next();
    };
    document.addEventListener("keydown", handler);
    return () => document.removeEventListener("keydown", handler);
  }, [openIdx, close, prev, next]);

  useEffect(() => {
    if (openIdx !== null) {
      document.body.style.overflow = "hidden";
    } else {
      document.body.style.overflow = "";
    }
    return () => { document.body.style.overflow = ""; };
  }, [openIdx]);

  return (
    <>
      <div className="grid grid-cols-2 gap-3">
        {images.map((img, i) => (
          <div
            key={img.src}
            className="rounded-lg border overflow-hidden cursor-zoom-in hover:ring-2 hover:ring-primary/50 transition-all"
            onClick={() => setOpenIdx(i)}
          >
            <Image src={img.src} alt={img.alt} width={400} height={225} className="w-full h-auto" />
          </div>
        ))}
      </div>
      {openIdx !== null && (
        <div
          className="fixed inset-0 z-[100] flex items-center justify-center bg-black/80 backdrop-blur-sm"
          onClick={close}
        >
          <button
            className="absolute top-4 right-4 text-white/80 hover:text-white p-2 rounded-full bg-black/40 hover:bg-black/60 transition-colors"
            onClick={close}
          >
            <X className="size-6" />
          </button>
          {openIdx > 0 && (
            <button
              className="absolute left-4 text-white/80 hover:text-white p-2 rounded-full bg-black/40 hover:bg-black/60 transition-colors"
              onClick={(e) => { e.stopPropagation(); prev(); }}
            >
              <ChevronLeft className="size-6" />
            </button>
          )}
          {openIdx < images.length - 1 && (
            <button
              className="absolute right-4 text-white/80 hover:text-white p-2 rounded-full bg-black/40 hover:bg-black/60 transition-colors"
              onClick={(e) => { e.stopPropagation(); next(); }}
            >
              <ChevronRight className="size-6" />
            </button>
          )}
          <div className="max-w-[90vw] max-h-[90vh] text-center" onClick={(e) => e.stopPropagation()}>
            <Image
              src={images[openIdx].src}
              alt={images[openIdx].alt}
              width={1600}
              height={900}
              className="w-auto h-auto max-w-[90vw] max-h-[85vh] rounded-lg shadow-2xl"
              quality={95}
            />
            <p className="text-white/70 text-sm mt-3">{images[openIdx].alt} ({openIdx + 1}/{images.length})</p>
          </div>
        </div>
      )}
    </>
  );
}
