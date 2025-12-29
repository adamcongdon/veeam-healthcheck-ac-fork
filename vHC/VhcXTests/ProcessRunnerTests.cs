// Copyright (c) Veeam Software Group GmbH
// Licensed under MIT

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using VeeamHealthCheck.Functions.Collection.PSCollections;
using Xunit;

namespace VhcXTests
{
    /// <summary>
    /// Tests for ProcessRunner async process execution utility.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Category", "Async")]
    public class ProcessRunnerTests
    {
        #region ProcessResult Tests

        [Fact]
        public void ProcessResult_WithExitCodeZero_ReportsSuccess()
        {
            // Arrange & Act
            var result = new ProcessResult(0, "output", "", 100);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(0, result.ExitCode);
            Assert.Equal("output", result.StandardOutput);
            Assert.Equal("", result.StandardError);
        }

        [Fact]
        public void ProcessResult_WithNonZeroExitCode_ReportsFailure()
        {
            // Arrange & Act
            var result = new ProcessResult(1, "", "error message", 50);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(1, result.ExitCode);
            Assert.Equal("error message", result.StandardError);
        }

        [Fact]
        public void ProcessResult_PreservesAllProperties()
        {
            // Arrange
            int exitCode = 42;
            string stdout = "standard output";
            string stderr = "standard error";
            long elapsedMs = 1234;

            // Act
            var result = new ProcessResult(exitCode, stdout, stderr, elapsedMs);

            // Assert
            Assert.Equal(exitCode, result.ExitCode);
            Assert.Equal(stdout, result.StandardOutput);
            Assert.Equal(stderr, result.StandardError);
            Assert.Equal(elapsedMs, result.ElapsedMilliseconds);
            Assert.False(result.Success);
        }

        #endregion

        #region RunAsync Tests

        [Fact]
        [Trait("Category", "Integration")]
        public async Task RunAsync_WithValidCommand_ReturnsOutput()
        {
            // Arrange
            var runner = new ProcessRunner();
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c echo hello",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            // Act
            var result = await runner.RunAsync(startInfo);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("hello", result.StandardOutput);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task RunAsync_WithFailingCommand_ReturnsNonZeroExitCode()
        {
            // Arrange
            var runner = new ProcessRunner();
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c exit 1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            // Act
            var result = await runner.RunAsync(startInfo);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(1, result.ExitCode);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task RunAsync_WithCancellation_ThrowsTaskCanceledException()
        {
            // Arrange
            var runner = new ProcessRunner();
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c ping -n 10 127.0.0.1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(100));

            // Act & Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => runner.RunAsync(startInfo, cancellationToken: cts.Token));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task RunAsync_WithTimeout_ThrowsTimeoutException()
        {
            // Arrange
            var runner = new ProcessRunner();
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c ping -n 10 127.0.0.1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            // Act & Assert
            await Assert.ThrowsAsync<TimeoutException>(
                () => runner.RunAsync(startInfo, timeout: TimeSpan.FromMilliseconds(100)));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task RunAsync_CapturesStandardError()
        {
            // Arrange
            var runner = new ProcessRunner();
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c echo error_message 1>&2",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            // Act
            var result = await runner.RunAsync(startInfo);

            // Assert
            Assert.Contains("error_message", result.StandardError);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task RunAsync_TracksElapsedTime()
        {
            // Arrange
            var runner = new ProcessRunner();
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c echo quick",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            // Act
            var result = await runner.RunAsync(startInfo);

            // Assert
            Assert.True(result.ElapsedMilliseconds >= 0);
        }

        #endregion

        #region Timeout Handling

        [Fact]
        [Trait("Category", "Integration")]
        public async Task RunAsync_DefaultTimeout_CompletesSuccessfully()
        {
            // Arrange
            var runner = new ProcessRunner();
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c echo fast",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            // Act - default timeout should be long enough for this
            var result = await runner.RunAsync(startInfo);

            // Assert
            Assert.True(result.Success);
        }

        #endregion
    }
}
