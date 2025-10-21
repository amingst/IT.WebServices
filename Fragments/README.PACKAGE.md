# @inverted-tech/fragments

Runtime-ready TypeScript artifacts for IT WebServices Fragments.

What’s included
- Protos: Protobuf-ES message classes and Connect service descriptors.
  - Import via subpath: `@inverted-tech/fragments/protos`
- Schemas: Zod validation schemas for domain data messages (requests/responses and service interfaces are excluded).
  - Import via subpath: `@inverted-tech/fragments/schemas`
- Dual module outputs (ESM + CJS) and `.d.ts` types.

Install
```bash
npm install @inverted-tech/fragments
```

Quick start
- Protos (messages + service descriptors)
```ts
// Namespaced protos
import { Authentication } from '@inverted-tech/fragments/protos/gen/Protos/IT/WebServices/Fragments';

// Deep import a specific message
import { UserRecord } from '@inverted-tech/fragments/protos/gen/Protos/IT/WebServices/Fragments/Authentication/UserRecord_pb';
```

- Schemas (runtime validation with Zod)
```ts
// Namespaced schemas
import { IT as Schemas } from '@inverted-tech/fragments/schemas/IT';
const UserRecordSchema = Schemas.WebServices.Fragments.Authentication.UserRecordSchema;

// Or deep import a specific schema
import { UserRecordSchema } from '@inverted-tech/fragments/schemas/IT/WebServices/Fragments/Authentication/UserRecord';

// Infer TS types from schemas
import { z } from 'zod';
type UserRecordInput = z.infer<typeof UserRecordSchema>;
```

Notes
- Zod schemas focus on domain data messages (e.g., `*Record`, `*Settings`).
  - Request/Response and service-interface-only types are intentionally omitted.
- Timestamps map to `Date`. Duration maps to `{ seconds?: bigint; nanos?: number }`.

Support matrix
- Node.js ≥ 18
- Modern browsers (ES2020)

Changelog
- This package uses Changesets; see release notes on the npm page.

License
- See `LICENSE` in the package.