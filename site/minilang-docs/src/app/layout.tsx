import type { Metadata } from "next";
import Link from "next/link";
import "./globals.css";

export const metadata: Metadata = {
  title: "MiniLang Docs",
  description: "Official docs, reference, interop, IDE, and compiler guidance for MiniLang."
};

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="en">
      <body>
        <div className="site-shell">
          <header className="topbar">
            <div className="brand">
              <span className="brand-mark" />
              <div>
                <strong>MiniLang</strong>
                <div className="muted">Official language, IDE, and tooling docs</div>
              </div>
            </div>
            <nav className="nav-links">
              <Link href="/">Home</Link>
              <Link href="/docs/getting-started">Getting Started</Link>
              <Link href="/docs/language-reference">Language Reference</Link>
              <Link href="/docs/interop">Interop</Link>
            </nav>
          </header>
          {children}
        </div>
      </body>
    </html>
  );
}
