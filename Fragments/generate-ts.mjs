#!/usr/bin/env node

import { spawnSync } from 'node:child_process';
import { promises as fsp } from 'node:fs';
import fs from 'node:fs';
import path from 'node:path';

const scriptDir = path.dirname(new URL(import.meta.url).pathname).replace(/^\//, '');
const cwd = scriptDir;                              // Fragments/
const nodeBin = path.join(cwd, 'node_modules', '.bin');
const bufBin = path.join(nodeBin, process.platform === 'win32' ? 'buf.cmd' : 'buf');
const contextCwd = cwd;

function log(msg) { console.log(msg); }
function warn(msg) { console.warn(msg); }
function err(msg) { console.error(msg); }

async function ensureDir(dir) { await fsp.mkdir(dir, { recursive: true }); }
async function rimrafSafe(target) { await fsp.rm(target, { recursive: true, force: true }).catch(() => {}); }

/**
 * Run buf generate once. Your buf.gen.v2.yaml must include all inputs you need
 * (your tree + googleapis + protovalidate).
 */
function runBufGenerateOnce() {
  const env = {
    ...process.env,
    PATH: `${nodeBin}${path.delimiter}${process.env.PATH || process.env.Path || ''}`,
    Path: `${nodeBin}${path.delimiter}${process.env.PATH || process.env.Path || ''}`,
  };
  const template = path.join(cwd, 'buf.gen.v2.yaml');

  // Build a single command string (works reliably on Windows/MINGW)
  const quotedBuf = process.platform === 'win32' ? `"${bufBin}"` : bufBin;
  const quotedTpl = process.platform === 'win32' ? `"${template}"` : template;
  const cmd = `${quotedBuf} generate --template ${quotedTpl}`;

  log(`> buf command: ${cmd}`);
  const res = spawnSync(cmd, [], {
    cwd: contextCwd,
    env,
    shell: true,
    encoding: 'utf8',
  });

  if (res.error) {
    err('buf spawn error: ' + (res.error?.stack || res.error?.message || res.error));
    return false;
  }
  if (res.status !== 0) {
    err(`buf exited with non-zero status: ${res.status}`);
    if (res.stdout?.trim()) warn('--- buf stdout ---\n' + res.stdout.trim());
    if (res.stderr?.trim()) err('--- buf stderr ---\n' + res.stderr.trim());
    return false;
  }
  if (res.stdout?.trim()) log(res.stdout.trim());
  return true;
}

/**
 * Shim protovalidate import path:
 * Generated files under ts-gen/gen/Protos/... import "../../../../buf/validate/validate_pb",
 * but the actual file from the protovalidate module lands at ts-gen/gen/buf/validate/validate_pb.ts.
 * This creates a tiny re-export so both paths work.
 */
async function fixProtovalidateImportPath() {
  const genRoot = path.join(cwd, 'ts-gen', 'gen');

  // Actual generated file (from buf.build/bufbuild/protovalidate)
  const src = path.join(genRoot, 'buf', 'validate', 'validate_pb.ts');

  // Where our generated code expects to find it (under Protos/...)
  const destDir = path.join(genRoot, 'Protos', 'buf', 'validate');
  const dest = path.join(destDir, 'validate_pb.ts');

  if (!fs.existsSync(src)) {
    warn('[protovalidate] Expected source not found: ' + src);
    warn('  Check that buf.gen.v2.yaml includes the protovalidate module under inputs.');
    return;
  }

  await ensureDir(destDir);

  // Relative path from the shim file (destDir) to the real file (src)
  const relToSrcDir = path.relative(destDir, path.dirname(src)).replace(/\\/g, '/');
  const shim = `// Auto-generated shim — DO NOT EDIT
// Map the real protovalidate file (ts-gen/gen/buf/validate/validate_pb.ts)
// to the path and symbol our generated code expects under Protos/...
export * from '${relToSrcDir}/validate_pb';
export { file_buf_validate_validate as file_Protos_buf_validate_validate } from '${relToSrcDir}/validate_pb';
`;
  await fsp.writeFile(dest, shim, 'utf8');


  // Optional: index.ts for that directory
  const idx = `// Auto-generated shim index — DO NOT EDIT
export * from './validate_pb';
`;
  await fsp.writeFile(path.join(destDir, 'index.ts'), idx, 'utf8');

  log('[protovalidate] Created shim:', dest, '→ re-exports', src);
}

async function generateIndexes() {
  const genRoot = path.join(cwd, 'ts-gen', 'gen');
  const allDirs = new Set();

  (function collect(d) {
    if (!fs.existsSync(d)) return;
    allDirs.add(d);
    for (const ent of fs.readdirSync(d, { withFileTypes: true })) {
      const full = path.join(d, ent.name);
      if (ent.isDirectory()) collect(full);
    }
  })(genRoot);

  function hasSuffix(name, suffix) {
    return name.toLowerCase().endsWith(suffix.toLowerCase());
  }

  function dedupeBySuffix(files, suffix = '_pb.ts') {
    const withSuffix = files.filter(f => hasSuffix(f, suffix));
    const withoutSuffix = files.filter(f => !hasSuffix(f, suffix));
    const keep = new Set(withoutSuffix);
    const suffixBucket = [...withSuffix].sort((a, b) => b.length - a.length); // longest first
    const keptTails = new Set();
    for (const f of suffixBucket) {
      const tail = f;
      if (keptTails.has(tail)) continue;
      const longerExists = Array.from(keep).some(k => k !== f && hasSuffix(k, suffix) && k.endsWith(tail));
      if (longerExists) continue;
      keep.add(f);
      keptTails.add(tail);
    }
    return Array.from(keep).sort();
  }

  // NEW: detect if a directory (recursively) has any *_connect.ts files
  function dirHasConnect(dir) {
    if (!fs.existsSync(dir)) return false;
    const entries = fs.readdirSync(dir, { withFileTypes: true });
    if (entries.some(e => e.isFile() && e.name.endsWith('_connect.ts'))) return true;
    for (const ent of entries) {
      if (ent.isDirectory() && dirHasConnect(path.join(dir, ent.name))) return true;
    }
    return false;
  }

  async function generateIndexFor(dir) {
    const entries = fs.readdirSync(dir, { withFileTypes: true });
    const subdirs = entries.filter(e => e.isDirectory()).map(e => e.name).sort();

    const tsFiles = entries
      .filter(e => e.isFile() && e.name.endsWith('.ts') && e.name !== 'index.ts' && e.name !== 'connect.ts')
      .map(e => e.name)
      .sort();

    const connectFiles = tsFiles.filter(n => n.endsWith('_connect.ts'));
    const rawPbFiles   = tsFiles.filter(n => !n.endsWith('_connect.ts'));

    const pbFiles = dedupeBySuffix(rawPbFiles, '_pb.ts');

    // ----- index.ts (PB + non-connect) -----
    {
      let idx = `// Auto-generated - DO NOT EDIT\n`;
      for (const f of pbFiles) {
        const base = f.replace(/\.ts$/, '');
        idx += `export * from './${base}';\n`;
      }
      for (const sd of subdirs) idx += `export * as ${sd.replace(/[^A-Za-z0-9_]/g,'')} from './${sd}';\n`;
      await fsp.writeFile(path.join(dir, 'index.ts'), idx, 'utf8');
    }

    // ----- connect.ts (ONLY if this dir or any subdir has *_connect.ts) -----
    const subdirsWithConnect = subdirs.filter(sd => dirHasConnect(path.join(dir, sd)));
    if (connectFiles.length || subdirsWithConnect.length) {
      let cidx = `// Auto-generated - DO NOT EDIT\n`;
      for (const f of connectFiles) {
        const base = f.replace(/\.ts$/, '');
        cidx += `export * from './${base}';\n`;
      }
      for (const sd of subdirsWithConnect) {
        cidx += `export * as ${sd.replace(/[^A-Za-z0-9_]/g,'')} from './${sd}/connect';\n`;
      }
      await fsp.writeFile(path.join(dir, 'connect.ts'), cidx, 'utf8');
    } else {
      // If an old connect.ts exists here from a previous run, remove it
      const cpath = path.join(dir, 'connect.ts');
      if (fs.existsSync(cpath)) await fsp.rm(cpath).catch(() => {});
    }
  }

  for (const dir of Array.from(allDirs).sort((a, b) => b.length - a.length)) {
    const entries = fs.existsSync(dir) ? fs.readdirSync(dir, { withFileTypes: true }) : [];
    const hasAnyTs = entries.some(e => e.isFile() && e.name.endsWith('.ts') && e.name !== 'index.ts' && e.name !== 'connect.ts');
    const hasSub   = entries.some(e => e.isDirectory());
    if (hasAnyTs || hasSub) await generateIndexFor(dir);
  }

  // main barrels remain as you had them...
  const mainIndex = path.join(cwd, 'ts-gen', 'index.ts');
  const mainContent =
`// Auto-generated main index file - DO NOT EDIT MANUALLY
// Generated on: ${new Date().toString()}

export * from './gen/Protos';
`;
  await fsp.writeFile(mainIndex, mainContent, 'utf8');

  const protosDir = path.join(cwd, 'ts-gen', 'protos');
  await ensureDir(protosDir);
  const protosIndex = path.join(protosDir, 'index.ts');
  const protosContent =
`// Auto-generated - DO NOT EDIT
export * from '../gen/Protos/IT/WebServices/Fragments';
`;
  await fsp.writeFile(protosIndex, protosContent, 'utf8');
}



async function buildFlatShims() {
  const deepRoot = path.join(cwd, 'ts-gen', 'gen', 'Protos', 'IT', 'WebServices', 'Fragments');
  const flatRoot = path.join(cwd, 'ts-gen');
  if (!fs.existsSync(deepRoot)) return;

  function titleCase(name) { return name.replace(/[^A-Za-z0-9_]/g, ''); }
  function relDeep(from, to) { return path.relative(from, to).replace(/\\/g, '/'); }

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

  await ensureDir(outRoot);
  const baseIdx = `// Auto-generated - DO NOT EDIT\nexport * from '../gen/Protos/IT/WebServices/Fragments';\n`;
  await fsp.writeFile(path.join(outRoot, 'index.ts'), baseIdx, 'utf8');

  for (const mod of fs.readdirSync(deepRoot, { withFileTypes: true })) {
    if (!mod.isDirectory()) continue;
    const deepModDir = path.join(deepRoot, mod.name);
    const outModDir = path.join(outRoot, mod.name);
    await ensureDir(outModDir);
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
  for (const e of await fsp.readdir(tsGenDir, { withFileTypes: true })) {
    if (e.name === '_meta') continue;
    await rimrafSafe(path.join(tsGenDir, e.name));
  }
  await ensureDir(path.join(tsGenDir, 'gen'));
  log('Cleaned ts-gen directory.');

  // RUN ONCE — let the template control inputs/paths.
  if (!runBufGenerateOnce()) process.exit(1);

  // ⬇️ Fix the remaining protovalidate import path mismatch
  await fixProtovalidateImportPath();

  log('Building hierarchical index.ts files...');
  await generateIndexes();
  log('Index build complete.');

  log('Building flat re-export shims...');
  await buildFlatShims();
  log('Flat shims complete.');

  log('Building protos flat shims...');
  await buildProtosFlatShims();
  log('Protos flat shims complete.');
}

main().catch((e) => { console.error('Generation failed with error:', e); process.exit(1); });
