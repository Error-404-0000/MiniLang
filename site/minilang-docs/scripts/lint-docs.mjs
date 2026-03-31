import { docsPages } from "../src/lib/docs.js";

const requiredSections = ["whatItIs", "whyItExists", "syntax", "pitfalls", "bestPractices", "compilerView"];
const failures = [];

for (const page of docsPages) {
  for (const section of requiredSections) {
    if (!page[section] || String(page[section]).trim().length === 0) {
      failures.push(`${page.slug}: missing required section '${section}'`);
    }
  }

  if (!Array.isArray(page.examples) || page.examples.length < 2) {
    failures.push(`${page.slug}: every page must have at least two topic-matched examples`);
    continue;
  }

  for (const example of page.examples) {
    if (example.topic !== page.topic) {
      failures.push(`${page.slug}: example '${example.title}' is tagged '${example.topic}' instead of '${page.topic}'`);
    }

    if (!example.code || String(example.code).trim().length === 0) {
      failures.push(`${page.slug}: example '${example.title}' is missing code`);
    }

    if ("result" in example && String(example.result ?? "").trim().length === 0) {
      failures.push(`${page.slug}: example '${example.title}' has an empty result block`);
    }
  }
}

if (failures.length > 0) {
  console.error("MiniLang docs content lint failed:");
  for (const failure of failures) {
    console.error(`- ${failure}`);
  }
  process.exit(1);
}

console.log(`MiniLang docs content lint passed for ${docsPages.length} pages.`);
