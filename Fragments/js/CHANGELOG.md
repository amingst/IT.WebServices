# @inverted-tech/fragments

## 0.2.4

### Patch Changes

- DX: flatten exports for protos and schemas so consumers can import modules directly from @inverted-tech/fragments/protos and /schemas without deep paths (Authentication, Content, etc.).
- Automated patch bump

## 0.2.3

### Patch Changes

- Exports: add .js/.d.ts in wildcard subpaths (schemas/_, protos/_, gen/\*) so deep imports work without explicit extension in editors and TS.
- Automated patch bump

## 0.2.2

### Patch Changes

- Types: add explicit export subpaths for schemas/_ and protos/_ to improve TS editor resolution of deep imports in IDEs (e.g., VS Code).
- Automated patch bump

## 0.2.1

### Patch Changes

- Fix: emit true ESM in dist/esm for Next.js/Turbopack; ensure deep schema exports work (e.g., Authentication/UserRecord). ESM-only package.
- Automated patch bump

## 0.2.0

### Minor Changes

- Remove cjs

## 0.1.1

### Patch Changes

- Add scripts for development process

## 0.1.0

### Minor Changes

- 5a3b5ad: Initial release of @inverted-tech/fragments with dual ESM/CJS runtime and declaration files.
