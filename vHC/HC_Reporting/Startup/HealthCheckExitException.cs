// Copyright (c) 2021, Adam Congdon <adam.congdon2@gmail.com>
// MIT License
using System;

namespace VeeamHealthCheck.Startup
{
    /// <summary>
    /// Exception thrown to signal a controlled program exit.
    /// This replaces Environment.Exit() calls to allow proper cleanup
    /// and testability.
    /// </summary>
    public class HealthCheckExitException : Exception
    {
        /// <summary>
        /// The exit code to return when the application terminates.
        /// </summary>
        public int ExitCode { get; }

        /// <summary>
        /// Indicates whether the exit was due to an error condition.
        /// </summary>
        public bool IsError => ExitCode != 0;

        /// <summary>
        /// Creates a new exit exception with the specified exit code.
        /// </summary>
        /// <param name="exitCode">The exit code (0 = success, non-zero = error).</param>
        public HealthCheckExitException(int exitCode)
            : base(exitCode == 0 ? "Application exit requested" : "Application exit due to error")
        {
            ExitCode = exitCode;
        }

        /// <summary>
        /// Creates a new exit exception with exit code and message.
        /// </summary>
        /// <param name="exitCode">The exit code (0 = success, non-zero = error).</param>
        /// <param name="message">The reason for the exit.</param>
        public HealthCheckExitException(int exitCode, string message)
            : base(message)
        {
            ExitCode = exitCode;
        }

        /// <summary>
        /// Creates a new exit exception wrapping an inner exception.
        /// </summary>
        /// <param name="exitCode">The exit code.</param>
        /// <param name="message">The reason for the exit.</param>
        /// <param name="innerException">The underlying exception.</param>
        public HealthCheckExitException(int exitCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ExitCode = exitCode;
        }
    }
}
