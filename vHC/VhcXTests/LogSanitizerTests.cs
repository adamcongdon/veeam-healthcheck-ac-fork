// Copyright (c) 2021, Adam Congdon <adam.congdon2@gmail.com>
// MIT License
using Xunit;
using VeeamHealthCheck.Functions.Collection.Security;

namespace VeeamHealthCheck.Tests.Security
{
    [Collection("Security Tests")]
    [Trait("Category", "Security")]
    public class LogSanitizerTests
    {
        [Fact]
        public void MaskSensitiveValue_WithPassword_ShouldMask()
        {
            string message = "Connecting with password: MySecret123";
            var result = LogSanitizer.MaskSensitiveValue(message, "MySecret123");
            Assert.DoesNotContain("MySecret123", result);
            Assert.Equal("Connecting with password: ****", result);
        }

        [Fact]
        public void MaskSensitiveValue_WithNullMessage_ShouldReturnEmpty()
        {
            var result = LogSanitizer.MaskSensitiveValue(null, "secret");
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void MaskSensitiveValue_WithNullSensitiveValue_ShouldReturnOriginal()
        {
            string message = "Some log message";
            var result = LogSanitizer.MaskSensitiveValue(message, null);
            Assert.Equal(message, result);
        }

        [Fact]
        public void MaskSensitiveValue_WithMultipleOccurrences_ShouldMaskAll()
        {
            string message = "Password=secret, Confirm=secret";
            var result = LogSanitizer.MaskSensitiveValue(message, "secret");
            Assert.Equal("Password=****, Confirm=****", result);
        }

        [Fact]
        public void MaskSensitiveValues_WithMultipleSecrets_ShouldMaskAll()
        {
            string message = "User: admin, Pass: secret123, Token: abc-xyz";
            var result = LogSanitizer.MaskSensitiveValues(message, "secret123", "abc-xyz");
            Assert.DoesNotContain("secret123", result);
            Assert.DoesNotContain("abc-xyz", result);
            Assert.Contains("admin", result);
        }

        [Fact]
        public void MaskPowerShellPasswords_WithSingleQuotePassword_ShouldMask()
        {
            string command = "Connect-VBRServer -Server 'localhost' -Password 'MySecret123'";
            var result = LogSanitizer.MaskPowerShellPasswords(command);
            Assert.DoesNotContain("MySecret123", result);
            Assert.Contains("-Password '****'", result);
        }

        [Fact]
        public void MaskPowerShellPasswords_WithDoubleQuotePassword_ShouldMask()
        {
            string command = "Connect-VBRServer -Server \"localhost\" -Password \"MySecret123\"";
            var result = LogSanitizer.MaskPowerShellPasswords(command);
            Assert.DoesNotContain("MySecret123", result);
            Assert.Contains("-Password \"****\"", result);
        }

        [Fact]
        public void MaskPowerShellPasswords_WithBase64Password_ShouldMask()
        {
            string command = "-User \"admin\" -PasswordBase64 \"U2VjcmV0MTIz\"";
            var result = LogSanitizer.MaskPowerShellPasswords(command);
            Assert.DoesNotContain("U2VjcmV0MTIz", result);
            Assert.Contains("-PasswordBase64 \"****\"", result);
        }

        [Fact]
        public void MaskPowerShellPasswords_WithComplexCommand_ShouldMaskOnlyPasswords()
        {
            string command = "Import-Module Veeam.Backup.PowerShell; Connect-VBRServer -Server 'vbr01.local' -User 'admin' -Password 'P@ssw0rd!'";
            var result = LogSanitizer.MaskPowerShellPasswords(command);
            Assert.DoesNotContain("P@ssw0rd!", result);
            Assert.Contains("vbr01.local", result);
            Assert.Contains("admin", result);
            Assert.Contains("-Password '****'", result);
        }

        [Fact]
        public void MaskPowerShellPasswords_WithNoPassword_ShouldReturnOriginal()
        {
            string command = "Get-VBRJob | Select-Object Name";
            var result = LogSanitizer.MaskPowerShellPasswords(command);
            Assert.Equal(command, result);
        }

        [Fact]
        public void SanitizeProcessArguments_RealWorldTestMfaCommand_ShouldMask()
        {
            string escapedPassword = "Test''Password";
            string args = $"Import-Module Veeam.Backup.PowerShell; Connect-VBRServer -Server 'localhost' -User 'admin' -Password '{escapedPassword}'";
            var result = LogSanitizer.SanitizeProcessArguments(args, escapedPassword);
            Assert.DoesNotContain(escapedPassword, result);
            Assert.Contains("-Password '****'", result);
        }
    }
}
