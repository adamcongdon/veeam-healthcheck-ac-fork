// Copyright (c) 2021, Adam Congdon <adam.congdon2@gmail.com>
// MIT License
using Xunit;
using VeeamHealthCheck.Startup;

namespace VeeamHealthCheck.Tests
{
    [Trait("Category", "ExceptionHandling")]
    public class HealthCheckExitExceptionTests
    {
        [Fact]
        public void Constructor_WithZeroExitCode_ShouldNotBeError()
        {
            var ex = new HealthCheckExitException(0);
            
            Assert.Equal(0, ex.ExitCode);
            Assert.False(ex.IsError);
        }

        [Fact]
        public void Constructor_WithNonZeroExitCode_ShouldBeError()
        {
            var ex = new HealthCheckExitException(1);
            
            Assert.Equal(1, ex.ExitCode);
            Assert.True(ex.IsError);
        }

        [Fact]
        public void Constructor_WithMessage_ShouldPreserveMessage()
        {
            var ex = new HealthCheckExitException(1, "Test error message");
            
            Assert.Equal("Test error message", ex.Message);
            Assert.Equal(1, ex.ExitCode);
        }

        [Fact]
        public void Constructor_WithInnerException_ShouldPreserveInnerException()
        {
            var inner = new System.InvalidOperationException("Inner error");
            var ex = new HealthCheckExitException(1, "Outer message", inner);
            
            Assert.Equal(inner, ex.InnerException);
            Assert.Equal("Outer message", ex.Message);
        }

        [Fact]
        public void ExitCode_SuccessValues_ShouldNotBeError()
        {
            var ex = new HealthCheckExitException(0, "User requested exit");
            Assert.False(ex.IsError);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(-1)]
        [InlineData(255)]
        public void ExitCode_ErrorValues_ShouldBeError(int exitCode)
        {
            var ex = new HealthCheckExitException(exitCode);
            Assert.True(ex.IsError);
        }
    }
}
