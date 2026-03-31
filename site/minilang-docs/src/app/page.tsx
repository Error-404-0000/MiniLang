import Link from "next/link";
import { SearchPanel } from "@/components/SearchPanel";
import { docsPages } from "@/lib/docs";

export default function HomePage() {
  return (
    <>
      <section className="hero">
        <div className="hero-panel">
          <span className="section-label">Serious language ecosystem</span>
          <h1>MiniLang is now documenting the real legacy runtime, not an imagined future shell.</h1>
          <p>
            The active platform combines the legacy MiniLang runtime, Monaco-based MiniLang Studio, reusable workspace
            libraries, a safe Windows interop bridge, new collection features like arrays and <code>foreach</code>,
            and an official docs site built around topic-accurate examples with expected results.
          </p>
          <div className="feature-grid">
            <div className="feature-card">
              <strong>Language</strong>
              <p className="muted">Legacy MiniLang runtime with enums, arrays, foreach, indexing, and reusable functions.</p>
            </div>
            <div className="feature-card">
              <strong>Studio</strong>
              <p className="muted">WinUI host, Monaco editor, startup files, diagnostics, Release console runs, and structured inspectors.</p>
            </div>
            <div className="feature-card">
              <strong>Interop</strong>
              <p className="muted">Safe user-mode process, time, console, IO, and user helpers exposed through the `win` bridge.</p>
            </div>
            <div className="feature-card">
              <strong>Reusable Workspace</strong>
              <p className="muted">Core, Console, IO, Window, and Collections libraries plus runnable startup apps under a real folder layout.</p>
            </div>
          </div>
          <p>
            <Link href="/docs/getting-started" className="section-label">
              Open the docs
            </Link>
          </p>
        </div>
        <SearchPanel />
      </section>

      <section className="hero-panel" style={{ marginTop: 24 }}>
        <span className="section-label">Documentation map</span>
        <div className="feature-grid">
          {docsPages.map((page) => (
            <Link key={page.slug} href={`/docs/${page.slug}`} className="feature-card">
              <strong>{page.title}</strong>
              <p className="muted">{page.summary}</p>
            </Link>
          ))}
        </div>
      </section>
    </>
  );
}
