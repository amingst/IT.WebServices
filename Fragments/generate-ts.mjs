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
  const genRoot = path.join(cwd, 'ts-gen');
  const allDirs = new Set();

  (function collect(d) {
    if (!fs.existsSync(d)) return;
    allDirs.add(d);
    for (const ent of fs.readdirSync(d, { withFileTypes: true })) {
      const full = path.join(d, ent.name);
      if (ent.isDirectory()) collect(full);
    }
  })(genRoot);

  // helpers
  const stripTs = (n) => n.replace(/\.ts$/, '');
  const baseOf = (n) => stripTs(n).replace(/_(pb|connect)$/, '');
  const hasSuffix = (n, s) => n.toLowerCase().endsWith(s.toLowerCase());

  function dedupeBySuffix(files, suffix = '_pb.ts') {
    // keep longest names first to bias toward more specific files
    const a = [...files].sort((x, y) => y.length - x.length);
    const kept = [];
    const seen = new Set();
    for (const f of a) {
      const key = f.toLowerCase();
      if (seen.has(key)) continue;
      kept.push(f);
      seen.add(key);
    }
    return kept.sort();
  }

  // Heuristic: if a generic "Backup_pb.ts" exists alongside any "*Backup_pb.ts",
  // drop the generic one to avoid re-exporting duplicate symbols.
  function filterGenericVsQualifiedPb(tsFiles) {
    const hasQualifiedBackup = tsFiles.some(n => /[A-Za-z0-9]+Backup_pb\.ts$/.test(n) && n !== 'Backup_pb.ts');
    return tsFiles.filter(n => {
      if (n === 'Backup_pb.ts' && hasQualifiedBackup) return false;
      return true;
    });
  }

  // returns true if dir or any subdir contains *_connect.ts
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
    const subdirs  = entries.filter(e => e.isDirectory()).map(e => e.name).sort();

    // only real TS files (not barrels) here
    const tsFilesAll = entries
      .filter(e => e.isFile() && e.name.endsWith('.ts') && e.name !== 'index.ts' && e.name !== 'connect.ts')
      .map(e => e.name);

    // Partition
    const connectFiles = tsFilesAll.filter(n => hasSuffix(n, '_connect.ts'));
    const rawPbFiles   = tsFilesAll.filter(n => !hasSuffix(n, '_connect.ts'));

    // Avoid generic vs qualified duplicates like Backup_pb vs AssetBackup_pb
    let pbFiles = filterGenericVsQualifiedPb(rawPbFiles).filter(n => hasSuffix(n, '_pb.ts'));

    // If there is a *_connect.ts for a base, exclude its *_pb.ts from the index to avoid symbol collisions
    const connectBases = new Set(connectFiles.map(baseOf));
    pbFiles = pbFiles.filter(pb => !connectBases.has(baseOf(pb)));

    // Deduplicate by suffix and bias toward longer names
    pbFiles = dedupeBySuffix(pbFiles, '_pb.ts');

    // ----- Write index.ts (only PB + non-connect helpers that end with _pb.ts) -----
    {
      let idx = `// Auto-generated - DO NOT EDIT\n`;
      for (const f of pbFiles.sort()) {
        idx += `export * from './${stripTs(f)}';\n`;
      }
      for (const sd of subdirs) {
        idx += `export * as ${sd.replace(/[^A-Za-z0-9_]/g,'')} from './${sd}';\n`;
      }
      await fsp.writeFile(path.join(dir, 'index.ts'), idx, 'utf8');
    }

    // ----- connect.ts (ONLY if this dir or any subdir has *_connect.ts) -----
    const subdirsWithConnect = subdirs.filter(sd => dirHasConnect(path.join(dir, sd)));
    if (connectFiles.length || subdirsWithConnect.length) {
      let cidx = `// Auto-generated - DO NOT EDIT\n`;
      for (const f of connectFiles.sort()) {
        cidx += `export * from './${stripTs(f)}';\n`;
      }
      for (const sd of subdirsWithConnect) {
        cidx += `export * as ${sd.replace(/[^A-Za-z0-9_]/g,'')} from './${sd}/connect';\n`;
      }
      await fsp.writeFile(path.join(dir, 'connect.ts'), cidx, 'utf8');
    } else {
      const cpath = path.join(dir, 'connect.ts');
      if (fs.existsSync(cpath)) await fsp.rm(cpath).catch(() => {});
    }
  }

  // Generate barrels bottom-up
  for (const dir of Array.from(allDirs).sort((a, b) => b.length - a.length)) {
    const entries = fs.readdirSync(dir, { withFileTypes: true });
    const hasAnyTs = entries.some(e => e.isFile() && e.name.endsWith('.ts') && e.name !== 'index.ts' && e.name !== 'connect.ts');
    const hasSub   = entries.some(e => e.isDirectory());
    if (hasAnyTs || hasSub) {
      await generateIndexFor(dir);
    }
  }

  // Keep your top-level barrels
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

  const stripTs = (n) => n.replace(/\.ts$/, '');
  const baseOf = (n) => stripTs(n).replace(/_(pb|connect)$/, '');
  const hasSuffix = (n, s) => n.toLowerCase().endsWith(s.toLowerCase());

  // Prefer qualified "*Backup_pb.ts" over generic "Backup_pb.ts"
  const filterGenericVsQualifiedPb = (list) => {
    const hasQualifiedBackup = list.some(n => /[A-Za-z0-9]+Backup_pb\.ts$/.test(n) && n !== 'Backup_pb.ts');
    return list.filter(n => !(n === 'Backup_pb.ts' && hasQualifiedBackup));
  };

  const relDeep = (from, to) => path.relative(from, to).replace(/\\/g, '/');

  function shimDir(deepDir, outDir) {
    const entries = fs.readdirSync(deepDir, { withFileTypes: true });
    const files = entries.filter(e => e.isFile() && e.name.endsWith('.ts') && e.name !== 'index.ts' && e.name !== 'connect.ts').map(e => e.name);
    const subdirs = entries.filter(e => e.isDirectory()).map(e => e.name).sort();

    const connectFiles = files.filter(n => hasSuffix(n, '_connect.ts')).sort();
    let pbFiles = files.filter(n => hasSuffix(n, '_pb.ts')).sort();

    // drop pb if connect exists for same base
    const connectBases = new Set(connectFiles.map(baseOf));
    pbFiles = pbFiles.filter(pb => !connectBases.has(baseOf(pb)));
    // drop generic Backup_pb when a qualified one exists
    pbFiles = filterGenericVsQualifiedPb(pbFiles);

    // Create shims for PB files in outDir/
    fs.mkdirSync(outDir, { recursive: true });
    let idx = `// Auto-generated - DO NOT EDIT\n`;
    for (const f of pbFiles) {
      const base = stripTs(f);
      const src = path.join(deepDir, f);
      const shimPath = path.join(outDir, `${base}.ts`);
      const importFrom = relDeep(path.dirname(shimPath), src).replace(/\.ts$/, '');
      fs.writeFileSync(shimPath, `// Auto-generated - DO NOT EDIT\nexport * from '${importFrom}';\n`);
      idx += `export * from './${base}';\n`;
    }
    // Recurse for subdirs
    for (const sd of subdirs) {
      const subOut = path.join(outDir, sd);
      const subDeep = path.join(deepDir, sd);
      shimDir(subDeep, subOut);
      idx += `export * as ${sd.replace(/[^A-Za-z0-9_]/g,'')} from './${sd}';\n`;
    }
    fs.writeFileSync(path.join(outDir, 'index.ts'), idx);

    // Create connect barrel and shims under outDir/connect/
    const subdirsWithConnect = subdirs.filter(sd => {
      // look for any *_connect.ts in that sub-tree
      const walk = (p) => {
        const ents = fs.readdirSync(p, { withFileTypes: true });
        if (ents.some(e => e.isFile() && e.name.endsWith('_connect.ts'))) return true;
        return ents.some(e => e.isDirectory() && walk(path.join(p, e.name)));
      };
      return walk(path.join(deepDir, sd));
    });

    if (connectFiles.length || subdirsWithConnect.length) {
const connectDir = path.join(outDir, 'connect');
fs.mkdirSync(connectDir, { recursive: true });
let cidx = `// Auto-generated - DO NOT EDIT\n`;
for (const f of connectFiles) {
  const base = stripTs(f);
  const src = path.join(deepDir, f);
  const shimPath = path.join(connectDir, `${base}.ts`);
  const importFrom = relDeep(path.dirname(shimPath), src).replace(/\.ts$/, '');
  fs.writeFileSync(shimPath, `// Auto-generated - DO NOT EDIT\nexport * from '${importFrom}';\n`);
  cidx += `export * from './${base}';\n`;
}
for (const sd of subdirsWithConnect) {
  cidx += `export * as ${sd.replace(/[^A-Za-z0-9_]/g,'')} from '../${sd}/connect';\n`;
}
fs.writeFileSync(path.join(connectDir, 'index.ts'), cidx);

    } else {
// clean up stale sibling file and/or dir when no connect
const siblingBarrel = path.join(outDir, 'connect.ts');
if (fs.existsSync(siblingBarrel)) fs.rmSync(siblingBarrel);
const connectDir = path.join(outDir, 'connect');
if (fs.existsSync(connectDir)) fs.rmSync(connectDir, { recursive: true, force: true });

    }
  }

  for (const mod of fs.readdirSync(deepRoot, { withFileTypes: true })) {
    if (!mod.isDirectory()) continue;
    const deepModDir = path.join(deepRoot, mod.name);
    const outModDir  = path.join(flatRoot, mod.name);
    shimDir(deepModDir, outModDir);
  }
}


async function buildProtosFlatShims() {
  const deepRoot = path.join(cwd, 'ts-gen', 'gen', 'Protos', 'IT', 'WebServices', 'Fragments');
  const outRoot  = path.join(cwd, 'ts-gen', 'protos');
  if (!fs.existsSync(deepRoot)) return;

  const stripTs = (n) => n.replace(/\.ts$/, '');
  const baseOf = (n) => stripTs(n).replace(/_(pb|connect)$/, '');
  const hasSuffix = (n, s) => n.toLowerCase().endsWith(s.toLowerCase());
  const rel = (from, to) => path.relative(from, to).replace(/\\/g, '/');

  const filterGenericVsQualifiedPb = (list) => {
    const hasQualifiedBackup = list.some(n => /[A-Za-z0-9]+Backup_pb\.ts$/.test(n) && n !== 'Backup_pb.ts');
    return list.filter(n => !(n === 'Backup_pb.ts' && hasQualifiedBackup));
  };

  await ensureDir(outRoot);
  const baseIdx = `// Auto-generated - DO NOT EDIT
export * from '../gen/Protos/IT/WebServices/Fragments';
`;
  await fsp.writeFile(path.join(outRoot, 'index.ts'), baseIdx, 'utf8');

  function buildForModule(moduleDeepDir, moduleOutDir) {
    fs.mkdirSync(moduleOutDir, { recursive: true });
    const entries = fs.readdirSync(moduleDeepDir, { withFileTypes: true });

    const files   = entries.filter(e => e.isFile() && e.name.endsWith('.ts') && e.name !== 'index.ts' && e.name !== 'connect.ts').map(e => e.name);
    const subdirs = entries.filter(e => e.isDirectory()).map(e => e.name).sort();

    const connectFiles = files.filter(n => hasSuffix(n, '_connect.ts')).sort();
    let pbFiles = files.filter(n => hasSuffix(n, '_pb.ts')).sort();

    const connectBases = new Set(connectFiles.map(baseOf));
    pbFiles = pbFiles.filter(pb => !connectBases.has(baseOf(pb)));
    pbFiles = filterGenericVsQualifiedPb(pbFiles);

    let idx = `// Auto-generated - DO NOT EDIT\n`;
    for (const f of pbFiles) {
      const base = stripTs(f);
      const out  = path.join(moduleOutDir, `${base}.ts`);
      const imp  = rel(path.dirname(out), path.join(moduleDeepDir, f)).replace(/\.ts$/, '');
      fs.writeFileSync(out, `// Auto-generated - DO NOT EDIT\nexport * from '${imp}';\n`);
      idx += `export * from './${base}';\n`;
    }
    for (const sd of subdirs) {
      const subDeep = path.join(moduleDeepDir, sd);
      const subOut  = path.join(moduleOutDir, sd);
      buildForModule(subDeep, subOut);
      idx += `export * as ${sd.replace(/[^A-Za-z0-9_]/g,'')} from './${sd}';\n`;
    }
    fs.writeFileSync(path.join(moduleOutDir, 'index.ts'), idx);

    // connect barrel
    const hasConnectRecursive = (() => {
      const walk = (p) => {
        const ents = fs.readdirSync(p, { withFileTypes: true });
        if (ents.some(e => e.isFile() && e.name.endsWith('_connect.ts'))) return true;
        return ents.some(e => e.isDirectory() && walk(path.join(p, e.name)));
      };
      return connectFiles.length > 0 || subdirs.some(sd => walk(path.join(moduleDeepDir, sd)));
    })();

    if (hasConnectRecursive) {
     const connectDir = path.join(moduleOutDir, 'connect');
fs.mkdirSync(connectDir, { recursive: true });
let cidx = `// Auto-generated - DO NOT EDIT\n`;
for (const f of connectFiles) {
  const base = stripTs(f);
  const out  = path.join(connectDir, `${base}.ts`);
  const imp  = rel(path.dirname(out), path.join(moduleDeepDir, f)).replace(/\.ts$/, '');
  fs.writeFileSync(out, `// Auto-generated - DO NOT EDIT\nexport * from '${imp}';\n`);
  cidx += `export * from './${base}';\n`;
}
for (const sd of subdirs) {
  cidx += `export * as ${sd.replace(/[^A-Za-z0-9_]/g,'')} from '../${sd}/connect';\n`;
}
fs.writeFileSync(path.join(connectDir, 'index.ts'), cidx);
    } else {
const siblingBarrel = path.join(moduleOutDir, 'connect.ts');
if (fs.existsSync(siblingBarrel)) fs.rmSync(siblingBarrel);
const cdir = path.join(moduleOutDir, 'connect');
if (fs.existsSync(cdir)) fs.rmSync(cdir, { recursive: true, force: true });

    }
  }

  for (const mod of fs.readdirSync(deepRoot, { withFileTypes: true })) {
    if (!mod.isDirectory()) continue;
    buildForModule(path.join(deepRoot, mod.name), path.join(outRoot, mod.name));
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
  if (e.name === 'validation.ts') continue; // keep the helper
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
