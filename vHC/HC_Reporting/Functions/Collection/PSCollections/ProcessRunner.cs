// Copyright (c) Veeam Software Group GmbH
// Licensed under MIT

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace VeeamHealthCheck.Functions.Collection.PSCollections
{
    /// <summary>
    /// Result of an async process execution.
    /// </summary>
    public sealed record ProcessResult(
        int ExitCode,
        string StandardOutput,
        string StandardError,
        long ElapsedMilliseconds)
    {
        /// <summary>
        /// Gets a value indicating whether the process exited successfully (exit code 0).
        /// </summary>
        public bool Success => this.ExitCode == 0;
    }

    /// <summary>
    /// Async wrapper for process execution to avoid blocking the UI thread.
    /// </summary>
    /// <remarks>
    /// This class provides async methods for running external processes,
    /// replacing synchronous WaitForExit() calls that block the calling thread.
    /// </remarks>
    public class ProcessRunner : IProcessRunner
    {
        /// <summary>
        /// Default timeout for process execution (5 minutes).
        /// </summary>
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Runs a process asynchronously and captures output.
        /// </summary>
        /// <param name="startInfo">The process start information.</param>
        /// <param name="timeout">Optional timeout. Defaults to 5 minutes.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A ProcessResult containing exit code, output, and timing.</returns>
        /// <exception cref="TimeoutException">Thrown when the process exceeds the timeout.</exception>
        /// <exception cref="OperationCanceledException">Thrown when cancellation is requested.</exception>
        public async Task<ProcessResult> RunAsync(
            ProcessStartInfo startInfo,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            timeout ??= DefaultTimeout;

            // Ensure we can capture output
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            var stopwatch = Stopwatch.StartNew();

            using var process = new Process { StartInfo = startInfo };
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Set up timeout
            linkedCts.CancelAfter(timeout.Value);

            // Start the process
            process.Start();

            try
            {
                // Read output streams asynchronously to prevent deadlocks
                var stdOutTask = process.StandardOutput.ReadToEndAsync(linkedCts.Token);
                var stdErrTask = process.StandardError.ReadToEndAsync(linkedCts.Token);

                // Wait for process to exit asynchronously
                await process.WaitForExitAsync(linkedCts.Token).ConfigureAwait(false);

                // Get the output
                string stdOut = await stdOutTask.ConfigureAwait(false);
                string stdErr = await stdErrTask.ConfigureAwait(false);

                stopwatch.Stop();

                return new ProcessResult(
                    process.ExitCode,
                    stdOut,
                    stdErr,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Timeout occurred (not user cancellation)
                stopwatch.Stop();

                // Try to kill the process
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch
                {
                    // Process may have already exited
                }

                throw new TimeoutException(
                    $"Process execution exceeded timeout of {timeout.Value.TotalSeconds} seconds.");
            }
            catch (OperationCanceledException)
            {
                // User requested cancellation
                stopwatch.Stop();

                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch
                {
                    // Process may have already exited
                }

                throw;
            }
        }
    }

    /// <summary>
    /// Interface for async process execution. Enables mocking in tests.
    /// </summary>
    public interface IProcessRunner
    {
        /// <summary>
        /// Runs a process asynchronously and captures output.
        /// </summary>
        Task<ProcessResult> RunAsync(
            ProcessStartInfo startInfo,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);
    }
}
