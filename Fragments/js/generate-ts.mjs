#!/usr/bin/env node

import { spawnSync } from 'node:child_process';
import { promises as fsp } from 'node:fs';
import fs from 'node:fs';
import path from 'node:path';

const scriptDir = path.dirname(new URL(import.meta.url).pathname).replace(/^\//, '');
const cwd = scriptDir;
const nodeBin = path.join(cwd, 'node_modules', '.bin');
const bufBin = path.join(nodeBin, process.platform === 'win32' ? 'buf.cmd' : 'buf');
const contextCwd = path.join(cwd, '..');

function log(msg) { console.log(msg); }

async function ensureDir(dir) { await fsp.mkdir(dir, { recursive: true }); }
async function rimrafSafe(target) { await fsp.rm(target, { recursive: true, force: true }).catch(() => {}); }

function runBufGenerate(relPath) {
  const currentPath = process.env.PATH || process.env.Path || '';
  const newPath = `${nodeBin}${path.delimiter}${currentPath}`;
  const env = { ...process.env, PATH: newPath, Path: newPath };
  const template = path.join(cwd, 'buf.gen.yaml');
  const cmd = process.platform === 'win32'
    ? `"${bufBin}" generate --template "${template}" --path "${relPath.replaceAll('\\','/')}"`
    : `${bufBin} generate --template ${template} --path ${relPath}`;
  const res = spawnSync(cmd, { cwd: contextCwd, env, stdio: 'inherit', shell: true });
  return res.status === 0;
}

async function discoverModules() {
  const base = path.join(contextCwd, 'Protos', 'IT', 'WebServices', 'Fragments');
  const modules = [];
  const entries = await fsp.readdir(base, { withFileTypes: true }).catch(() => []);
  for (const e of entries) {
    if (!e.isDirectory()) continue;
    const moduleDir = path.join(base, e.name);
    const files = await fsp.readdir(moduleDir).catch(() => []);
    if (files.some(f => f.toLowerCase().endsWith('.proto'))) modules.push(e.name);
  }
  const rootHasProto = (await fsp.readdir(base).catch(() => [])).some(f => f.toLowerCase().endsWith('.proto'));
  return { modules, rootHasProto };
}

async function generateIndexes() {
  const genRoot = path.join(cwd, 'ts-gen', 'gen');
  const allDirs = new Set();
  function collect(d) {
    if (!fs.existsSync(d)) return;
    allDirs.add(d);
    for (const ent of fs.readdirSync(d, { withFileTypes: true })) {
      const full = path.join(d, ent.name);
      if (ent.isDirectory()) collect(full);
    }
  }
  collect(genRoot);

  async function generateIndexFor(dir) {
    const entries = fs.readdirSync(dir, { withFileTypes: true });
    const files = entries.filter(e => e.isFile() && e.name.endsWith('.ts')).map(e => e.name).sort();
    const subdirs = entries.filter(e => e.isDirectory()).map(e => e.name).sort();
    let idx = '';
    idx += `// Auto-generated - DO NOT EDIT\n`;
    for (const f of files) {
      const base = f.replace(/\.ts$/, '');
      if (base === 'index') continue;
      idx += `export * from './${base}';\n`;
    }
    for (const sd of subdirs) idx += `export * as ${sd} from './${sd}';\n`;
    await fsp.writeFile(path.join(dir, 'index.ts'), idx, 'utf8');
  }

  for (const dir of Array.from(allDirs).sort((a, b) => b.length - a.length)) {
    const entries = fs.existsSync(dir) ? fs.readdirSync(dir, { withFileTypes: true }) : [];
    const hasIndex = entries.some(e => e.isFile() && e.name === 'index.ts');
    const hasAnyTs = entries.some(e => e.isFile() && e.name.endsWith('.ts'));
    const hasSubTs = entries.some(e => e.isDirectory());
    if (!hasIndex && (hasAnyTs || hasSubTs)) await generateIndexFor(dir);
  }

  // Main index in ts-gen
  const mainIndex = path.join(cwd, 'ts-gen', 'index.ts');
  const mainContent = `// Auto-generated main index file - DO NOT EDIT MANUALLY\n// This file provides access to all generated protobuf definitions\n// Generated on: ${new Date().toString()}\n\nexport * from './gen/Protos';\n`;
  await fsp.writeFile(mainIndex, mainContent, 'utf8');

  // Subpath entry: protos -> re-export protobuf-es/connect-es outputs
  const protosDir = path.join(cwd, 'ts-gen', 'protos');
  await ensureDir(protosDir);
  const protosIndex = path.join(protosDir, 'index.ts');
  const protosContent = `// Auto-generated - DO NOT EDIT\n// Re-exports protobuf-es/connect-es generated types\n// Generated on: ${new Date().toString()}\n\nexport * from '../gen/Protos/IT/WebServices/Fragments';\n`;
  await fsp.writeFile(protosIndex, protosContent, 'utf8');
}

async function buildFlatShims() {
  const deepRoot = path.join(cwd, 'ts-gen', 'gen', 'Protos', 'IT', 'WebServices', 'Fragments');
  const flatRoot = path.join(cwd, 'ts-gen');
  if (!fs.existsSync(deepRoot)) return;

  function titleCase(name) { return name.replace(/[^A-Za-z0-9_]/g, ''); }

  function relDeep(from, to) {
    return path.relative(from, to).replace(/\\/g, '/');
  }

  function shimDir(deepDir, relUnderModule, outDir) {
    const entries = fs.readdirSync(deepDir, { withFileTypes: true });
    let idx = `// Auto-generated - DO NOT EDIT\n`;
    for (const ent of entries) {
      const full = path.join(deepDir, ent.name);
      if (ent.isFile() && ent.name.endsWith('.ts')) {
        const base = ent.name.replace(/\.ts$/, '');
        if (base === 'index') continue;
        const shimPath = path.join(outDir, `${base}.ts`);
        const importFrom = relDeep(path.dirname(shimPath), full).replace(/\.ts$/, '');
        fs.mkdirSync(path.dirname(shimPath), { recursive: true });
        fs.writeFileSync(shimPath, `// Auto-generated - DO NOT EDIT\nexport * from '${importFrom}';\n`);
        idx += `export * from './${base}';\n`;
      }
    }
    for (const ent of entries) {
      if (ent.isDirectory()) {
        const subOut = path.join(outDir, ent.name);
        const subDeep = path.join(deepDir, ent.name);
        fs.mkdirSync(subOut, { recursive: true });
        fs.writeFileSync(path.join(subOut, 'index.ts'), `// Auto-generated - DO NOT EDIT\n`);
        shimDir(subDeep, path.join(relUnderModule, ent.name), subOut);
        idx += `export * as ${titleCase(ent.name)} from './${ent.name}';\n`;
      }
    }
    fs.mkdirSync(outDir, { recursive: true });
    fs.writeFileSync(path.join(outDir, 'index.ts'), idx);
  }

  for (const mod of fs.readdirSync(deepRoot, { withFileTypes: true })) {
    if (!mod.isDirectory()) continue;
    const deepModDir = path.join(deepRoot, mod.name);
    const outModDir = path.join(flatRoot, mod.name);
    shimDir(deepModDir, mod.name, outModDir);
  }
}

async function buildProtosFlatShims() {
  const deepRoot = path.join(cwd, 'ts-gen', 'gen', 'Protos', 'IT', 'WebServices', 'Fragments');
  const outRoot = path.join(cwd, 'ts-gen', 'protos');
  if (!fs.existsSync(deepRoot)) return;

  function rel(from, to) { return path.relative(from, to).replace(/\\/g, '/'); }

  // Ensure base index that re-exports whole tree
  await ensureDir(outRoot);
  const baseIdx = `// Auto-generated - DO NOT EDIT\nexport * from '../gen/Protos/IT/WebServices/Fragments';\n`;
  await fsp.writeFile(path.join(outRoot, 'index.ts'), baseIdx, 'utf8');

  for (const mod of fs.readdirSync(deepRoot, { withFileTypes: true })) {
    if (!mod.isDirectory()) continue;
    const modName = mod.name;
    const deepModDir = path.join(deepRoot, modName);
    const outModDir = path.join(outRoot, modName);
    await ensureDir(outModDir);
    // Walk files and make shims
    const entries = fs.readdirSync(deepModDir, { withFileTypes: true });
    let idx = `// Auto-generated - DO NOT EDIT\n`;
    for (const ent of entries) {
      const full = path.join(deepModDir, ent.name);
      if (ent.isFile() && ent.name.endsWith('.ts')) {
        const base = ent.name.replace(/\.ts$/, '');
        if (base === 'index') continue;
        const shim = path.join(outModDir, `${base}.ts`);
        const importFrom = rel(path.dirname(shim), full).replace(/\.ts$/, '');
        await fsp.writeFile(shim, `// Auto-generated - DO NOT EDIT\nexport * from '${importFrom}';\n`, 'utf8');
        idx += `export * from './${base}';\n`;
      }
    }
    await fsp.writeFile(path.join(outModDir, 'index.ts'), idx, 'utf8');
  }
}

async function main() {
  log('Starting TypeScript generation for all proto files...');
  log(`Working directory: ${cwd}`);
  log(`Using buf at: ${bufBin}`);

  const tsGenDir = path.join(cwd, 'ts-gen');
  await ensureDir(tsGenDir);
  // Clean except _meta if present
  for (const e of await fsp.readdir(tsGenDir, { withFileTypes: true })) {
    if (e.name === '_meta') continue;
    await rimrafSafe(path.join(tsGenDir, e.name));
  }
  await ensureDir(path.join(tsGenDir, 'gen'));
  log('Cleaned ts-gen directory.');

  const { modules, rootHasProto } = await discoverModules();
  log(`Discovered modules: ${modules.join(', ') || '(none)'}`);

  const successful = [];
  const failed = [];
  for (const mod of modules) {
    log(`Generating: ${mod}`);
    const ok = runBufGenerate(path.join('Protos', 'IT', 'WebServices', 'Fragments', mod));
    if (ok) successful.push(mod); else failed.push(mod);
  }
  if (rootHasProto) {
    log('Generating: root-level proto files');
    runBufGenerate(path.join('Protos', 'IT', 'WebServices', 'Fragments'));
  }

  log('Building hierarchical index.ts files...');
  await generateIndexes();
  log('Index build complete.');

  // Build flat shims at ts-gen/<Module> re-exporting deep tree
  log('Building flat re-export shims...');
  await buildFlatShims();
  log('Flat shims complete.');
  log('Building protos flat shims...');
  await buildProtosFlatShims();
  log('Protos flat shims complete.');
}

main().catch((err) => { console.error('Generation failed with error:', err); process.exit(1); });
