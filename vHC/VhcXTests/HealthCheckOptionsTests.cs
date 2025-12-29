// Copyright (c) Veeam Software Group GmbH
// Licensed under MIT

using System;
using VeeamHealthCheck.Shared;
using Xunit;

namespace VhcXTests
{
    /// <summary>
    /// Tests for HealthCheckOptions configuration class.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Category", "Configuration")]
    public class HealthCheckOptionsTests
    {
        #region Default Values Tests

        [Fact]
        public void DefaultOptions_HasCorrectDefaults()
        {
            // Arrange & Act
            var options = new HealthCheckOptions();

            // Assert
            Assert.Equal(7, options.ReportDays);
            Assert.False(options.Scrub);
            Assert.False(options.ExportPdf);
            Assert.False(options.ExportPptx);
            Assert.False(options.OpenHtmlAfterGeneration);
            Assert.False(options.OpenExplorerAfterGeneration);
            Assert.Equal("localhost", options.TargetServer);
            Assert.False(options.IsRemoteExecution);
        }

        #endregion

        #region Property Setting Tests

        [Theory]
        [InlineData(7)]
        [InlineData(30)]
        [InlineData(90)]
        public void ReportDays_CanBeSet(int days)
        {
            // Arrange
            var options = new HealthCheckOptions();

            // Act
            options.ReportDays = days;

            // Assert
            Assert.Equal(days, options.ReportDays);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Scrub_CanBeToggled(bool scrub)
        {
            // Arrange
            var options = new HealthCheckOptions();

            // Act
            options.Scrub = scrub;

            // Assert
            Assert.Equal(scrub, options.Scrub);
        }

        [Fact]
        public void TargetServer_CanBeSet()
        {
            // Arrange
            var options = new HealthCheckOptions();
            string server = "remote-server.domain.com";

            // Act
            options.TargetServer = server;

            // Assert
            Assert.Equal(server, options.TargetServer);
        }

        [Fact]
        public void IsRemoteExecution_TrueWhenServerNotLocalhost()
        {
            // Arrange
            var options = new HealthCheckOptions
            {
                TargetServer = "remote-server"
            };

            // Act & Assert
            Assert.True(options.IsRemoteExecution);
        }

        [Fact]
        public void IsRemoteExecution_FalseWhenServerIsLocalhost()
        {
            // Arrange
            var options = new HealthCheckOptions
            {
                TargetServer = "localhost"
            };

            // Act & Assert
            Assert.False(options.IsRemoteExecution);
        }

        #endregion

        #region ReportType Tests

        [Fact]
        public void ReportType_VbrFull_SetsCorrectFlags()
        {
            // Arrange
            var options = new HealthCheckOptions();

            // Act
            options.ReportType = ReportType.VbrFull;

            // Assert
            Assert.Equal(ReportType.VbrFull, options.ReportType);
        }

        [Fact]
        public void ReportType_Vb365_SetsCorrectFlags()
        {
            // Arrange
            var options = new HealthCheckOptions();

            // Act
            options.ReportType = ReportType.Vb365;

            // Assert
            Assert.Equal(ReportType.Vb365, options.ReportType);
        }

        [Fact]
        public void ReportType_VbrSecurity_SetsCorrectFlags()
        {
            // Arrange
            var options = new HealthCheckOptions();

            // Act
            options.ReportType = ReportType.VbrSecurity;

            // Assert
            Assert.Equal(ReportType.VbrSecurity, options.ReportType);
        }

        #endregion

        #region Validation Tests

        [Fact]
        public void Validate_WithValidOptions_ReturnsTrue()
        {
            // Arrange
            var options = new HealthCheckOptions
            {
                ReportType = ReportType.VbrFull,
                ReportDays = 7
            };

            // Act
            bool isValid = options.Validate(out string error);

            // Assert
            Assert.True(isValid);
            Assert.Null(error);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidReportDays_ReturnsFalse(int days)
        {
            // Arrange
            var options = new HealthCheckOptions
            {
                ReportType = ReportType.VbrFull,
                ReportDays = days
            };

            // Act
            bool isValid = options.Validate(out string error);

            // Assert
            Assert.False(isValid);
            Assert.NotNull(error);
            Assert.Contains("ReportDays", error);
        }

        #endregion
    }
}
