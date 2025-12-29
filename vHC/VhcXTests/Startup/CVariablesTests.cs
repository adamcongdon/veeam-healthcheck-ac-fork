// Copyright (c) 2021, Adam Congdon <adam.congdon2@gmail.com>
// MIT License
using System;
using System.IO;
using VeeamHealthCheck;
using VeeamHealthCheck.Shared;
using Xunit;

namespace VhcXTests.Startup
{
    /// <summary>
    /// Tests for <see cref="CVariables"/> path configuration.
    /// </summary>
    [Trait("Category", "Unit")]
    public class CVariablesTests : IDisposable
    {
        private readonly string _originalDesiredPath;
        private readonly string _originalVbrServerName;
        private readonly string _originalRunTimestamp;

        public CVariablesTests()
        {
            // Save original values
            _originalDesiredPath = CGlobals.desiredPath;
            _originalVbrServerName = CGlobals.VBRServerName;
            _originalRunTimestamp = CGlobals.GetRunTimestamp();
        }

        public void Dispose()
        {
            // Restore original values
            CGlobals.desiredPath = _originalDesiredPath;
            CGlobals.VBRServerName = _originalVbrServerName;
        }

        #region Static Path Tests

        [Fact]
        public void OutDir_HasDefaultValue()
        {
            // Assert
            Assert.Equal(@"C:\temp\vHC", CVariables.outDir);
        }

        [Fact]
        public void VbrDir_ContainsVBR()
        {
            // Assert
            Assert.Contains("VBR", CVariables.VbrDir);
        }

        [Fact]
        public void Vb365Dir_ContainsVB365()
        {
            // Assert
            Assert.Contains("VB365", CVariables.vb365Dir);
        }

        [Fact]
        public void SafeSuffix_ContainsAnonymous()
        {
            // Assert
            Assert.Contains("Anonymous", CVariables.safeSuffix);
        }

        [Fact]
        public void UnsafeSuffix_ContainsReport()
        {
            // Assert
            Assert.Contains("Report", CVariables.unsafeSuffix);
        }

        #endregion

        #region SafeDir Tests

        [Fact]
        public void SafeDir_WhenDesiredPathNull_UsesDefaultOutDir()
        {
            // Arrange
            CGlobals.desiredPath = null;

            // Act
            var result = CVariables.safeDir;

            // Assert
            Assert.StartsWith(CVariables.outDir, result);
            Assert.Contains("Anonymous", result);
        }

        [Fact]
        public void SafeDir_WhenDesiredPathSet_UsesDesiredPath()
        {
            // Arrange
            CGlobals.desiredPath = @"D:\CustomPath";

            // Act
            var result = CVariables.safeDir;

            // Assert
            Assert.StartsWith(@"D:\CustomPath", result);
            Assert.Contains("Anonymous", result);
        }

        #endregion

        #region UnsafeDir Tests

        [Fact]
        public void UnsafeDir_WhenDesiredPathNull_UsesDefaultOutDir()
        {
            // Arrange
            CGlobals.desiredPath = null;

            // Act
            var result = CVariables.unsafeDir;

            // Assert
            Assert.StartsWith(CVariables.outDir, result);
            Assert.Contains("Original", result);
        }

        [Fact]
        public void UnsafeDir_WhenDesiredPathSet_UsesDesiredPath()
        {
            // Arrange
            CGlobals.desiredPath = @"D:\CustomPath";

            // Act
            var result = CVariables.unsafeDir;

            // Assert
            Assert.StartsWith(@"D:\CustomPath", result);
            Assert.Contains("Original", result);
        }

        #endregion

        #region VbrDir Tests

        [Fact]
        public void VbrDir_ContainsServerName()
        {
            // Arrange
            CGlobals.VBRServerName = "TestServer";

            // Act
            var result = CVariables.vbrDir;

            // Assert
            Assert.Contains("TestServer", result);
        }

        [Fact]
        public void VbrDir_WhenServerNameEmpty_UsesLocalhost()
        {
            // Arrange
            CGlobals.VBRServerName = "";

            // Act
            var result = CVariables.vbrDir;

            // Assert
            Assert.Contains("localhost", result);
        }

        [Fact]
        public void VbrDir_ContainsTimestamp()
        {
            // Act
            var result = CVariables.vbrDir;
            var timestamp = CGlobals.GetRunTimestamp();

            // Assert
            Assert.Contains(timestamp, result);
        }

        #endregion

        #region GetVbrBaseDir Tests

        [Fact]
        public void GetVbrBaseDir_ReturnsPathWithVBR()
        {
            // Act
            var result = CVariables.GetVbrBaseDir();

            // Assert
            Assert.Contains("VBR", result);
            Assert.Contains("Original", result);
        }

        [Fact]
        public void GetVbrBaseDir_DoesNotContainTimestamp()
        {
            // Arrange
            CGlobals.VBRServerName = "TestServer";
            
            // Act
            var result = CVariables.GetVbrBaseDir();
            var timestamp = CGlobals.GetRunTimestamp();

            // Assert - Base dir should NOT have timestamp
            Assert.DoesNotContain(timestamp, result);
            Assert.DoesNotContain("TestServer", result); // Nor server name
        }

        #endregion

        #region Vb365Dir Tests

        [Fact]
        public void Vb365dir_CombinesUnsafeDirAndVb365Dir()
        {
            // Arrange
            CGlobals.desiredPath = null;

            // Act
            var result = CVariables.vb365dir;

            // Assert
            Assert.Contains("Original", result);
            Assert.Contains("VB365", result);
        }

        #endregion

        #region Instance Method Tests

        [Fact]
        public void UnSafeDir2_InstanceMethod_ReturnsUnsafeDir()
        {
            // Arrange
            var variables = new CVariables();

            // Act
            var result = variables.unSafeDir2();

            // Assert
            Assert.Equal(CVariables.unsafeDir, result);
        }

        #endregion

        #region Path Validation Tests

        [Fact]
        public void Paths_UseCorrectDirectorySeparator()
        {
            // Act
            var safeDir = CVariables.safeDir;
            var unsafeDir = CVariables.unsafeDir;

            // Assert - Should use Path.Combine which uses correct separator
            Assert.DoesNotContain("//", safeDir);
            Assert.DoesNotContain("//", unsafeDir);
        }

        #endregion
    }
}
