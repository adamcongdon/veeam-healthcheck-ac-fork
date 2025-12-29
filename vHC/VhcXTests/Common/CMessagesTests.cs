// Copyright (c) 2021, Adam Congdon <adam.congdon2@gmail.com>
// MIT License
using System;
using System.Collections.Generic;
using VeeamHealthCheck.Shared;
using Xunit;

namespace VhcXTests.Common
{
    /// <summary>
    /// Tests for <see cref="CMessages"/> formatting methods.
    /// </summary>
    [Trait("Category", "Unit")]
    public class CMessagesTests
    {
        #region HelpMenu Tests

        [Fact]
        public void HelpMenu_ContainsUsageSection()
        {
            // Arrange & Act
            var helpMenu = CMessages.helpMenu;

            // Assert
            Assert.Contains("USAGE:", helpMenu);
            Assert.Contains("VeeamHealthCheck.exe", helpMenu);
        }

        [Fact]
        public void HelpMenu_ContainsBasicCommands()
        {
            // Arrange & Act
            var helpMenu = CMessages.helpMenu;

            // Assert
            Assert.Contains("/run", helpMenu);
            Assert.Contains("/gui", helpMenu);
            Assert.Contains("/help", helpMenu);
        }

        [Fact]
        public void HelpMenu_ContainsReportingOptions()
        {
            // Arrange & Act
            var helpMenu = CMessages.helpMenu;

            // Assert
            Assert.Contains("/days:", helpMenu);
            Assert.Contains("/lite", helpMenu);
            Assert.Contains("/pdf", helpMenu);
            Assert.Contains("/pptx", helpMenu);
            Assert.Contains("/scrub:", helpMenu);
        }

        [Fact]
        public void HelpMenu_ContainsRemoteOperations()
        {
            // Arrange & Act
            var helpMenu = CMessages.helpMenu;

            // Assert
            Assert.Contains("/remote", helpMenu);
            Assert.Contains("/host=", helpMenu);
        }

        [Fact]
        public void HelpMenu_ContainsSpecialModes()
        {
            // Arrange & Act
            var helpMenu = CMessages.helpMenu;

            // Assert
            Assert.Contains("/security", helpMenu);
            Assert.Contains("/import", helpMenu);
            Assert.Contains("/hotfix", helpMenu);
        }

        [Fact]
        public void HelpMenu_ContainsExamples()
        {
            // Arrange & Act
            var helpMenu = CMessages.helpMenu;

            // Assert
            Assert.Contains("EXAMPLES:", helpMenu);
        }

        #endregion

        #region FoundHotfixesMessage Tests

        [Fact]
        public void FoundHotfixesMessage_EmptyList_ReturnsMessageWithZeroCount()
        {
            // Arrange
            var fixes = new List<string>();

            // Act
            var result = CMessages.FoundHotfixesMessage(fixes);

            // Assert
            Assert.Contains("found 0 hotfixes", result);
        }

        [Fact]
        public void FoundHotfixesMessage_SingleFix_IncludesFixInOutput()
        {
            // Arrange
            var fixes = new List<string> { "KB12345-SomeHotfix" };

            // Act
            var result = CMessages.FoundHotfixesMessage(fixes);

            // Assert
            Assert.Contains("found 1 hotfixes", result);
            Assert.Contains("KB12345-SomeHotfix", result);
        }

        [Fact]
        public void FoundHotfixesMessage_MultipleFixes_IncludesAllFixes()
        {
            // Arrange
            var fixes = new List<string> 
            { 
                "KB12345-FirstHotfix",
                "KB67890-SecondHotfix",
                "KB11111-ThirdHotfix"
            };

            // Act
            var result = CMessages.FoundHotfixesMessage(fixes);

            // Assert
            Assert.Contains("found 3 hotfixes", result);
            Assert.Contains("KB12345-FirstHotfix", result);
            Assert.Contains("KB67890-SecondHotfix", result);
            Assert.Contains("KB11111-ThirdHotfix", result);
        }

        [Fact]
        public void FoundHotfixesMessage_ContainsSupportInstructions()
        {
            // Arrange
            var fixes = new List<string> { "TestFix" };

            // Act
            var result = CMessages.FoundHotfixesMessage(fixes);

            // Assert
            Assert.Contains("Open a support case", result);
            Assert.Contains("Hotfix Detector Results", result);
        }

        [Fact]
        public void FoundHotfixesMessage_ContainsUpgradeWarning()
        {
            // Arrange
            var fixes = new List<string> { "TestFix" };

            // Act
            var result = CMessages.FoundHotfixesMessage(fixes);

            // Assert
            Assert.Contains("delay your upgrade", result);
            Assert.Contains("safely upgrade", result);
        }

        #endregion

        #region Static Messages Tests

        [Fact]
        public void PsVbrConfigStart_IsNotEmpty()
        {
            // Assert
            Assert.False(string.IsNullOrEmpty(CMessages.PsVbrConfigStart));
        }

        [Fact]
        public void PsVbrConfigDone_ContainsDone()
        {
            // Assert
            Assert.Contains("DONE!", CMessages.PsVbrConfigDone);
        }

        [Fact]
        public void PsVbrFunctionStart_IsNotEmpty()
        {
            // Assert
            Assert.False(string.IsNullOrEmpty(CMessages.PsVbrFunctionStart));
        }

        [Fact]
        public void PsVbrConfigStartProc_ContainsPowerShell()
        {
            // Assert
            Assert.Contains("PowerShell", CMessages.PsVbrConfigStartProc);
        }

        #endregion
    }
}
