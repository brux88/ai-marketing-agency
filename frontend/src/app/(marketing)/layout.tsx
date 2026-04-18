import Link from "next/link";
import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { Logo } from "@/components/shared/Logo";

export default function MarketingLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-screen flex flex-col bg-background">
      <nav className="sticky top-0 z-50 border-b bg-background/80 backdrop-blur-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
          <Link href="/" className="flex items-center">
            <Logo variant="primary" size="sm" />
          </Link>
          <div className="flex items-center gap-3">
            <Link href="/guide" className={cn(buttonVariants({ variant: "ghost", size: "sm" }))}>
              Guida
            </Link>
            <Link href="/login" className={cn(buttonVariants({ variant: "ghost", size: "sm" }))}>
              Accedi
            </Link>
            <Link href="/register" className={cn(buttonVariants({ size: "sm" }))}>
              Prova gratis
            </Link>
          </div>
        </div>
      </nav>
      <main className="flex-1">{children}</main>
      <footer className="border-t py-8">
        <div className="max-w-7xl mx-auto px-4 text-center text-muted-foreground text-sm">
          &copy; 2026 weposteai.com &mdash; Powered by AI Agents
        </div>
      </footer>
    </div>
  );
}
