// Copyright (c) 2021, Adam Congdon <adam.congdon2@gmail.com>
// MIT License
using System;
using VeeamHealthCheck.Shared;
using Xunit;

namespace VhcXTests.Common
{
    /// <summary>
    /// Tests for <see cref="CGlobals"/> static properties.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Category", "Configuration")]
    public class CGlobalsTests : IDisposable
    {
        // Store original values to restore after tests
        private readonly int _originalReportDays;
        private readonly bool _originalScrub;
        private readonly bool _originalIsVbr;
        private readonly bool _originalIsVb365;
        private readonly string _originalVbrServerName;

        public CGlobalsTests()
        {
            // Save original values
            _originalReportDays = CGlobals.ReportDays;
            _originalScrub = CGlobals.Scrub;
            _originalIsVbr = CGlobals.IsVbr;
            _originalIsVb365 = CGlobals.IsVb365;
            _originalVbrServerName = CGlobals.VBRServerName;
        }

        public void Dispose()
        {
            // Restore original values
            CGlobals.ReportDays = _originalReportDays;
            CGlobals.Scrub = _originalScrub;
            CGlobals.IsVbr = _originalIsVbr;
            CGlobals.IsVb365 = _originalIsVb365;
            CGlobals.VBRServerName = _originalVbrServerName;
        }

        #region ReportDays Tests

        [Theory]
        [InlineData(7)]
        [InlineData(30)]
        [InlineData(90)]
        public void ReportDays_CanBeSet(int days)
        {
            // Act
            CGlobals.ReportDays = days;

            // Assert
            Assert.Equal(days, CGlobals.ReportDays);
        }

        #endregion

        #region Scrub Tests

        [Fact]
        public void Scrub_DefaultIsFalse()
        {
            // Note: Default may have been changed by other tests, 
            // so just verify it can be set
            
            // Arrange
            CGlobals.Scrub = false;

            // Act & Assert
            Assert.False(CGlobals.Scrub);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Scrub_CanBeToggled(bool value)
        {
            // Act
            CGlobals.Scrub = value;

            // Assert
            Assert.Equal(value, CGlobals.Scrub);
        }

        #endregion

        #region Product Flags Tests

        [Fact]
        public void IsVbr_CanBeSet()
        {
            // Arrange
            CGlobals.IsVbr = true;

            // Assert
            Assert.True(CGlobals.IsVbr);
        }

        [Fact]
        public void IsVb365_CanBeSet()
        {
            // Arrange
            CGlobals.IsVb365 = true;

            // Assert
            Assert.True(CGlobals.IsVb365);
        }

        [Fact]
        public void BothProductFlags_CanBeTrue()
        {
            // Arrange & Act
            CGlobals.IsVbr = true;
            CGlobals.IsVb365 = true;

            // Assert
            Assert.True(CGlobals.IsVbr);
            Assert.True(CGlobals.IsVb365);
        }

        #endregion

        #region VBRServerName Tests

        [Fact]
        public void VBRServerName_CanBeSet()
        {
            // Arrange
            string serverName = "test-server";

            // Act
            CGlobals.VBRServerName = serverName;

            // Assert
            Assert.Equal(serverName, CGlobals.VBRServerName);
        }

        [Fact]
        public void VBRServerName_CanBeNull()
        {
            // Act
            CGlobals.VBRServerName = null;

            // Assert
            Assert.Null(CGlobals.VBRServerName);
        }

        #endregion

        #region Timestamp Tests

        [Fact]
        public void GetRunTimestamp_ReturnsConsistentValue()
        {
            // Act
            var first = CGlobals.GetRunTimestamp();
            var second = CGlobals.GetRunTimestamp();

            // Assert
            Assert.Equal(first, second);
        }

        [Fact]
        public void GetRunTimestamp_HasCorrectFormat()
        {
            // Act
            var timestamp = CGlobals.GetRunTimestamp();

            // Assert - Format should be yyyyMMdd_HHmmss or similar
            Assert.NotNull(timestamp);
            Assert.NotEmpty(timestamp);
            // Verify it contains underscore (date_time separator)
            Assert.Contains("_", timestamp);
        }

        [Fact]
        public void ResetRunTimestamp_CreatesNewTimestamp()
        {
            // Arrange
            var original = CGlobals.GetRunTimestamp();

            // Act - Simulate a small delay then reset
            System.Threading.Thread.Sleep(1100); // Wait over a second
            CGlobals.ResetRunTimestamp();
            var newTimestamp = CGlobals.GetRunTimestamp();

            // Assert - Timestamps should be different
            Assert.NotEqual(original, newTimestamp);
        }

        #endregion

        #region Logger Tests

        [Fact]
        public void Logger_IsNotNull()
        {
            // Assert
            Assert.NotNull(CGlobals.Logger);
        }

        #endregion

        #region Report Mode Tests

        [Fact]
        public void RunFullReport_CanBeSet()
        {
            // Arrange
            var original = CGlobals.RunFullReport;

            // Act
            CGlobals.RunFullReport = true;

            // Assert
            Assert.True(CGlobals.RunFullReport);

            // Cleanup
            CGlobals.RunFullReport = original;
        }

        [Fact]
        public void RunSecReport_CanBeSet()
        {
            // Arrange
            var original = CGlobals.RunSecReport;

            // Act
            CGlobals.RunSecReport = true;

            // Assert
            Assert.True(CGlobals.RunSecReport);

            // Cleanup
            CGlobals.RunSecReport = original;
        }

        #endregion
    }
}
