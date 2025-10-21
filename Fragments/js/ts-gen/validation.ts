// js/ts-gen/validation.ts
import {
	createValidator,
	type ValidationResult,
} from '@bufbuild/protovalidate';
import type { DescMessage, Message, PlainMessage } from '@bufbuild/protobuf';

// Reuse one validator instance
export const validator = createValidator();

/**
 * Validate using a protobuf-es Schema (*Schema constant) + a value (plain or message instance).
 * Note: In protovalidate@1.0.0, there is no single-argument overload — schema is required.
 */
export function validate(
	schema: DescMessage,
	value: Message | PlainMessage<Message>
): ValidationResult {
	return validator.validate(schema, value);
}

/** Normalize a violation's path into a dot string. */
function pathOf(v: any): string {
	if (typeof v.fieldPath === 'string') return v.fieldPath;
	if (Array.isArray(v.fieldPath)) return v.fieldPath.join('.');
	if (typeof v.path === 'string') return v.path;

	const fp = v.fieldPath ?? v.FieldPath ?? v.Path;
	const segs = fp?.segments ?? fp?.Segments;
	if (Array.isArray(segs)) {
		const parts = segs
			.map((s: any) => s?.field ?? s?.name ?? s?.Field ?? s?.Name)
			.filter(Boolean);
		if (parts.length) return parts.join('.');
	}
	return '';
}

/** Normalize a violation's code (rule/constraint id). */
function codeOf(v: any): string {
	if (typeof v.constraintId === 'string') return v.constraintId;
	if (typeof v.ruleId === 'string') return v.ruleId;
	if (typeof v.code === 'string') return v.code;

	const rule = v.rule ?? v.Rule;
	if (rule) return rule.id ?? rule.name ?? rule.Id ?? rule.Name ?? '';
	return '';
}

/** Convert ValidationResult → { field, message, code }[] */
export function toIssues(res: ValidationResult) {
	if (res.kind === 'valid' || !res.violations?.length) return [];
	return res.violations.map((v: any) => ({
		field: pathOf(v),
		message: String(v.message ?? 'Invalid value'),
		code: codeOf(v),
	}));
}

/**
 * Zod-like assertion. We keep the schema/value untyped here to align with DescMessage being non-generic.
 * If you want caller-side type narrowing, wrap this with a typed helper per message.
 */
export function assertValid(
	schema: DescMessage,
	value: Message | PlainMessage<Message>
): void {
	const r = validate(schema, value);
	if (r.kind === 'invalid' && r.violations?.length) {
		const details = r.violations
			.map((v: any) => `${pathOf(v)}: ${v.message ?? 'Invalid value'}`)
			.join('; ');
		throw new Error(`Validation failed: ${details}`);
	}
}
