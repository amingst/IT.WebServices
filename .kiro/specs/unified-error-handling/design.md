# Design Document

## Overview

This design outlines the implementation of unified error handling for the UserService.cs and the creation of a generic protobuf error extension method. The solution will standardize error responses while maintaining backward compatibility and integrating with existing validation patterns.

The design follows the existing architectural patterns in the codebase, particularly the extension method patterns found in `Fragments/Protos/IT/WebServices/Fragments/Generic/GenericExtensions.cs` and the current AuthError structure defined in the protobuf files.

## Architecture

### Current State Analysis

The UserService currently uses inconsistent error handling patterns:
- Some methods return `AuthError` objects with Type, Message, and Validation fields
- Other methods use specific response error enums (e.g., `ChangeOtherPasswordResponseErrorType`)
- Validation logic exists but is not consistently integrated with error responses
- The existing ProtoValidate validation in `CreateUser` method provides a good pattern to follow

### Target Architecture

The unified error handling will:
1. Standardize all UserService methods to use AuthError consistently
2. Provide a generic extension method for protobuf error types
3. Maintain existing validation logic while enhancing error reporting
4. Preserve backward compatibility through enum-to-AuthErrorReason mapping

## Components and Interfaces

### 1. Generic Protobuf Error Extensions

**Location:** `Fragments/Protos/IT/WebServices/Fragments/Generic/ErrorExtensions.cs`

**Purpose:** Provide generic extension methods for protobuf error types that support ValidationIssue collections.

**Key Methods:**
- `CreateValidationError<T>()` - Creates error with validation issues
- `CreateNotFoundError<T>()` - Creates user not found errors
- `CreateUnauthorizedError<T>()` - Creates unauthorized access errors
- `CreateServiceUnavailableError<T>()` - Creates offline/service unavailable errors
- `AddValidationIssue<T>()` - Adds individual validation issues
- `FromProtoValidateResult<T>()` - Converts ProtoValidate results to error format

### 2. UserService Error Mapping

**Purpose:** Map existing error enum values to AuthErrorReason values for backward compatibility.

**Implementation Strategy:**
- Create mapping methods for each response type enum
- Preserve existing error semantics while using AuthError structure
- Maintain existing method signatures initially

### 3. Enhanced AuthError Usage

**Purpose:** Standardize error responses across all UserService methods.

**Key Changes:**
- Replace enum-based error returns with AuthError objects
- Integrate validation error collection consistently
- Preserve existing error messages and types through mapping

## Data Models

### AuthError Structure (Existing)
```protobuf
message AuthError {
    AuthErrorReason Type = 1;
    string Message = 2;
    repeated ValidationIssue Validation = 3;
}
```

### ValidationIssue Structure (Existing)
```protobuf
message ValidationIssue {
    string field = 1;      
    string message = 2;   
    string code = 3;  
}
```

### Error Mapping Structures

The design will create mapping functions between existing error enums and AuthErrorReason values:

- `ChangeOtherPasswordResponseErrorType` → `AuthErrorReason`
- `ChangeOtherProfileImageResponseErrorType` → `AuthErrorReason`
- `ChangeOwnPasswordResponseErrorType` → `AuthErrorReason`
- `ChangeOwnProfileImageResponseErrorType` → `AuthErrorReason`
- `DisableEnableOtherUserResponseErrorType` → `AuthErrorReason`

## Error Handling

### Generic Extension Method Error Handling

The extension methods will handle common error scenarios:

1. **Validation Errors**: Collect and format field-level validation issues
2. **Not Found Errors**: Standardized user/resource not found responses
3. **Authorization Errors**: Consistent unauthorized access responses
4. **Service Errors**: Offline and service unavailable responses
5. **Unknown Errors**: Generic error handling with logging support

### UserService Error Integration

Each UserService method will be updated to:

1. Use the generic extension methods for error creation
2. Map existing error enum values to AuthErrorReason
3. Preserve existing validation logic
4. Maintain current error messages and semantics



## Implementation Phases

### Phase 1: Generic Extension Method Creation
- Create ErrorExtensions.cs in Fragments/Generic
- Implement core extension methods

### Phase 2: UserService Method Updates
- Update methods one by one to use AuthError
- Implement error enum to AuthErrorReason mapping
- Preserve existing validation logic
- Maintain backward compatibility

### Phase 3: Integration and Validation
- Verify compatibility with existing clients
- Documentation updates
- Code review and refinement

## Security Considerations

1. **Sensitive Information Protection**:
   - Ensure error messages don't leak sensitive data
   - Maintain existing logging patterns for security events
   - Preserve authentication failure handling

2. **Validation Security**:
   - Maintain existing validation rules
   - Ensure validation errors don't expose system internals
   - Preserve rate limiting and security controls

## Performance Considerations

1. **Error Object Creation**:
   - Minimize object allocation in error paths
   - Reuse common error instances where appropriate
   - Maintain existing performance characteristics

2. **Validation Performance**:
   - Preserve existing ProtoValidate performance
   - Avoid duplicate validation processing
   - Optimize ValidationIssue collection building

## Backward Compatibility

1. **Response Format**:
   - Maintain existing error semantics
   - Preserve error message content
   - Map enum values to appropriate AuthErrorReason values

2. **Client Integration**:
   - Ensure existing clients can handle AuthError format
   - Provide migration guidance for client updates
   - Maintain API contract compatibility