"use client";

import Link from "next/link";
import { useMemo, useState } from "react";
import { docsPages } from "@/lib/docs";

export function SearchPanel() {
  const [query, setQuery] = useState("");

  const results = useMemo(() => {
    const lowered = query.trim().toLowerCase();
    if (!lowered) {
      return docsPages.slice(0, 6);
    }

    return docsPages.filter((page) =>
      `${page.title} ${page.summary} ${page.section} ${page.topic}`.toLowerCase().includes(lowered),
    );
  }, [query]);

  return (
    <section className="search-panel">
      <label className="search-label" htmlFor="docs-search">
        Search the official docs
      </label>
      <input
        id="docs-search"
        className="search-input"
        value={query}
        onChange={(event) => setQuery(event.target.value)}
        placeholder="Search modules, attributes, interop, IDE, CLI..."
      />
      <div className="search-results">
        {results.map((page) => (
          <Link key={page.slug} href={`/docs/${page.slug}`} className="search-result">
            <span>{page.title}</span>
            <small>{page.section}</small>
          </Link>
        ))}
      </div>
    </section>
  );
}
