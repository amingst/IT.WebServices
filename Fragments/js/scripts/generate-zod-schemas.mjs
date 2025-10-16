#!/usr/bin/env node
import { promises as fsp } from 'node:fs';
import fs from 'node:fs';
import path from 'node:path';
import vm from 'node:vm';

const root = path.resolve(path.join(path.dirname(new URL(import.meta.url).pathname).replace(/^\//, ''), '..'));
const genZodTsRoot = path.join(root, 'ts-gen', '_meta', 'Protos');
const targetRoot = path.join(root, 'ts-gen', 'schemas');

function log(msg) { console.log(msg); }

async function ensureDir(dir) { await fsp.mkdir(dir, { recursive: true }); }

function extractFileDescriptor(source) {
  const key = 'fileDescriptor:';
  let searchFrom = 0;
  let idx = -1;
  while (true) {
    idx = source.indexOf(key, searchFrom);
    if (idx === -1) return null;
    let j = idx + key.length;
    while (j < source.length && /\s|\n|\r|\t/.test(source[j])) j++;
    if (source[j] === '{') { searchFrom = idx; break; }
    searchFrom = idx + key.length;
  }
  let i = searchFrom + key.length;
  while (i < source.length && /\s|\n|\r|\t/.test(source[i])) i++;
  if (source[i] !== '{') return null;
  let start = i;
  let depth = 0;
  for (; i < source.length; i++) {
    const ch = source[i];
    if (ch === '{') depth++;
    else if (ch === '}') {
      depth--;
      if (depth === 0) { i++; break; }
    }
  }
  const objText = source.slice(start, i);
  // Evaluate as JS object literal
  try {
    const ctx = {};
    const result = vm.runInNewContext('(' + objText + ')', ctx, { timeout: 1000 });
    return result;
  } catch (e) {
    return null;
  }
}

function typeToZodExpr(field) {
  // Protobuf FieldDescriptorProto.Type enums
  switch (field.type) {
    case 1: // double
    case 2: // float
      return 'z.number()';
    case 3: // int64
    case 4: // uint64
    case 6: // fixed64
    case 15: // sfixed32 (still 32, but keep number)
    case 16: // sfixed64
    case 18: // sint64
      return 'z.bigint()';
    case 5: // int32
    case 7: // fixed32
    case 13: // uint32
    case 17: // sint32
      return 'z.number().int()';
    case 8:
      return 'z.boolean()';
    case 9:
      return 'z.string()';
    case 12:
      return 'z.instanceof(Uint8Array)';
    case 11: // message
      // typeName is like .package.Message
      return null; // handled by caller
    default:
      return 'z.any()';
  }
}

async function collectFiles(dir) {
  const files = [];
  const dirs = [dir];
  while (dirs.length) {
    const d = dirs.pop();
    if (!fs.existsSync(d)) continue;
    for (const ent of fs.readdirSync(d, { withFileTypes: true })) {
      const full = path.join(d, ent.name);
      if (ent.isDirectory()) dirs.push(full);
      else if (ent.isFile() && ent.name.endsWith('.ts')) files.push(full);
    }
  }
  return files;
}

// Create a short schema id from a name path (without package), e.g. Parent.Child -> Parent_ChildSchema
function schemaIdFromNamePath(namePath) {
  return namePath.split('.').join('_') + 'Schema';
}

async function main() {
  log(`Generating Zod schemas from ts-proto metadata...`);
  if (!fs.existsSync(genZodTsRoot)) {
    log(`No gen-zod TS output found at ${genZodTsRoot}; skipping.`);
    return;
  }
  const files = await collectFiles(genZodTsRoot);
  log(`Scanning ${files.length} ts-proto files for descriptors...`);
  const schemaFiles = new Map();
  const typeToFile = new Map();
  const typeToShortId = new Map();

  function iterMessages(pkg, msgs, parent = '') {
    const out = [];
    for (const m of msgs || []) {
      const name = parent ? parent + '.' + m.name : m.name;
      const fq = (pkg ? pkg + '.' : '') + name;
      out.push({ fq, msg: m, namePath: name });
      if (m.nestedType && m.nestedType.length) {
        out.push(...iterMessages(pkg, m.nestedType, name));
      }
    }
    return out;
  }

  // First pass to build a map of fully-qualified type name -> source file and short ids
  for (const file of files) {
    const src = await fsp.readFile(file, 'utf8');
    const fdm = extractFileDescriptor(src);
    if (!fdm) continue;
    const pkg = fdm.package || '';
    const relFromProtos = path.relative(genZodTsRoot, file).replace(/\\/g, '/');
    for (const { fq, namePath } of iterMessages(pkg, fdm.messageType)) {
      typeToFile.set(fq, relFromProtos);
      typeToShortId.set(fq, schemaIdFromNamePath(namePath));
    }
  }

// Fallback mapping: fqName -> short id based on namePath (after package)
function schemaName(fqName) {
  const val = typeToShortId.get(fqName);
  if (val) return val;
  const afterPkg = fqName.replace(/^.*?\./, '');
  return afterPkg.replace(/\./g, '_') + 'Schema';
}

  // Generate per-file schemas
  for (const file of files) {
    const src = await fsp.readFile(file, 'utf8');
    const fdm = extractFileDescriptor(src);
    if (!fdm) continue;
    const relFromProtos = path.relative(genZodTsRoot, file).replace(/\\/g, '/');
    const base = path.basename(relFromProtos, '.ts');
    // Skip entire Interface files
    if (base.endsWith('Interface')) {
      continue;
    }
    const outTsPath = path.join(targetRoot, relFromProtos);
    await ensureDir(path.dirname(outTsPath));

    const pkg = fdm.package || '';
    const wktTimestampFq = 'google.protobuf.Timestamp';

    let content = '';
    content += `// Auto-generated Zod schemas - DO NOT EDIT\n`;
    content += `// Source: ${relFromProtos}\n`;
    content += `import { z } from 'zod';\n`;
    // Track imports required for referenced message schemas
    const importMap = new Map(); // fromPath -> Set(identifiers)

    // Collect messages, skipping Request/Response types by name
    const allMessages = iterMessages(pkg, fdm.messageType).filter(({ fq }) => {
      const simple = fq.split('.').pop() || '';
      return !(simple.endsWith('Request') || simple.endsWith('Response'));
    });
    if (allMessages.length === 0) {
      continue;
    }
    for (const { fq, msg } of allMessages) {
      const fields = msg.field || [];
      let objLines = [];
      for (const field of fields) {
        const name = field.jsonName || field.name;
        const label = field.label; // 1=optional, 2=required (proto2), 3=repeated
        let expr = typeToZodExpr(field);
        if (expr === null && field.type === 11) {
          // Message type
          const tn = field.typeName.replace(/^\./, '');
          if (tn === wktTimestampFq) {
            expr = 'z.date()';
          } else if (tn === 'google.protobuf.Duration') {
            expr = 'z.object({ seconds: z.optional(z.bigint()), nanos: z.optional(z.number().int()) })';
          } else {
            expr = `${schemaName(tn)}`;
            const depRel = typeToFile.get(tn);
            if (depRel && depRel !== relFromProtos) {
              const depOut = path.join(targetRoot, depRel);
              let fromPath = path.relative(path.dirname(outTsPath), depOut).replace(/\\/g, '/');
              if (!fromPath.startsWith('.')) fromPath = './' + fromPath;
              fromPath = fromPath.replace(/\.ts$/, '');
              const set = importMap.get(fromPath) ?? new Set();
              set.add(schemaName(tn));
              importMap.set(fromPath, set);
            }
          }
        }
        if (label === 3) expr = `z.array(${expr})`;
        // proto3 fields are optional by presence
        expr = `z.optional(${expr})`;
        objLines.push(`  ${name}: ${expr},`);
      }
      const id = schemaName(fq);
      content += `export const ${id}: z.ZodType<any> = z.lazy(() => z.object({\n${objLines.join('\n')}\n}));\n`;
    }

    // Inject imports
    if (importMap.size) {
      let importsText = '';
      for (const [from, ids] of importMap.entries()) {
        importsText += `import { ${Array.from(ids).join(', ')} } from '${from}';\n`;
      }
      content = content.replace("import { z } from 'zod';\n", "import { z } from 'zod';\n" + importsText);
    }
    await fsp.writeFile(outTsPath, content, 'utf8');
    schemaFiles.set(outTsPath, true);
  }

  // Build hierarchical namespace indexes to avoid name collisions
  async function buildIndexes(dir) {
    const entries = fs.readdirSync(dir, { withFileTypes: true });
    const files = entries.filter(e => e.isFile() && e.name.endsWith('.ts') && e.name !== 'index.ts').map(e => e.name).sort();
    const subdirs = entries.filter(e => e.isDirectory()).map(e => e.name).sort();
    let idx = '';
    idx += `// Auto-generated - DO NOT EDIT\n`;
    for (const f of files) idx += `export * from './${f.replace(/\.ts$/, '')}';\n`;
    for (const sd of subdirs) idx += `export * as ${sd} from './${sd}';\n`;
    await fsp.writeFile(path.join(dir, 'index.ts'), idx, 'utf8');
    for (const sd of subdirs) await buildIndexes(path.join(dir, sd));
  }
  await ensureDir(targetRoot);
  await buildIndexes(targetRoot);

  log(`Generated ${schemaFiles.size} schema files.`);

  // Cleanup meta directory to avoid clutter / duplicates
  const metaRoot = path.join(root, 'ts-gen', '_meta');
  try {
    await fsp.rm(metaRoot, { recursive: true, force: true });
    log('Cleaned up temporary ts-gen/_meta');
  } catch {}
}

main().catch((e) => { console.error('Zod schema generation failed:', e); process.exit(1); });
