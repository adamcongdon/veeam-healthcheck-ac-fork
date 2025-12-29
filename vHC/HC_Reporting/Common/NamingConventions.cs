// Copyright (c) Veeam Software Group GmbH
// Licensed under MIT

// =============================================================================
// NAMING CONVENTION MIGRATION GUIDE
// =============================================================================
//
// BACKGROUND:
// This codebase historically used Hungarian-style "C" prefix for class names
// (e.g., CGlobals, CDataFormer, CCsvParser). This convention is no longer
// recommended in modern .NET development.
//
// CURRENT STATE:
// - 121 legacy classes use the "C" prefix
// - New classes should NOT use the "C" prefix
// - Gradual migration is in progress
//
// NEW CLASS NAMING GUIDELINES:
// ✓ Use PascalCase without prefix: Logger, DataFormer, CsvParser
// ✓ Use descriptive names: HealthCheckOptions, ProcessRunner
// ✓ Use standard suffixes: *Exception, *Helper, *Handler, *Service
//
// EXAMPLES OF CORRECT NAMING:
// - HealthCheckOptions (not CHealthCheckOptions)
// - HealthCheckContext (not CHealthCheckContext)
// - ProcessRunner (not CProcessRunner)
// - LogSanitizer (not CLogSanitizer)
// - CredentialHelper (not CCredentialHelper)
//
// MIGRATION STRATEGY:
// 1. All NEW classes must use the correct naming convention
// 2. Legacy "C" prefixed classes will be migrated gradually
// 3. When refactoring a class, consider renaming it
// 4. Use 'using' aliases for backward compatibility during migration
//
// =============================================================================

namespace VeeamHealthCheck.Shared
{
    // Type aliases for gradual migration
    // These allow new code to use clean names while maintaining compatibility
    
    // Example usage in consuming code:
    // using Logger = VeeamHealthCheck.Shared.Logging.CLogger;
    // using Globals = VeeamHealthCheck.Shared.CGlobals;
}

namespace VeeamHealthCheck.Functions.Reporting.CsvHandlers
{
    // Future type aliases for CSV parsing classes
    // using CsvParser = CCsvParser;
    // using CsvReader = CCsvReader;
}
