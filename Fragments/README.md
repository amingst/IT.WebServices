# IT.WebServices.Fragments TypeScript Generation

This directory contains protobuf definitions and generates TypeScript definitions for the IT WebServices Fragments.

## Structure

-   `Protos/` - Contains the protobuf definition files
-   `ts-gen/` - Generated TypeScript files and index
-   `generate-ts.sh` - Script to generate TypeScript from protobuf files
-   `buf.yaml` - Buf configuration for protobuf generation
-   `buf.gen.yaml` - Buf generation configuration
-   `package.json` - Node.js dependencies for generation

## Usage

### Generate TypeScript Files

```bash
# Run the generation script
bash generate-ts.sh

# Or use npm script
npm run build
```

### Clean Generated Typescript Files

```bash
# Clean all generated files
npm run clean

# Clean and regenerate
npm run rebuild
```

## Generated Files

The script generates:

1. **TypeScript Protobuf Files** (`*_pb.ts`) - Message definitions
2. **Connect-ES Service Files** (`*_connect.ts`) - gRPC service definitions
3. **Hierarchical Index Files** (`index.ts`) - Clean imports at every level
4. **Main Index File** (`ts-gen/index.ts`) - Single entry point for all exports

## Hierarchical Import Structure

The new structure provides clean, conflict-free imports at multiple levels:

### Method 1: Full Hierarchy Access

```typescript
import * as Fragments from '@invertedtech/protos';
const userRecord =
	new Fragments.IT.WebServices.Fragments.Authentication.UserRecord();
```

### Method 2: Import at Module Level

```typescript
import { IT } from '@invertedtech/protos/gen/Protos';
const userRecord = new IT.WebServices.Fragments.Authentication.UserRecord();
```

### Method 3: Import Specific Modules

```typescript
import * as Authentication from '@invertedtech/protos/gen/Protos/IT/WebServices/Fragments/Authentication';
const userRecord = new Authentication.UserRecord();
```

### Method 4: Import Classes Directly (Recommended)

```typescript
import {
	UserRecord,
	UserPublicRecord,
} from '@invertedtech/protos/gen/Protos/IT/WebServices/Fragments/Authentication';
const userRecord = new UserRecord();
const publicRecord = new UserPublicRecord();
```

### Method 5: Import Services

```typescript
import { UserInterfaceService } from '@invertedtech/protos/gen/Protos/IT/WebServices/Fragments/Authentication';
// Use for gRPC service calls
```

## Available Modules

Currently generating TypeScript for these modules:

-   ✅ Authentication
-   ✅ Authorization
-   ✅ Comment
-   ✅ Content
-   ✅ CreatorDashboard
-   ✅ Generic
-   ✅ Notification
-   ✅ Page
-   ✅ Settings

## Known Issues

### Content Module

The Content module has enum value conflicts between `Content.proto` and `AssetInterface.proto`:

-   `None` and `Audio` enum values are defined in both files
-   This causes protobuf compilation to fail due to C++ scoping rules

**Resolution**: The proto files need to be updated to use unique enum values or proper namespacing.

## Dependencies

-   `@bufbuild/buf` - Protobuf build tool
-   `@bufbuild/protobuf` - Protobuf runtime
-   `@bufbuild/protoc-gen-es` - TypeScript protobuf generator
-   `@bufbuild/protoc-gen-connect-es` - Connect-ES service generator

## Import Usage

```typescript
// Import everything
import * as Fragments from '@invertedtech/protos';

// Import specific modules
import { Authentication_UserRecord } from '@invertedtech/protos';

// Import services
import { UserInterfaceService } from '@invertedtech/protos';
```
