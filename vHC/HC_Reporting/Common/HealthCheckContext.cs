// Copyright (c) Veeam Software Group GmbH
// Licensed under MIT

using System;
using System.Collections.Generic;
using VeeamHealthCheck.Functions.Analysis.DataModels;
using VeeamHealthCheck.Functions.Reporting.DataTypes;
using VeeamHealthCheck.Shared.Logging;

namespace VeeamHealthCheck.Shared
{
    /// <summary>
    /// Holds runtime state collected during a health check execution.
    /// This class provides a structured alternative to the static collections in CGlobals.
    /// </summary>
    /// <remarks>
    /// <para>
    /// HealthCheckContext separates runtime state from configuration (HealthCheckOptions).
    /// It is designed to be created once per health check run and passed through the execution pipeline.
    /// </para>
    /// <para>
    /// Migration path: New code should receive HealthCheckContext via constructor injection.
    /// Existing code can use <see cref="Current"/> as a bridge during migration.
    /// </para>
    /// </remarks>
    public class HealthCheckContext : IDisposable
    {
        private static HealthCheckContext _current;
        private bool _disposed;

        /// <summary>
        /// Gets or sets the current context for the running health check.
        /// This property provides backward compatibility during migration from CGlobals.
        /// </summary>
        public static HealthCheckContext Current
        {
            get => _current ??= new HealthCheckContext();
            set => _current = value;
        }

        /// <summary>
        /// Gets the options for this health check run.
        /// </summary>
        public HealthCheckOptions Options { get; }

        /// <summary>
        /// Gets the logger for this context.
        /// </summary>
        public CLogger Logger { get; }

        /// <summary>
        /// Gets the timestamp when this health check run started.
        /// </summary>
        public DateTime StartTime { get; }

        /// <summary>
        /// Gets the formatted run timestamp (yyyyMMdd_HHmmss).
        /// </summary>
        public string RunTimestamp { get; }

        /// <summary>
        /// Gets or sets the VBR server name being analyzed.
        /// </summary>
        public string VbrServerName { get; set; } = "localhost";

        /// <summary>
        /// Gets or sets the VBR major version.
        /// </summary>
        public int VbrMajorVersion { get; set; }

        /// <summary>
        /// Gets or sets the VBR full version string.
        /// </summary>
        public string VbrFullVersion { get; set; }

        /// <summary>
        /// Gets or sets the VHC tool version.
        /// </summary>
        public string VhcVersion { get; set; }

        /// <summary>
        /// Gets or sets the PowerShell version detected.
        /// </summary>
        public int PowerShellVersion { get; set; }

        /// <summary>
        /// Gets or sets whether VBR is installed on the target.
        /// </summary>
        public bool IsVbrInstalled { get; set; }

        /// <summary>
        /// Gets or sets whether MFA is enabled on the VBR server.
        /// </summary>
        public bool IsMfaEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether reconnaissance was detected.
        /// </summary>
        public bool IsReconDetected { get; set; }

        /// <summary>
        /// Gets or sets the last reconnaissance run time.
        /// </summary>
        public DateTime LastReconRun { get; set; } = DateTime.MinValue;

        #region Database Information

        /// <summary>
        /// Gets or sets whether the database is local.
        /// </summary>
        public string IsDbLocal { get; set; }

        /// <summary>
        /// Gets or sets the database type (SQL Server, PostgreSQL).
        /// </summary>
        public string DbType { get; set; }

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        public string DbName { get; set; }

        /// <summary>
        /// Gets or sets the database version.
        /// </summary>
        public string DbVersion { get; set; }

        /// <summary>
        /// Gets or sets the database hostname.
        /// </summary>
        public string DbHostname { get; set; }

        /// <summary>
        /// Gets or sets the database edition.
        /// </summary>
        public string DbEdition { get; set; }

        /// <summary>
        /// Gets or sets the database instance name.
        /// </summary>
        public string DbInstance { get; set; }

        /// <summary>
        /// Gets or sets the database server core count.
        /// </summary>
        public int DbCores { get; set; }

        /// <summary>
        /// Gets or sets the database server RAM in MB.
        /// </summary>
        public int DbRam { get; set; }

        #endregion

        #region Collected Data

        /// <summary>
        /// Gets the CSV validation results from data collection.
        /// </summary>
        public List<CsvValidationResult> CsvValidationResults { get; } = new();

        /// <summary>
        /// Gets the server information collected.
        /// </summary>
        public List<CServerTypeInfos> ServerInfo { get; } = new();

        /// <summary>
        /// Gets or sets the backup server information.
        /// </summary>
        public BackupServer BackupServer { get; set; }

        /// <summary>
        /// Gets or sets the data types parser.
        /// </summary>
        public CDataTypesParser DataTypesParser { get; set; }

        /// <summary>
        /// Gets the default registry keys.
        /// </summary>
        public Dictionary<string, object> DefaultRegistryKeys { get; } = new();

        /// <summary>
        /// Gets or sets the raw (unscrubbed) report path.
        /// </summary>
        public string RawReportPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the scrubbed report path.
        /// </summary>
        public string ScrubbedReportPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets any user-facing error message.
        /// </summary>
        public string UserFacingError { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the HealthCheckContext class with default options.
        /// </summary>
        public HealthCheckContext()
            : this(new HealthCheckOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the HealthCheckContext class with the specified options.
        /// </summary>
        /// <param name="options">The health check options.</param>
        public HealthCheckContext(HealthCheckOptions options)
            : this(options, CGlobals.Logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the HealthCheckContext class with the specified options and logger.
        /// </summary>
        /// <param name="options">The health check options.</param>
        /// <param name="logger">The logger to use.</param>
        public HealthCheckContext(HealthCheckOptions options, CLogger logger)
        {
            this.Options = options ?? throw new ArgumentNullException(nameof(options));
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.StartTime = DateTime.Now;
            this.RunTimestamp = this.StartTime.ToString("yyyyMMdd_HHmmss");
        }

        /// <summary>
        /// Synchronizes this context's state to CGlobals for backward compatibility.
        /// </summary>
        public void SyncToGlobals()
        {
            // Apply options
            this.Options.ApplyToGlobals();

            // Sync runtime state
            CGlobals.VBRServerName = this.VbrServerName;
            CGlobals.VBRMAJORVERSION = this.VbrMajorVersion;
            CGlobals.VBRFULLVERSION = this.VbrFullVersion;
            CGlobals.VHCVERSION = this.VhcVersion;
            CGlobals.PowerShellVersion = this.PowerShellVersion;
            CGlobals.IsVbrInstalled = this.IsVbrInstalled;
            CGlobals.IsMfaEnabled = this.IsMfaEnabled;
            CGlobals.IsReconDetected = this.IsReconDetected;
            CGlobals.LastReconRun = this.LastReconRun;
            CGlobals.TOOLSTART = this.StartTime;

            // Database info
            CGlobals.ISDBLOCAL = this.IsDbLocal;
            CGlobals.DBTYPE = this.DbType;
            CGlobals.DBNAME = this.DbName;
            CGlobals.DBVERSION = this.DbVersion;
            CGlobals.DBHOSTNAME = this.DbHostname;
            CGlobals.DBEdition = this.DbEdition;
            CGlobals.DBINSTANCE = this.DbInstance;
            CGlobals.DBCORES = this.DbCores;
            CGlobals.DBRAM = this.DbRam;

            // Collected data
            CGlobals.BACKUPSERVER = this.BackupServer;
            CGlobals.DtParser = this.DataTypesParser;
            CGlobals.RawReport = this.RawReportPath;
            CGlobals.ScrubbedReport = this.ScrubbedReportPath;
            CGlobals.UserFacingError = this.UserFacingError;

            // Collections - sync by reference
            CGlobals.CsvValidationResults.Clear();
            CGlobals.CsvValidationResults.AddRange(this.CsvValidationResults);

            CGlobals.ServerInfo.Clear();
            CGlobals.ServerInfo.AddRange(this.ServerInfo);

            CGlobals.DEFAULTREGISTRYKEYS.Clear();
            foreach (var kvp in this.DefaultRegistryKeys)
            {
                CGlobals.DEFAULTREGISTRYKEYS[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>
        /// Creates a HealthCheckContext from current CGlobals state.
        /// </summary>
        /// <returns>A new context reflecting current CGlobals values.</returns>
        public static HealthCheckContext FromGlobals()
        {
            var options = HealthCheckOptions.FromGlobals();
            var context = new HealthCheckContext(options)
            {
                VbrServerName = CGlobals.VBRServerName,
                VbrMajorVersion = CGlobals.VBRMAJORVERSION,
                VbrFullVersion = CGlobals.VBRFULLVERSION,
                VhcVersion = CGlobals.VHCVERSION,
                PowerShellVersion = CGlobals.PowerShellVersion,
                IsVbrInstalled = CGlobals.IsVbrInstalled,
                IsMfaEnabled = CGlobals.IsMfaEnabled,
                IsReconDetected = CGlobals.IsReconDetected,
                LastReconRun = CGlobals.LastReconRun,
                IsDbLocal = CGlobals.ISDBLOCAL,
                DbType = CGlobals.DBTYPE,
                DbName = CGlobals.DBNAME,
                DbVersion = CGlobals.DBVERSION,
                DbHostname = CGlobals.DBHOSTNAME,
                DbEdition = CGlobals.DBEdition,
                DbInstance = CGlobals.DBINSTANCE,
                DbCores = CGlobals.DBCORES,
                DbRam = CGlobals.DBRAM,
                BackupServer = CGlobals.BACKUPSERVER,
                DataTypesParser = CGlobals.DtParser,
                RawReportPath = CGlobals.RawReport,
                ScrubbedReportPath = CGlobals.ScrubbedReport,
                UserFacingError = CGlobals.UserFacingError
            };

            // Copy collections
            context.CsvValidationResults.AddRange(CGlobals.CsvValidationResults);
            context.ServerInfo.AddRange(CGlobals.ServerInfo);

            foreach (var kvp in CGlobals.DEFAULTREGISTRYKEYS)
            {
                context.DefaultRegistryKeys[kvp.Key] = kvp.Value;
            }

            return context;
        }

        /// <summary>
        /// Disposes this context and clears collected data.
        /// </summary>
        public void Dispose()
        {
            if (!this._disposed)
            {
                this.CsvValidationResults.Clear();
                this.ServerInfo.Clear();
                this.DefaultRegistryKeys.Clear();

                if (_current == this)
                {
                    _current = null;
                }

                this._disposed = true;
            }
        }
    }
}
