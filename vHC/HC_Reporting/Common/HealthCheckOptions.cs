// Copyright (c) Veeam Software Group GmbH
// Licensed under MIT

using System;

namespace VeeamHealthCheck.Shared
{
    /// <summary>
    /// Specifies the type of health check report to generate.
    /// </summary>
    public enum ReportType
    {
        /// <summary>
        /// No report type selected.
        /// </summary>
        None = 0,

        /// <summary>
        /// Full VBR health check report.
        /// </summary>
        VbrFull = 1,

        /// <summary>
        /// VBR security-focused report.
        /// </summary>
        VbrSecurity = 2,

        /// <summary>
        /// Veeam Backup for Microsoft 365 report.
        /// </summary>
        Vb365 = 3
    }

    /// <summary>
    /// Encapsulates configuration options for a health check run.
    /// This class provides a strongly-typed, testable alternative to static CGlobals fields.
    /// </summary>
    /// <remarks>
    /// This class is designed to gradually replace the static state in CGlobals.
    /// New code should prefer receiving HealthCheckOptions via constructor injection.
    /// </remarks>
    public class HealthCheckOptions
    {
        /// <summary>
        /// Gets or sets the number of days to include in the report.
        /// Valid values: 7, 30, 90. Default: 7.
        /// </summary>
        public int ReportDays { get; set; } = 7;

        /// <summary>
        /// Gets or sets whether to anonymize/scrub sensitive data in the report.
        /// </summary>
        public bool Scrub { get; set; }

        /// <summary>
        /// Gets or sets the type of report to generate.
        /// </summary>
        public ReportType ReportType { get; set; } = ReportType.None;

        /// <summary>
        /// Gets or sets the target VBR/VB365 server name.
        /// </summary>
        public string TargetServer { get; set; } = "localhost";

        /// <summary>
        /// Gets a value indicating whether this is a remote execution.
        /// </summary>
        public bool IsRemoteExecution =>
            !string.IsNullOrEmpty(this.TargetServer) &&
            !this.TargetServer.Equals("localhost", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets whether to export the report as PDF.
        /// </summary>
        public bool ExportPdf { get; set; }

        /// <summary>
        /// Gets or sets whether to export the report as PowerPoint.
        /// </summary>
        public bool ExportPptx { get; set; }

        /// <summary>
        /// Gets or sets whether to open the HTML report after generation.
        /// </summary>
        public bool OpenHtmlAfterGeneration { get; set; }

        /// <summary>
        /// Gets or sets whether to open Explorer to the output folder after generation.
        /// </summary>
        public bool OpenExplorerAfterGeneration { get; set; }

        /// <summary>
        /// Gets or sets the custom output path. If null, uses default.
        /// </summary>
        public string OutputPath { get; set; }

        /// <summary>
        /// Gets or sets whether to run in debug mode with verbose logging.
        /// </summary>
        public bool DebugMode { get; set; }

        /// <summary>
        /// Gets or sets whether to clear stored credentials before running.
        /// </summary>
        public bool ClearStoredCredentials { get; set; }

        /// <summary>
        /// Gets or sets whether to export individual job HTML files.
        /// </summary>
        public bool ExportIndividualJobHtmls { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to check for hotfixes.
        /// </summary>
        public bool CheckHotfixes { get; set; }

        /// <summary>
        /// Validates the options and returns any validation errors.
        /// </summary>
        /// <param name="error">The validation error message, if any.</param>
        /// <returns>True if options are valid, false otherwise.</returns>
        public bool Validate(out string error)
        {
            error = null;

            if (this.ReportDays <= 0)
            {
                error = "ReportDays must be greater than 0.";
                return false;
            }

            if (this.ReportDays > 365)
            {
                error = "ReportDays cannot exceed 365.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a copy of these options.
        /// </summary>
        /// <returns>A new HealthCheckOptions instance with the same values.</returns>
        public HealthCheckOptions Clone()
        {
            return new HealthCheckOptions
            {
                ReportDays = this.ReportDays,
                Scrub = this.Scrub,
                ReportType = this.ReportType,
                TargetServer = this.TargetServer,
                ExportPdf = this.ExportPdf,
                ExportPptx = this.ExportPptx,
                OpenHtmlAfterGeneration = this.OpenHtmlAfterGeneration,
                OpenExplorerAfterGeneration = this.OpenExplorerAfterGeneration,
                OutputPath = this.OutputPath,
                DebugMode = this.DebugMode,
                ClearStoredCredentials = this.ClearStoredCredentials,
                ExportIndividualJobHtmls = this.ExportIndividualJobHtmls,
                CheckHotfixes = this.CheckHotfixes
            };
        }

        /// <summary>
        /// Applies these options to CGlobals for backward compatibility.
        /// </summary>
        /// <remarks>
        /// This method provides a bridge during the migration from static CGlobals
        /// to instance-based HealthCheckOptions. New code should avoid using CGlobals directly.
        /// </remarks>
        public void ApplyToGlobals()
        {
            CGlobals.ReportDays = this.ReportDays;
            CGlobals.Scrub = this.Scrub;
            CGlobals.REMOTEHOST = this.TargetServer;
            CGlobals.REMOTEEXEC = this.IsRemoteExecution;
            CGlobals.EXPORTPDF = this.ExportPdf;
            CGlobals.EXPORTPPTX = this.ExportPptx;
            CGlobals.OpenHtml = this.OpenHtmlAfterGeneration;
            CGlobals.OpenExplorer = this.OpenExplorerAfterGeneration;
            CGlobals.DEBUG = this.DebugMode;
            CGlobals.ClearStoredCreds = this.ClearStoredCredentials;
            CGlobals.EXPORTINDIVIDUALJOBHTMLS = this.ExportIndividualJobHtmls;
            CGlobals.CHECKFIXES = this.CheckHotfixes;

            if (!string.IsNullOrEmpty(this.OutputPath))
            {
                CGlobals.desiredPath = this.OutputPath;
            }

            // Set report type flags
            switch (this.ReportType)
            {
                case ReportType.VbrFull:
                    CGlobals.IsVbr = true;
                    CGlobals.IsVb365 = false;
                    CGlobals.RunFullReport = true;
                    CGlobals.RunSecReport = false;
                    break;
                case ReportType.VbrSecurity:
                    CGlobals.IsVbr = true;
                    CGlobals.IsVb365 = false;
                    CGlobals.RunFullReport = false;
                    CGlobals.RunSecReport = true;
                    break;
                case ReportType.Vb365:
                    CGlobals.IsVbr = false;
                    CGlobals.IsVb365 = true;
                    CGlobals.RunFullReport = true;
                    CGlobals.RunSecReport = false;
                    break;
            }
        }

        /// <summary>
        /// Creates a HealthCheckOptions instance from current CGlobals state.
        /// </summary>
        /// <returns>A new HealthCheckOptions reflecting current CGlobals values.</returns>
        public static HealthCheckOptions FromGlobals()
        {
            var options = new HealthCheckOptions
            {
                ReportDays = CGlobals.ReportDays,
                Scrub = CGlobals.Scrub,
                TargetServer = CGlobals.REMOTEHOST,
                ExportPdf = CGlobals.EXPORTPDF,
                ExportPptx = CGlobals.EXPORTPPTX,
                OpenHtmlAfterGeneration = CGlobals.OpenHtml,
                OpenExplorerAfterGeneration = CGlobals.OpenExplorer,
                DebugMode = CGlobals.DEBUG,
                ClearStoredCredentials = CGlobals.ClearStoredCreds,
                ExportIndividualJobHtmls = CGlobals.EXPORTINDIVIDUALJOBHTMLS,
                CheckHotfixes = CGlobals.CHECKFIXES,
                OutputPath = CGlobals.desiredPath
            };

            // Determine report type from flags
            if (CGlobals.IsVb365)
            {
                options.ReportType = ReportType.Vb365;
            }
            else if (CGlobals.RunSecReport)
            {
                options.ReportType = ReportType.VbrSecurity;
            }
            else if (CGlobals.RunFullReport || CGlobals.IsVbr)
            {
                options.ReportType = ReportType.VbrFull;
            }

            return options;
        }
    }
}
