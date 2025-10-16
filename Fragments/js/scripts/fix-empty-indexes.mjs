#!/usr/bin/env node
import { promises as fs } from 'node:fs';
import path from 'node:path';

const rawDir = path.dirname(new URL(import.meta.url).pathname);
const dir = process.platform === 'win32' && rawDir.startsWith('/') ? rawDir.slice(1) : rawDir;
const root = path.resolve(path.join(dir, '..'));
const genDirs = [
  path.join(root, 'ts-gen', 'schemas'),
  path.join(root, 'ts-gen', 'gen'),
  path.join(root, 'ts-gen'),
];

async function fileExists(p) {
  try { await fs.access(p); return true; } catch { return false; }
}

async function walk(dir, acc = []) {
  const entries = await fs.readdir(dir, { withFileTypes: true });
  for (const e of entries) {
    const full = path.join(dir, e.name);
    if (e.isDirectory()) acc.push(...await walk(full));
    else if (e.isFile() && e.name === 'index.ts') acc.push(full);
  }
  return acc;
}

function isEmptyIndex(content) {
  const trimmed = content.replace(/\r\n/g, '\n').trim();
  if (!trimmed) return true;
  // Treat comment-only files as empty
  const noComments = trimmed.replace(/^\/\/.*$/gm, '').trim();
  if (!noComments) return true;
  // If it has no export/import keywords, it's effectively empty for TS module purposes
  return !/\bexport\b|\bimport\b/.test(noComments);
}

let fixed = 0;
for (const base of genDirs) {
  if (!(await fileExists(base))) continue;
  const indexes = await walk(base);
  for (const idx of indexes) {
    const content = await fs.readFile(idx, 'utf8');
    if (isEmptyIndex(content)) {
      const header = content.endsWith('\n') ? content : content + '\n';
      await fs.writeFile(idx, header + 'export {};\n', 'utf8');
      fixed++;
    }
  }
}
console.log(`fix-empty-indexes: ensured module syntax in ${fixed} index.ts files`);
