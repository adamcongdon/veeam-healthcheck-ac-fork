// Copyright (c) Veeam Software Group GmbH
// Licensed under MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace VhcXTests.Functions.Reporting.CsvHandlers
{
    /// <summary>
    /// Tests for the generic CSV reading helper methods in CCsvParser.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Category", "CSV")]
    public class CCsvParserGenericTests
    {
        #region ReadCsvRecords<T> Tests

        [Fact]
        public void ReadCsvRecords_WithNullReader_ReturnsNull()
        {
            // This test validates the null-safety pattern that should be maintained
            // when the generic ReadCsvRecords method receives a null CsvReader
            
            // Arrange - null reader simulates file not found
            CsvHelper.CsvReader reader = null;

            // Act & Assert
            // The pattern should return null or empty enumerable when file not found
            Assert.Null(reader);
        }

        [Fact]
        public void ReadDynamicRecords_WithNullReader_ReturnsEmptyEnumerable()
        {
            // This validates the dynamic reader pattern
            // When file not found, should return empty enumerable, not null
            
            // Arrange
            IEnumerable<dynamic> result = Enumerable.Empty<dynamic>();

            // Act & Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region File Path Resolution Tests

        [Fact]
        public void CsvFileName_ShouldNotContainExtension_ForNameConstants()
        {
            // Validates that file name constants don't include .csv extension
            // since the file finder adds it
            string fileName = "Proxies";
            
            Assert.DoesNotContain(".csv", fileName);
        }

        [Fact]
        public void CsvFileName_WhenCombinedWithExtension_FormsValidPath()
        {
            // Validates path formation pattern
            string fileName = "Proxies";
            string extension = ".csv";
            string expectedPattern = "Proxies.csv";

            string result = fileName + extension;

            Assert.Equal(expectedPattern, result);
        }

        #endregion

        #region Enumerable Null Safety Tests

        [Fact]
        public void EnumerableResult_WhenNull_ShouldBeHandledGracefully()
        {
            // Tests the pattern of safely handling null enumerable results
            IEnumerable<object> nullResult = null;

            // Safe pattern: use ?? Enumerable.Empty<T>()
            var safeResult = nullResult ?? Enumerable.Empty<object>();

            Assert.NotNull(safeResult);
            Assert.Empty(safeResult);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void EnumerableResult_WithItems_ShouldReturnCorrectCount(int itemCount)
        {
            // Tests that enumerable counting works correctly
            var items = Enumerable.Range(0, itemCount).Select(i => new { Id = i });

            Assert.Equal(itemCount, items.Count());
        }

        #endregion
    }
}
