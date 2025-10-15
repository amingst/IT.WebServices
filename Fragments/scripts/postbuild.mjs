#!/usr/bin/env node
import { promises as fs } from 'node:fs';
import path from 'node:path';

const rawDir = path.dirname(new URL(import.meta.url).pathname);
const dir = process.platform === 'win32' && rawDir.startsWith('/') ? rawDir.slice(1) : rawDir;
const root = path.resolve(path.join(dir, '..'));
const esmDir = path.join(root, 'dist', 'esm');
const cjsDir = path.join(root, 'dist', 'cjs');

async function ensure(dir) {
  await fs.mkdir(dir, { recursive: true });
}

async function writeJSON(file, obj) {
  await fs.writeFile(file, JSON.stringify(obj, null, 2), 'utf8');
}

await ensure(esmDir);
await ensure(cjsDir);

await writeJSON(path.join(esmDir, 'package.json'), { type: 'module' });
await writeJSON(path.join(cjsDir, 'package.json'), { type: 'commonjs' });

console.log('Wrote module-type package.json files to dist/esm and dist/cjs');
