# IT.WebServices.Fragments - Types

Types-only package generation for IT WebServices Fragments published as `@inverted-tech/fragments`.

## Structure
- `Protos/` - protobuf sources
- `ts-gen/` - generated TypeScript and barrel indexes
- `generate-ts.mjs` - cross-platform generator (Node.js)
- `buf.yaml` / `buf.gen.yaml` - Buf config

## Generate
```bash
# From the Fragments directory
npm run build   # generates TS and emits .d.ts to dist/, plus JS to dist/esm (ESM-only)
```

Clean and rebuild:
```bash
npm run rebuild
```

## Import Patterns
Published as `@inverted-tech/fragments`. This package ships declaration files and an ESM runtime (ESM-only).

- Deep import for specific types (recommended):
```ts
import type { UserRecord } from '@inverted-tech/fragments/gen/Protos/IT/WebServices/Fragments/Authentication/UserRecord_pb';
```

- Namespaced imports via generated indexes (avoids symbol collisions):
```ts
import { Authentication } from '@inverted-tech/fragments/gen/Protos/IT/WebServices/Fragments';
type User = Authentication.UserRecord_pb.UserRecord;
```

- Convenience subpaths for app usage:
  - Protobuf-es/connect-es code (services + messages):
    ```ts
    // Protos namespace re-export
    import { Authentication } from '@inverted-tech/fragments/protos/gen/Protos/IT/WebServices/Fragments';
    // Or deep messages
    import { UserRecord } from '@inverted-tech/fragments/protos/gen/Protos/IT/WebServices/Fragments/Authentication/UserRecord_pb';
    ```
  - Zod schemas for runtime validation (data messages only):
    ```ts
    // Namespaced
    import { Authentication as AuthSchemas } from '@inverted-tech/fragments/schemas/IT/WebServices/Fragments';
    const UserRecordSchema = AuthSchemas.Authentication.UserRecordSchema;

    // Or deep import
    import { UserRecordSchema } from '@inverted-tech/fragments/schemas/IT/WebServices/Fragments/Authentication/UserRecord';
    ```

## Modules
Authentication, Authorization, Comment, Content, Generic, Notification, Page, Settings

Indexes use namespaced re-exports to prevent symbol collisions across files.

## Releases (Changesets)
This package uses Changesets to manage versions and publish to npm.

Prerequisites:
- Logged in to npm with rights for the `@inverted-tech` scope (`npm login`).
- 2FA configured if required (`npm profile enable-2fa auth-and-writes`).

Workflow (run from the `Fragments` directory):
```bash
# 1) Create a changeset (choose patch/minor/major)
npm run changeset

# 2) Apply versions and update CHANGELOG.md
npm run release:version

# 3) Build (ESM + types) and publish via Changesets
npm run release:publish
```

Notes:
- Scripts: `changeset`, `release:version`, `release:publish`.
- CI can be added later with `changesets/action` for automated releases on main.
