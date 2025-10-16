#!/usr/bin/env node

// Cross-platform TypeScript generation for protobufs in Fragments
// - Discovers modules under Protos/IT/WebServices/Fragments
// - Runs buf generate per module + root
// - Builds hierarchical index.ts files

import { spawnSync } from 'node:child_process';
import { promises as fsp } from 'node:fs';
import fs from 'node:fs';
import path from 'node:path';

const scriptDir = path.dirname(new URL(import.meta.url).pathname).replace(/^\//, '');
const cwd = scriptDir;
const nodeBin = path.join(cwd, 'node_modules', '.bin');
const bufBin = path.join(nodeBin, process.platform === 'win32' ? 'buf.cmd' : 'buf');
const contextCwd = path.join(cwd, '..'); // run buf from parent so ../Protos is inside context

function log(msg) {
  console.log(msg);
}

async function pathExists(p) {
  try {
    await fsp.access(p);
    return true;
  } catch {
    return false;
  }
}

async function rimrafSafe(target) {
  if (!(await pathExists(target))) return;
  // Remove directory contents recursively
  await fsp.rm(target, { recursive: true, force: true });
}

async function ensureDir(dir) {
  await fsp.mkdir(dir, { recursive: true });
}

function runBufGenerate(relPath) {
  // Ensure both PATH and Path are set on Windows
  const currentPath = process.env.PATH || process.env.Path || '';
  const newPath = `${nodeBin}${path.delimiter}${currentPath}`;
  const env = { ...process.env, PATH: newPath, Path: newPath };
  const template = path.join(cwd, 'buf.gen.yaml');
  // Use shell on Windows to ensure .cmd is executed correctly
  const cmd = process.platform === 'win32'
    ? `"${bufBin}" generate --template "${template}" --path "${relPath.replaceAll('\\','/')}"`
    : `${bufBin} generate --template ${template} --path ${relPath}`;
  const res = spawnSync(cmd, {
    cwd: contextCwd,
    env,
    stdio: 'pipe',
    shell: true,
  });
  if (res.stdout?.length) {
    process.stdout.write(res.stdout);
  }
  if (res.stderr?.length) {
    process.stderr.write(res.stderr);
  }
  return res.status === 0;
}

// Optional second-pass generator for Zod schemas via ts-proto template.
function runBufGenerateZod(relPath) {
  const currentPath = process.env.PATH || process.env.Path || '';
  const newPath = `${nodeBin}${path.delimiter}${currentPath}`;
  const env = { ...process.env, PATH: newPath, Path: newPath };
  const template = path.join(cwd, 'buf.gen.zod.yaml');
  const cmd = process.platform === 'win32'
    ? `"${bufBin}" generate --template "${template}" --path "${relPath.replaceAll('\\','/')}"`
    : `${bufBin} generate --template ${template} --path ${relPath}`;
  const res = spawnSync(cmd, {
    cwd: contextCwd,
    env,
    stdio: 'pipe',
    shell: true,
  });
  if (res.stdout?.length) {
    process.stdout.write(res.stdout);
  }
  if (res.stderr?.length) {
    process.stderr.write(res.stderr);
  }
  return res.status === 0;
}

async function discoverModules() {
  const base = path.join(contextCwd, 'Protos', 'IT', 'WebServices', 'Fragments');
  const modules = [];
  const entries = await fsp.readdir(base, { withFileTypes: true });
  for (const e of entries) {
    if (!e.isDirectory()) continue;
    const moduleDir = path.join(base, e.name);
    // Check if directory contains any .proto files (recursively)
    const hasProto = await hasProtoFiles(moduleDir);
    if (hasProto) modules.push(e.name);
  }
  // Root-level protos
  const rootHasProto = (await listProtoFiles(base)).length > 0;
  return { modules, rootHasProto };
}

async function listProtoFiles(dir) {
  const files = [];
  async function walk(d) {
    const entries = await fsp.readdir(d, { withFileTypes: true });
    for (const ent of entries) {
      const full = path.join(d, ent.name);
      if (ent.isDirectory()) continue; // do not recurse for listing convenience here
      if (full.toLowerCase().endsWith('.proto')) files.push(full);
    }
  }
  await walk(dir);
  return files;
}

async function hasProtoFiles(dir) {
  // check immediate files only like original script
  const files = await listProtoFiles(dir);
  return files.length > 0;
}

async function generateIndexes() {
  const genRoot = path.join(cwd, 'ts-gen', 'gen');
  if (!(await pathExists(genRoot))) return;

  async function directoriesWithTsFiles() {
    const dirs = new Set();
    async function walk(d) {
      const entries = await fsp.readdir(d, { withFileTypes: true });
      let hasTs = false;
      for (const ent of entries) {
        const full = path.join(d, ent.name);
        if (ent.isDirectory()) {
          await walk(full);
        } else if (ent.isFile() && ent.name.endsWith('.ts')) {
          hasTs = true;
        }
      }
      if (hasTs) dirs.add(d);
    }
    await walk(genRoot);
    // Sort deepest first
    return Array.from(dirs).sort((a, b) => b.split(path.sep).length - a.split(path.sep).length);
  }

  async function hasAnyTsRecursively(dir) {
    let found = false;
    async function walk(d) {
      const entries = await fsp.readdir(d, { withFileTypes: true });
      for (const ent of entries) {
        const full = path.join(d, ent.name);
        if (ent.isDirectory()) {
          await walk(full);
          if (found) return;
        } else if (ent.isFile() && ent.name.endsWith('.ts')) {
          found = true;
          return;
        }
      }
    }
    await walk(dir);
    return found;
  }

  async function generateIndexFor(dirPath) {
    const indexFile = path.join(dirPath, 'index.ts');
    const entries = await fsp.readdir(dirPath, { withFileTypes: true });
    const tsFiles = entries
      .filter((e) => e.isFile() && e.name.endsWith('.ts') && e.name !== 'index.ts')
      .map((e) => e.name)
      .sort();
    const subdirs = entries.filter((e) => e.isDirectory()).map((e) => e.name).sort();

    let content = '';
    content += `// Auto-generated index file - DO NOT EDIT MANUALLY\n`;
    content += `// Generated on: ${new Date().toString()}\n\n`;

    if (tsFiles.length > 0) {
      content += `// Namespaced re-exports to avoid symbol collisions\n`;
      for (const f of tsFiles) {
        const base = path.basename(f, '.ts');
        content += `export * as ${base} from './${base}';\n`;
      }
      content += `\n`;
    }

    // Re-exports from subdirectories that contain .ts somewhere within
    const subdirsWithTs = [];
    for (const sd of subdirs) {
      const full = path.join(dirPath, sd);
      if (await hasAnyTsRecursively(full)) subdirsWithTs.push(sd);
    }
    if (subdirsWithTs.length > 0) {
      content += `// Re-exports from subdirectories\n`;
      for (const sd of subdirsWithTs) {
        content += `export * as ${sd} from './${sd}';\n`;
      }
    }

    await fsp.writeFile(indexFile, content, 'utf8');
  }

  const dirs = await directoriesWithTsFiles();
  for (const dir of dirs) {
    await generateIndexFor(dir);
  }

  // Also generate index files for intermediate directories without direct .ts but with subdirs containing .ts
  const allDirs = new Set();
  async function collect(d) {
    allDirs.add(d);
    const entries = await fsp.readdir(d, { withFileTypes: true });
    for (const ent of entries) {
      if (ent.isDirectory()) await collect(path.join(d, ent.name));
    }
  }
  await collect(genRoot);
  for (const dir of Array.from(allDirs).sort((a, b) => b.length - a.length)) {
    const entries = await fsp.readdir(dir, { withFileTypes: true });
    const hasIndex = entries.some((e) => e.isFile() && e.name === 'index.ts');
    if (!hasIndex) {
      // generate if any .ts exists anywhere within
      if (await hasAnyTsRecursively(dir)) {
        await generateIndexFor(dir);
      }
    }
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

  // Subpath entry: schemas is built by scripts/generate-zod-schemas.mjs
  // (no-op here)
}

async function main() {
  log('Starting TypeScript generation for all proto files...');
  log(`Working directory: ${cwd}`);
  log(`Using buf at: ${bufBin}`);
  log(`Extending PATH with: ${nodeBin}`);

  // Clean generated folder fully to avoid odd names like "gen?"
  const tsGenDir = path.join(cwd, 'ts-gen');
  await ensureDir(tsGenDir);
  const entries = await fsp.readdir(tsGenDir, { withFileTypes: true });
  for (const e of entries) {
    await rimrafSafe(path.join(tsGenDir, e.name));
  }
  await ensureDir(path.join(tsGenDir, 'gen'));
  log('Cleaned ts-gen directory.');

  // Discover modules
  const { modules, rootHasProto } = await discoverModules();
  log(`Discovered modules: ${modules.join(', ') || '(none)'}`);

  const successful = [];
  const failed = [];

  for (const mod of modules) {
    log(`Generating: ${mod}`);
    const ok = runBufGenerate(path.join('Protos', 'IT', 'WebServices', 'Fragments', mod));
    if (ok) {
      successful.push(mod);
    } else {
      failed.push(mod);
    }
  }

  if (rootHasProto) {
    log('Generating: root-level proto files');
    const ok = runBufGenerate(path.join('Protos', 'IT', 'WebServices', 'Fragments'));
    if (!ok) {
      log('Failed to generate root-level protos.');
    }
  }

  // Attempt to generate Zod schemas (non-fatal)
  const zodSuccess = [];
  const zodFail = [];
  for (const mod of modules) {
    log(`Generating (schemas): ${mod}`);
    const ok = runBufGenerateZod(path.join('Protos', 'IT', 'WebServices', 'Fragments', mod));
    if (ok) zodSuccess.push(mod); else zodFail.push(mod);
  }
  if (rootHasProto) {
    log('Generating (schemas): root-level proto files');
    const ok = runBufGenerateZod(path.join('Protos', 'IT', 'WebServices', 'Fragments'));
    if (!ok) log('Failed to generate root-level schemas.');
  }

  // Build indexes
  log('Building hierarchical index.ts files...');
  await generateIndexes();
  log('Index build complete.');

  // Summary
  function countFiles(dir, filter) {
    let count = 0;
    function walk(d) {
      for (const ent of fs.readdirSync(d, { withFileTypes: true })) {
        const full = path.join(d, ent.name);
        if (ent.isDirectory()) walk(full);
        else if (ent.isFile() && filter(full)) count++;
      }
    }
    if (fs.existsSync(dir)) walk(dir);
    return count;
  }

  const genRoot = path.join(cwd, 'ts-gen', 'gen');
  const tsFiles = countFiles(genRoot, (f) => f.endsWith('.ts') && !f.endsWith('index.ts'));
  const idxFiles = countFiles(genRoot, (f) => f.endsWith('index.ts'));

  log('Generation Summary:');
  log(`- Successful modules (${successful.length}): ${successful.join(', ')}`);
  if (failed.length) {
    log(`- Failed modules (${failed.length}): ${failed.join(', ')}`);
    log('- Note: Failed modules may have protobuf definition conflicts.');
  }
  if (zodSuccess.length || zodFail.length) {
    log(`- Zod schemas generated for (${zodSuccess.length}) modules: ${zodSuccess.join(', ')}`);
    if (zodFail.length) log(`- Zod schema failures (${zodFail.length}): ${zodFail.join(', ')}`);
  }
  log(`- TypeScript files: ${tsFiles}`);
  log(`- Index files: ${idxFiles}`);
  log(`- Total files: ${tsFiles + idxFiles}`);

  log('TypeScript generation and index building complete.');
}

main().catch((err) => {
  console.error('Generation failed with error:', err);
  process.exit(1);
});
