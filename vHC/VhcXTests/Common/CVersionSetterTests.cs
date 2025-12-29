// Copyright (c) 2021, Adam Congdon <adam.congdon2@gmail.com>
// MIT License
using VeeamHealthCheck.Shared;
using Xunit;

namespace VhcXTests.Common
{
    /// <summary>
    /// Tests for <see cref="CVersionSetter"/> version detection.
    /// </summary>
    [Trait("Category", "Unit")]
    public class CVersionSetterTests
    {
        [Fact]
        public void GetFileVersion_ReturnsNonNullValue()
        {
            // Act
            var result = CVersionSetter.GetFileVersion();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetFileVersion_SetsGlobalVhcVersion()
        {
            // Arrange
            CGlobals.VHCVERSION = null;

            // Act
            var result = CVersionSetter.GetFileVersion();

            // Assert
            Assert.Equal(result, CGlobals.VHCVERSION);
        }

        [Fact]
        public void GetFileVersion_ReturnsSameValueOnMultipleCalls()
        {
            // Act
            var firstCall = CVersionSetter.GetFileVersion();
            var secondCall = CVersionSetter.GetFileVersion();

            // Assert
            Assert.Equal(firstCall, secondCall);
        }

        [Fact]
        public void GetFileVersion_ContainsVersionNumbers()
        {
            // Act
            var result = CVersionSetter.GetFileVersion();

            // Assert
            // Version should contain at least one digit
            Assert.Matches(@"\d", result);
        }
    }
}
