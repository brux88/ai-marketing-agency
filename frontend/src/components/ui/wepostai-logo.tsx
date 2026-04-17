export function WePostAILogo({ className = "size-6" }: { className?: string }) {
  return (
    <svg
      viewBox="0 0 64 64"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      className={className}
    >
      <defs>
        <linearGradient id="wg" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" stopColor="#6366f1" />
          <stop offset="100%" stopColor="#8b5cf6" />
        </linearGradient>
      </defs>
      <rect width="64" height="64" rx="14" fill="url(#wg)" />
      {/* Send/paper plane arrow */}
      <path
        d="M16 34 L48 18 L36 48 L30 36 Z"
        fill="white"
        opacity="0.95"
      />
      <path
        d="M30 36 L48 18"
        stroke="white"
        strokeWidth="2"
        opacity="0.5"
      />
      {/* Small sparkle dot for AI */}
      <circle cx="22" cy="22" r="3" fill="white" opacity="0.6" />
      <circle cx="18" cy="28" r="1.5" fill="white" opacity="0.4" />
    </svg>
  );
}
