// Copyright (c) Veeam Software Group GmbH
// Licensed under MIT

using System;
using VeeamHealthCheck.Shared;
using Xunit;

namespace VhcXTests
{
    /// <summary>
    /// Tests for HealthCheckContext runtime state class.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Category", "Context")]
    public class HealthCheckContextTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_WithDefaultOptions_InitializesCorrectly()
        {
            // Arrange & Act
            using var context = new HealthCheckContext();

            // Assert
            Assert.NotNull(context.Options);
            Assert.NotNull(context.Logger);
            Assert.True(context.StartTime <= DateTime.Now);
            Assert.NotNull(context.RunTimestamp);
        }

        [Fact]
        public void Constructor_WithCustomOptions_UsesProvidedOptions()
        {
            // Arrange
            var options = new HealthCheckOptions
            {
                ReportDays = 30,
                Scrub = true
            };

            // Act
            using var context = new HealthCheckContext(options);

            // Assert
            Assert.Equal(30, context.Options.ReportDays);
            Assert.True(context.Options.Scrub);
        }

        [Fact]
        public void Constructor_WithNullOptions_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new HealthCheckContext(null));
        }

        #endregion

        #region RunTimestamp Tests

        [Fact]
        public void RunTimestamp_HasCorrectFormat()
        {
            // Arrange
            using var context = new HealthCheckContext();

            // Act
            var timestamp = context.RunTimestamp;

            // Assert
            Assert.Matches(@"^\d{8}_\d{6}$", timestamp);
        }

        [Fact]
        public void RunTimestamp_MatchesStartTime()
        {
            // Arrange
            using var context = new HealthCheckContext();

            // Act
            var expected = context.StartTime.ToString("yyyyMMdd_HHmmss");

            // Assert
            Assert.Equal(expected, context.RunTimestamp);
        }

        #endregion

        #region Current Static Property Tests

        [Fact]
        public void Current_WhenNotSet_CreatesDefaultContext()
        {
            // Arrange
            HealthCheckContext.Current = null;

            // Act
            var current = HealthCheckContext.Current;

            // Assert
            Assert.NotNull(current);
        }

        [Fact]
        public void Current_CanBeSetExplicitly()
        {
            // Arrange
            var options = new HealthCheckOptions { ReportDays = 90 };
            using var context = new HealthCheckContext(options);

            // Act
            HealthCheckContext.Current = context;

            // Assert
            Assert.Same(context, HealthCheckContext.Current);
            Assert.Equal(90, HealthCheckContext.Current.Options.ReportDays);
        }

        #endregion

        #region Collections Tests

        [Fact]
        public void CsvValidationResults_IsInitializedEmpty()
        {
            // Arrange & Act
            using var context = new HealthCheckContext();

            // Assert
            Assert.NotNull(context.CsvValidationResults);
            Assert.Empty(context.CsvValidationResults);
        }

        [Fact]
        public void ServerInfo_IsInitializedEmpty()
        {
            // Arrange & Act
            using var context = new HealthCheckContext();

            // Assert
            Assert.NotNull(context.ServerInfo);
            Assert.Empty(context.ServerInfo);
        }

        [Fact]
        public void DefaultRegistryKeys_IsInitializedEmpty()
        {
            // Arrange & Act
            using var context = new HealthCheckContext();

            // Assert
            Assert.NotNull(context.DefaultRegistryKeys);
            Assert.Empty(context.DefaultRegistryKeys);
        }

        #endregion

        #region Database Properties Tests

        [Fact]
        public void DatabaseProperties_CanBeSetAndRetrieved()
        {
            // Arrange
            using var context = new HealthCheckContext();

            // Act
            context.DbType = "MS SQL";
            context.DbName = "VeeamBackup";
            context.DbHostname = "sql-server.domain.com";
            context.DbVersion = "15.0";
            context.DbCores = 8;
            context.DbRam = 32768;

            // Assert
            Assert.Equal("MS SQL", context.DbType);
            Assert.Equal("VeeamBackup", context.DbName);
            Assert.Equal("sql-server.domain.com", context.DbHostname);
            Assert.Equal("15.0", context.DbVersion);
            Assert.Equal(8, context.DbCores);
            Assert.Equal(32768, context.DbRam);
        }

        #endregion

        #region Dispose Tests

        [Fact]
        public void Dispose_ClearsCollections()
        {
            // Arrange
            var context = new HealthCheckContext();
            context.CsvValidationResults.Add(new CsvValidationResult("test", true, "test.csv"));
            context.DefaultRegistryKeys["key"] = "value";

            // Act
            context.Dispose();

            // Assert
            Assert.Empty(context.CsvValidationResults);
            Assert.Empty(context.DefaultRegistryKeys);
        }

        [Fact]
        public void Dispose_ClearsCurrentIfSameInstance()
        {
            // Arrange
            var context = new HealthCheckContext();
            HealthCheckContext.Current = context;

            // Act
            context.Dispose();

            // Assert - Current should be null or a new instance
            // Note: accessing Current after dispose may create a new one
            Assert.NotSame(context, HealthCheckContext.Current);
        }

        #endregion
    }
}
