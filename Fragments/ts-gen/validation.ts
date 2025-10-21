// ts-gen/validation.ts
import { createRegistry, type Registry } from "@bufbuild/protobuf";
import type { GenFile } from "@bufbuild/protobuf/codegenv2";
import { createValidator, type Validator } from "@bufbuild/protovalidate";

// Import barrels that re-export your generated descriptors (GenFile)
import * as Auth from "./gen/Protos/IT/WebServices/Fragments/Authentication";
import * as Authorization from "./gen/Protos/IT/WebServices/Fragments/Authorization";
import * as Comment from "./gen/Protos/IT/WebServices/Fragments/Comment";
import * as Content from "./gen/Protos/IT/WebServices/Fragments/Content";
import * as CreatorDashboard from "./gen/Protos/IT/WebServices/Fragments/CreatorDashboard";
import * as Generic from "./gen/Protos/IT/WebServices/Fragments/Generic";
import * as Notification from "./gen/Protos/IT/WebServices/Fragments/Notification";
import * as Page from "./gen/Protos/IT/WebServices/Fragments/Page";
import * as Settings from "./gen/Protos/IT/WebServices/Fragments/Settings";
import * as FragmentsRoot from "./gen/Protos/IT/WebServices/Fragments";

// Runtime guard (don’t use a type predicate over a module union)
function looksLikeGenFile(x: unknown): x is GenFile {
  const v = x as any;
  return !!v && typeof v === "object" && v.kind === "file" && typeof v.name === "string";
}

function collectFiles(): GenFile[] {
  const files: GenFile[] = [];
  const mods = [
    Auth,
    Authorization,
    Comment,
    Content,
    CreatorDashboard,
    Generic,
    Notification,
    Page,
    Settings,
    FragmentsRoot,
  ];
  for (const m of mods) {
    for (const value of Object.values(m) as unknown[]) {
      if (looksLikeGenFile(value)) files.push(value);
    }
  }
  return files;
}

const registry: Registry = createRegistry(...collectFiles());

/** Create a protovalidate Validator bound to this package’s descriptors. */
export async function getValidator(): Promise<Validator> {
  return createValidator({ registry });
}
