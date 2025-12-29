// Copyright (c) 2021, Adam Congdon <adam.congdon2@gmail.com>
// MIT License
using VeeamHealthCheck;
using Xunit;

namespace VhcXTests.Startup
{
    /// <summary>
    /// Tests for <see cref="CAdminCheck"/> privilege detection.
    /// </summary>
    /// <remarks>
    /// Note: These tests verify the method works without crashing.
    /// The actual admin status depends on the test runner's privileges.
    /// </remarks>
    [Trait("Category", "Unit")]
    public class CAdminCheckTests
    {
        [Fact]
        public void IsAdmin_ReturnsBooleanValue()
        {
            // Arrange
            var adminCheck = new CAdminCheck();

            // Act
            var result = adminCheck.IsAdmin();

            // Assert - Just verify it returns a boolean without exception
            Assert.IsType<bool>(result);
        }

        [Fact]
        public void IsAdmin_MultipleCalls_ReturnConsistentValue()
        {
            // Arrange
            var adminCheck = new CAdminCheck();

            // Act
            var firstCall = adminCheck.IsAdmin();
            var secondCall = adminCheck.IsAdmin();
            var thirdCall = adminCheck.IsAdmin();

            // Assert - Privilege status should be consistent
            Assert.Equal(firstCall, secondCall);
            Assert.Equal(secondCall, thirdCall);
        }

        [Fact]
        public void IsAdmin_NewInstance_ReturnsConsistentValue()
        {
            // Arrange
            var firstCheck = new CAdminCheck();
            var secondCheck = new CAdminCheck();

            // Act
            var firstResult = firstCheck.IsAdmin();
            var secondResult = secondCheck.IsAdmin();

            // Assert - Different instances should return same value
            Assert.Equal(firstResult, secondResult);
        }

        [Fact]
        public void IsAdmin_DoesNotThrowException()
        {
            // Arrange
            var adminCheck = new CAdminCheck();

            // Act & Assert
            var exception = Record.Exception(() => adminCheck.IsAdmin());
            Assert.Null(exception);
        }
    }
}
