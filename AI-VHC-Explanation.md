# Veeam Health Check (VBR) Report – Complete Data Point Reference for AI Training

Purpose: Provide precise, section-by-section definitions and evaluation logic for every data point found in the Veeam Health Check VBR HTML report so an AI agent can read, reason, and answer questions consistently. Use this as the canonical mapping between the report and Veeam product concepts. External references: https://helpcenter.veeam.com/ and https://veeambp.com/.

Notes
- Scrubbing: When Scrub=true, names/paths are anonymized via CScrubHandler.ScrubItem; logic lives in CDataFormer and HTML table builders.
- Lookback window: ReportDays controls job sessions and concurrency time filtering (CGlobals.ReportDays, CDataFormer.JobConcurrency()).
- Data flow: PowerShell → CSV → CCsvParser → CDataFormer → CHtmlTables/Exporters → HTML.

Sections Overview (Navigation)
- License Info (id=license): Overall licensing state and entitlements.
- Backup Server (id=vbrserver): Server properties, version, roles, resources.
- Security Summary (id=secsummary): MFA, four-eyes, encryption, immutability.
- Server Summary (id=serversummary): Counts by infrastructure type.
- Job Summary (id=jobsummary): Counts by job type and status.
- Missing Job Types (id=missingjobs): Workloads present but not protected or job types not used.
- Protected Workloads (id=protectedworkloads): Detected vs protected counts, VM and physical.
- Server Info (id=managedServerInfo): All managed servers in configuration.
- Non-Default Registry Keys (id=regkeys): Keys deviating from defaults.
- Proxy Info (id=proxies): Proxy capacity/config.
- SOBR Info (id=sobr): Scale-Out Backup Repository overview.
- SOBR Extent Info (id=extents): Per-extent resources/config.
- Repository Info (id=repos): Non-SOBR repositories.
- Job Concurrency (id=jobcon): Max concurrent jobs per hour.
- VM Concurrency (id=taskcon): Max concurrent VMs per hour.
- Job Session Summary (id=jobsesssum): Per-job session stats.
- Job Info (id=jobs): Detailed job configuration and performance metrics.

License Info
Fields (License.cs, CHtmlTables.LicTable):
- Licensed To: Customer name. Scrubbed when Scrub=true.
- Edition: VBR Edition (e.g., Community, Enterprise). See Veeam licensing docs.
- Status: License state (valid/expired).
- Type: Instance/Sockets/Rental; affects entitlement rules.
- Licensed Instances / Used Instances / New Instances / Rental Instances: Counters of entitled vs consumed.
- Licensed Sockets / Used Sockets: Socket-based licensing metrics.
- Licensed Nas / Used Nas: NAS capacity license metrics.
- Expiration Date / Support Expiration Date: License term and support end.
- Cloud Connect: Whether Cloud Connect features are enabled.
Best practice references: helpcenter.veeam.com → Licensing; veeambp.com → Sizing & licensing guidance.

Backup Server
Fields (CVbrServerTable, CVbrServerTableHelper):
- Name: VBR server host; scrubbed when enabled.
- Veeam Version / API Version: Product build/API compatibility.
- Cores / RAM: Hardware resources used for capacity planning.
- Proxy Role / Repo/Gateway Role / WAN Acc. (Accelerator) Role: Role flags determined by CDataFormer.CheckServerRoles().
- OS Info: Detected OS details.
- Is Unavailable: Connectivity/availability flag from CSV.
Interpretation: Ensure proper resources vs concurrency and roles aligned with best practices (veeambp.com → VBR server sizing).

Security Summary
Source (CDataFormer.SecSummary, CSecuritySummaryTable):
- MFA Enabled: True if any vbrinfo.mfa == "True".
- Four Eyes Authorization: True if vbrinfo.foureyes == "True"; otherwise false.
- Immutability: True if any repo/sobr extent reports immutability supported or object lock enabled.
- Traffic Encryption: True if any network rule encryptionenabled == "True".
- Backup File Encryption: True if jobs have non-empty/non-default pwdkeyid.
- Config Backup Encryption: True if configuration backup encryptionoptions == "True".
Guidance: helpcenter.veeam.com → Security; veeambp.com → Immutability and backup encryption recommendations.

Server Summary
Data (CGlobals.DtParser.ServerSummaryInfo):
- Types: Dictionary of server types and counts (e.g., Hyper-V hosts, ESXi proxies, repositories, tape). Used for environment sizing and gaps.

Job Summary
Data (CDataFormer.JobSummaryInfoToXml):
- Job Type: Distinct JobType values counted across CSV (backup, replication, NAS, agents, tape).
- Count: Number of jobs by type.
Interpretation: Verify coverage vs protected workloads.

Missing Job Types
Definition: Workloads present in infrastructure without corresponding jobs (e.g., detected VMs with no backups, physical servers with no agents, NAS shares without NAS jobs).
Logic: Derived by comparing sources (Vi/Hv/NAS/Physical readers) with job presence (JobInfos). Exact mapping implemented via CCsvParser and DataFormer checks.

Protected Workloads
Data sources (CDataFormer.ProtectedWorkloadsToXml):
- Vi Protected / Vi Not Prot / Vi Potential Dupes: From ViProtectedReader and ViUnProtectedReader; duplicates computed by name distinct counts.
- HV Protected / HV Unprotected / HV Duplicates: From HvProtectedReader/HvUnProtectedReader; dupes computed similarly.
- Phys Protected / Phys Not Prot: From GetPhysProtected/GetPhysNotProtected.
- Total VMs / Protected VMs / Not Protected VMs: Aggregations per server.
Interpretation: Protection rate = Protected / Total; duplicates indicate overlapping inclusion (handle carefully).

Server Info (Managed Servers)
Data (CDataFormer.ServerInfoToXml):
- Name, Cores, RAM, Type, API Version.
- ProtectedVms, NotProtectedVms, TotalVms: Computed counts per managed server.
- IsProxy, IsRepo, IsWan: Role flags.
- OS Info, IsUnavailable.
Usage: Capacity planning and role distribution.

Non-Default Registry Keys
Definition: Any registry value deviating from clean install baseline.
Data: VbrTables.Registry classes identify keys to skip vs report (CRegistrySkipKeys) and output Key/Value pairs.
Guidance: Only change registry per Veeam KB; report deviations for troubleshooting.

Proxy Info
Data: Proxy types (VI/HV/NAS/CDP), Max Tasks, Transport Mode, Block Size, Align Blocks, Compression Level, DeCompress, Failover to NBD.
Source: CCsvParser.GetDynViProxy/GetDynHvProxy/GetDynNasProxy/GetDynCdpProxy; formatted via CHtmlTables/Proxy tables.
Interpretation: Ensure transport/CPU/RAM aligns with concurrency (veeambp.com → Proxy sizing).

SOBR Info
Data (CDataFormer.SobrInfosToXml):
- Name, Extents, ExtentCount, JobCount, PolicyType (Performance/Capacity), Per-VM backup files
- Capacity Tier: Enable/Copy/Move; CapTier Type (object storage vendor), Archive Tier Enabled
- Immutability: ImmuteEnabled, ImmutePeriod
- Size Limit Enabled/Size Limit
Interpretation: Verify use of per-VM chains, capacity/archive policies, immutability per BP.

SOBR Extent Info
Data (CDataFormer.ExtentXmlFromCsv):
- Repo Name (scrubbed), SOBR Name, Max Tasks, Cores, RAM
- Auto Gateway flag; Gate Hosts listing
- Path (scrubbed)
- Free Space (TB), Total Space (TB), Free Space % = FreeSPace/TotalSpace calculated via FreePercent()
- Decompress, Align Blocks, Rotated Drives
- Immutability Supported: true if ObjectLockEnabled or IsImmutabilitySupported
- Type, Provisioning
Interpretation: Validate gateway settings, free space headroom, block alignment, immutability, tasks vs cores.

Repository Info (Non-SOBR)
Data (CDataFormer.RepoInfoToXml):
- Name, Host, Path, Type, Tasks, Cores, RAM
- Free/Total space (TB), Free %
- Align Blocks, Decompress, Rotated Drives
- Immutability and Provisioning
Usage: Same evaluation as extents for standalone repositories.

Job Concurrency (Jobs/hour)
Data (CDataFormer.JobConcurrency(true)):
- Time filter: JobSessions where CreationTime >= ToolStart - ReportDays.
- Aggregation: CConcurrencyHelper.JobCounter() produces hour buckets with max concurrent jobs.
Interpretation: Compare concurrency vs proxy/repo tasks; smooth peaks.

VM Concurrency (Tasks/hour)
Data (CDataFormer.JobConcurrency(false)):
- Aggregation: CConcurrencyHelper.TaskCounter() yields max concurrent tasks (VMs processed) per hour.
Interpretation: Tune tasks per proxy, channel limits, and CPU.

Job Session Summary
Data (CDataFormer.ConvertJobSessSummaryToXml):
- By Job Name: Total Sessions, Success Rate %, Fails, Retries
- Time Metrics: Min/Max/Avg Time (min), Wait metrics (dd.hh:mm:ss)
- Size Metrics: Avg/Max Backup Size (TB), Avg/Max Data Size (TB), Source Size
- Change Rate %: Avg Change Rate
- Restore Points, GFS retention
Usage: Stability and performance assessment; identify slow jobs and high retry rates.

Job Info (Per Job)
Fields (VbrTables/Jobs Info):
- Job Name, Job Type, Is Enabled/Is Disabled, Last Result, Next Run
- Backup Chain Type (per-VM vs per-machine), Retention Scheme (GFS Enabled, GFS Retention)
- Transport Mode, Block Size, Compression Level, Hardware Compression
- Active/Synthetic Full Enabled, Per-VM/Per-Machine settings
- Encryption: Backup File Encryption, Traffic Encryption, Config Backup Encryption
- Target: Repository/SOBR, Path
- Items: Count and item listing; Excluded Item presence
- Media/Tape: Media Pool (Full/Incremental), Eject Medium, Export Media Set
- Platform: VI/HV/Agent/NAS
Interpretation: Validate schedule, retention, chain type, transport, encryption per BP; ensure target capacity and immutability.

Security: Backup Server Table (CSecurityBackupServerTable)
- Console Status, RDP Status, Domain Status: Hardening checks.
- Malware Feature: Whether malware detection is enabled.
- Compliance Table: Recon/Compliance checks (CVbrSecurityTables).
Guidance: Align with Veeam hardening guide and BP.

Other Common Columns You May See (consolidated from table <th>)
- Tasks (Set Tasks): Max concurrent tasks for repository/proxy.
- Cache Path/Size: Dedup cache settings (NAS features).
- Policy: Copy/Move/Archive indicators for capacity tier.
- Immutability/Immutable Period: Object-lock and period days.
- Traffic Encryption: On for network rules.
- Cloud Connect Enabled: Presence of CC services.
- Vi/HV/Phys Totals/Protected/Not Prot: Coverage metrics.

Scrubbing Rules (high level)
- Server names, SOBR names, repository names, paths scrubbed with type-aware tokens.
- Outputs preserve structure and counts while anonymizing identifiers.

AI Training Instructions
- Parse by section id per Navigation list; treat each section independently.
- Map HTML table headers (<th>) to the field definitions above; use section context to disambiguate overlapping header names (e.g., "Name" in repo vs job).
- Apply logic rules from CDataFormer when computing derived values:
  - Security flags per SecSummary() boolean tests.
  - Free Space % via FreePercent(FreeSPace, TotalSpace).
  - Concurrency via JobConcurrency(isJob) with ReportDays filter.
  - Duplicates computed as total names minus distinct protected/not-protected counts.
- Respect Scrub flag: If scrubbed identifiers are present, do not attempt to de-anonymize; reason using roles/types and numeric metrics.
- Use external references for domain definitions and best-practice judgments: helpcenter.veeam.com (product features, configuration) and veeambp.com (hardening, sizing, immutability, transport modes).

Quality Checks The AI Should Perform
- Coverage: Protected vs total workloads by platform; highlight gaps.
- Capacity: Free space %, tasks vs cores/RAM, gateway settings.
- Security: MFA, four-eyes, encryption, immutability, hardening flags.
- Performance: Session duration and retries; concurrency peaks vs capacity.
- Configuration hygiene: Non-default registry keys and risky settings.

Answering Style for the AI
- Quote exact field names from the report and section.
- Explain how the value is obtained (CSV → parser → logic) and why it matters (BP reference).
- Provide actionable recommendations referencing veeambp.com and helpcenter.veeam.com when values deviate.

End of document.
