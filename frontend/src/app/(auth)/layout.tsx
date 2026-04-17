import Link from "next/link";
import { WePostAILogo } from "@/components/ui/wepostai-logo";

export default function AuthLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-screen flex items-center justify-center bg-muted/30 p-4">
      <div className="w-full max-w-md space-y-6">
        <div className="text-center">
          <Link href="/" className="inline-flex items-center gap-2 text-2xl font-bold">
            <WePostAILogo className="size-9" />
            WePostAI
          </Link>
        </div>
        {children}
      </div>
    </div>
  );
}
