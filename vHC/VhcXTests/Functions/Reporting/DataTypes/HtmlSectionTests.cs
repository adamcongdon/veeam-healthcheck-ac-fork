// Copyright (c) 2021, Adam Congdon <adam.congdon2@gmail.com>
// MIT License
using System.Collections.Generic;
using VeeamHealthCheck.Functions.Reporting.DataTypes;
using Xunit;

namespace VhcXTests.Functions.Reporting.DataTypes
{
    /// <summary>
    /// Tests for <see cref="HtmlSection"/> data transfer object.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Category", "DataTypes")]
    public class HtmlSectionTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_InitializesWithEmptyLists()
        {
            // Arrange & Act
            var section = new HtmlSection();

            // Assert
            Assert.NotNull(section.Headers);
            Assert.NotNull(section.Rows);
            Assert.Empty(section.Headers);
            Assert.Empty(section.Rows);
        }

        [Fact]
        public void Constructor_PropertiesAreNull()
        {
            // Arrange & Act
            var section = new HtmlSection();

            // Assert
            Assert.Null(section.SectionName);
            Assert.Null(section.Summary);
        }

        #endregion

        #region Property Tests

        [Fact]
        public void SectionName_CanBeSet()
        {
            // Arrange
            var section = new HtmlSection();

            // Act
            section.SectionName = "Test Section";

            // Assert
            Assert.Equal("Test Section", section.SectionName);
        }

        [Fact]
        public void Summary_CanBeSet()
        {
            // Arrange
            var section = new HtmlSection();

            // Act
            section.Summary = "This is a summary";

            // Assert
            Assert.Equal("This is a summary", section.Summary);
        }

        [Fact]
        public void Headers_CanAddItems()
        {
            // Arrange
            var section = new HtmlSection();

            // Act
            section.Headers.Add("Column1");
            section.Headers.Add("Column2");

            // Assert
            Assert.Equal(2, section.Headers.Count);
            Assert.Contains("Column1", section.Headers);
            Assert.Contains("Column2", section.Headers);
        }

        [Fact]
        public void Rows_CanAddItems()
        {
            // Arrange
            var section = new HtmlSection();
            var row1 = new List<string> { "Value1", "Value2" };
            var row2 = new List<string> { "Value3", "Value4" };

            // Act
            section.Rows.Add(row1);
            section.Rows.Add(row2);

            // Assert
            Assert.Equal(2, section.Rows.Count);
            Assert.Equal("Value1", section.Rows[0][0]);
            Assert.Equal("Value4", section.Rows[1][1]);
        }

        #endregion

        #region Data Integrity Tests

        [Fact]
        public void FullSection_CanBeCreated()
        {
            // Arrange & Act
            var section = new HtmlSection
            {
                SectionName = "Servers",
                Summary = "2 servers found",
                Headers = new List<string> { "Name", "IP", "Status" },
                Rows = new List<List<string>>
                {
                    new List<string> { "Server1", "192.168.1.1", "Online" },
                    new List<string> { "Server2", "192.168.1.2", "Offline" }
                }
            };

            // Assert
            Assert.Equal("Servers", section.SectionName);
            Assert.Equal("2 servers found", section.Summary);
            Assert.Equal(3, section.Headers.Count);
            Assert.Equal(2, section.Rows.Count);
        }

        [Fact]
        public void Rows_CanHaveDifferentLengths()
        {
            // Arrange
            var section = new HtmlSection();

            // Act
            section.Rows.Add(new List<string> { "Single" });
            section.Rows.Add(new List<string> { "One", "Two", "Three" });

            // Assert
            Assert.Single(section.Rows[0]);
            Assert.Equal(3, section.Rows[1].Count);
        }

        #endregion
    }
}
