import Link from "next/link";
import { notFound } from "next/navigation";
import { docsPages, getDocNeighbors, getDocPage } from "@/lib/docs";

export function generateStaticParams() {
  return docsPages.map((page) => ({ slug: page.slug }));
}

export default async function DocPage({
  params,
}: {
  params: Promise<{ slug: string }>;
}) {
  const { slug } = await params;
  const page = getDocPage(slug);

  if (!page) {
    notFound();
  }

  const neighbors = getDocNeighbors(slug);

  return (
    <div className="docs-layout">
      <aside className="sidebar">
        <span className="section-label">MiniLang docs</span>
        <nav>
          {docsPages.map((doc) => (
            <Link key={doc.slug} href={`/docs/${doc.slug}`}>
              {doc.title}
            </Link>
          ))}
        </nav>
      </aside>

      <article>
        <div className="breadcrumbs">
          <Link href="/">Home</Link>
          <span>/</span>
          <span>{page.section}</span>
          <span>/</span>
          <span>{page.title}</span>
        </div>

        <div className="section-label" style={{ marginTop: 16 }}>
          {page.section}
        </div>

        <h1>{page.title}</h1>
        <p className="muted">{page.summary}</p>

        <h2>What It Is</h2>
        <p>{page.whatItIs}</p>

        <h2>Why It Exists</h2>
        <p>{page.whyItExists}</p>

        <h2>Syntax</h2>
        <pre>{page.syntax}</pre>

        <h2>Examples</h2>
        {page.examples.map((example) => (
          <section key={example.title} className="example-card">
            <strong>{example.title}</strong>
            <pre>{example.code}</pre>
            {example.result ? (
              <>
                <div className="example-result-label">Expected Result</div>
                <pre className="example-result">{example.result}</pre>
              </>
            ) : null}
          </section>
        ))}

        <h2>Pitfalls</h2>
        <p>{page.pitfalls}</p>

        <h2>Best Practices</h2>
        <p>{page.bestPractices}</p>

        <h2>Compiler View</h2>
        <p>{page.compilerView}</p>

        <div className="pager">
          {neighbors.previous ? (
            <Link href={`/docs/${neighbors.previous.slug}`}>
              Previous: {neighbors.previous.title}
            </Link>
          ) : (
            <span />
          )}

          {neighbors.next ? (
            <Link href={`/docs/${neighbors.next.slug}`}>
              Next: {neighbors.next.title}
            </Link>
          ) : (
            <span />
          )}
        </div>
      </article>
    </div>
  );
}