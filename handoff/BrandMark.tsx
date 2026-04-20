// ================================================================
// src/components/shared/BrandMark.tsx
// Il simbolo "cursore-bolla" di weposteai.com come componente React.
// ================================================================
"use client";

interface BrandMarkProps {
  size?: number;
  strokeColor?: string;
  cursorColor?: string;
  /** Cursore lampeggiante. Usare solo in contesti "live" (AI attiva). */
  animated?: boolean;
  className?: string;
}

export function BrandMark({
  size = 28,
  strokeColor = "currentColor",
  cursorColor = "oklch(0.72 0.22 125)",
  animated = false,
  className,
}: BrandMarkProps) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 120 120"
      xmlns="http://www.w3.org/2000/svg"
      className={className}
      aria-hidden="true"
      role="img"
    >
      {/* Speech bubble outline */}
      <path
        d="M16 20 h88 a8 8 0 0 1 8 8 v52 a8 8 0 0 1 -8 8 h-42 l-18 18 v-18 h-28 a8 8 0 0 1 -8 -8 v-52 a8 8 0 0 1 8 -8 z"
        fill="none"
        stroke={strokeColor}
        strokeWidth={8}
        strokeLinejoin="round"
      />
      {/* Blinking cursor */}
      <rect x="56" y="34" width="8" height="40" fill={cursorColor}>
        {animated && (
          <animate
            attributeName="opacity"
            values="1;1;0;0"
            keyTimes="0;0.5;0.51;1"
            dur="1.1s"
            repeatCount="indefinite"
          />
        )}
      </rect>
    </svg>
  );
}
