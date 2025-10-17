# JSON Validation Rules

This folder contains JSON-driven validation rules per RPC. Rules are evaluated at the service boundary. On the first violation (default), the service sets the existing proto response `Error` enum and returns.

- Location: `configs/validation/{Domain}/{Service}/{Rpc}.json`
- Mode: `first_error` (stop at first violation) or `collect_all`
- Keep wire-compat: continue to use existing `*ResponseErrorType` enums defined in protos.

See `_schema/validation.schema.json` for the rule file schema.

Skeleton file contents:
```
{
  "version": "1",
  "rpc": "<RpcName>",
  "mode": "first_error",
  "rules": []
}
```
