// Copyright (c) 2021, Adam Congdon <adam.congdon2@gmail.com>
// MIT License
using System;
using System.Text.RegularExpressions;

namespace VeeamHealthCheck.Functions.Collection.Security
{
    /// <summary>
    /// Provides methods to sanitize log messages by masking sensitive data.
    /// </summary>
    public static class LogSanitizer
    {
        private const string MaskValue = "****";

        /// <summary>
        /// Masks a specific sensitive value in a log message.
        /// </summary>
        public static string MaskSensitiveValue(string message, string sensitiveValue)
        {
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(sensitiveValue))
                return message ?? string.Empty;

            return message.Replace(sensitiveValue, MaskValue);
        }

        /// <summary>
        /// Masks multiple sensitive values in a log message.
        /// </summary>
        public static string MaskSensitiveValues(string message, params string[] sensitiveValues)
        {
            if (string.IsNullOrEmpty(message))
                return message ?? string.Empty;

            foreach (var value in sensitiveValues)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    message = message.Replace(value, MaskValue);
                }
            }

            return message;
        }

        /// <summary>
        /// Masks password patterns in PowerShell command strings.
        /// Detects -Password and -PasswordBase64 parameters.
        /// </summary>
        public static string MaskPowerShellPasswords(string commandString)
        {
            if (string.IsNullOrEmpty(commandString))
                return commandString ?? string.Empty;

            // Mask -Password 'value' or -Password "value"
            var passwordPattern = @"(-Password\s+['""])([^'""]+)(['""])";
            commandString = Regex.Replace(commandString, passwordPattern, $"$1{MaskValue}$3", RegexOptions.IgnoreCase);

            // Mask -PasswordBase64 "value"
            var base64Pattern = @"(-PasswordBase64\s+['""])([^'""]+)(['""])";
            commandString = Regex.Replace(commandString, base64Pattern, $"$1{MaskValue}$3", RegexOptions.IgnoreCase);

            return commandString;
        }

        /// <summary>
        /// Creates a sanitized version of ProcessStartInfo.Arguments for logging.
        /// </summary>
        public static string SanitizeProcessArguments(string arguments, params string[] sensitiveValues)
        {
            if (string.IsNullOrEmpty(arguments))
                return arguments ?? string.Empty;

            var sanitized = MaskPowerShellPasswords(arguments);
            sanitized = MaskSensitiveValues(sanitized, sensitiveValues);

            return sanitized;
        }
    }
}
