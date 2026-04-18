"use client";

import { BrandMark } from "./BrandMark";
import { cn } from "@/lib/utils";

type LogoVariant = "primary" | "inverted" | "mark" | "wordmark" | "monogram";
type LogoSize = "sm" | "md" | "lg" | "xl";

interface LogoProps {
  variant?: LogoVariant;
  size?: LogoSize;
  className?: string;
  /** Blink del cursore nel mark. Default: false (solo in contesti "live"). */
  animated?: boolean;
}

const SIZES: Record<LogoSize, { mark: number; text: string; gap: string }> = {
  sm: { mark: 20, text: "text-lg", gap: "gap-1.5" },
  md: { mark: 28, text: "text-2xl", gap: "gap-2" },
  lg: { mark: 40, text: "text-4xl", gap: "gap-2.5" },
  xl: { mark: 56, text: "text-6xl", gap: "gap-3" },
};

export function Logo({
  variant = "primary",
  size = "md",
  className,
  animated = false,
}: LogoProps) {
  const sz = SIZES[size];
  const inverted = variant === "inverted";
  const inkColor = inverted ? "var(--color-paper)" : "var(--color-ink)";
  const mutedColor = inverted
    ? "color-mix(in oklch, var(--color-paper) 55%, transparent)"
    : "var(--color-muted-ink)";

  if (variant === "mark") {
    return (
      <BrandMark
        size={sz.mark}
        strokeColor={inkColor}
        cursorColor="var(--color-lime-deep)"
        animated={animated}
        className={className}
      />
    );
  }

  if (variant === "monogram") {
    return (
      <div
        className={cn(
          "inline-flex items-center justify-center rounded-md relative",
          className
        )}
        style={{
          width: sz.mark * 2.2,
          height: sz.mark * 2.2,
          border: `2px solid ${inverted ? "rgba(250,250,247,.2)" : "var(--color-line)"}`,
          background: inverted ? "var(--color-ink)" : "transparent",
        }}
        aria-label="weposteai.com"
      >
        <span
          className="font-serif leading-none"
          style={{
            fontSize: sz.mark * 1.6,
            letterSpacing: "-0.06em",
            color: inkColor,
          }}
        >
          w
          <em style={{ color: "var(--color-lime-deep)", fontStyle: "italic" }}>
            p
          </em>
        </span>
        <span
          className="absolute font-mono font-semibold"
          style={{
            bottom: -6,
            right: -6,
            background: "var(--color-lime)",
            color: "var(--color-ink)",
            fontSize: sz.mark * 0.36,
            padding: "2px 6px",
            borderRadius: 3,
          }}
        >
          .com
        </span>
      </div>
    );
  }

  if (variant === "wordmark") {
    return (
      <span
        className={cn("font-serif leading-none inline-flex items-baseline", className)}
        style={{ color: inkColor, letterSpacing: "-0.04em" }}
        aria-label="weposteai.com"
      >
        <span className={sz.text}>weposte</span>
        <em
          className={sz.text}
          style={{ color: mutedColor, fontStyle: "italic" }}
        >
          ai.com
        </em>
      </span>
    );
  }

  // primary + inverted
  return (
    <span
      className={cn("inline-flex items-center", sz.gap, className)}
      aria-label="weposteai.com"
    >
      <BrandMark
        size={sz.mark}
        strokeColor={inkColor}
        cursorColor={inverted ? "var(--color-lime)" : "var(--color-lime-deep)"}
        animated={animated}
      />
      <span
        className="font-serif leading-none inline-flex items-baseline"
        style={{ color: inkColor, letterSpacing: "-0.04em" }}
      >
        <span className={sz.text}>weposte</span>
        <em
          className={sz.text}
          style={{ color: mutedColor, fontStyle: "italic" }}
        >
          ai.com
        </em>
      </span>
    </span>
  );
}
