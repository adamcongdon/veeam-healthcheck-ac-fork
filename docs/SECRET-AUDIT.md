# Secret Usage Audit

Comprehensive analysis of all secrets referenced in GitHub Actions workflows.

## Summary

| Secret Name | Used? | Usage Count | Used In | Status |
|-------------|-------|-------------|---------|--------|
| `VBR_HOST` | ✅ Yes | 3 | ci-cd.yaml (3 integration tests) | **REQUIRED** |
| `VBR_USERNAME` | ✅ Yes | 2 | ci-cd.yaml (2 integration tests) | **REQUIRED** |
| `VBR_PASSWORD` | ✅ Yes | 1 | ci-cd.yaml (integration-test-vbr-12) | **REQUIRED** |
| `VBR_PASSWORD2` | ✅ Yes | 1 | ci-cd.yaml (integration-test-vbr) | **REQUIRED** |
| `VBR_WIN_USER` | ✅ Yes | 1 | ci-cd.yaml (integration-test-vbr-13-sql) | **REQUIRED** |
| `VBR_WIN_PWD` | ✅ Yes | 1 | ci-cd.yaml (integration-test-vbr-13-sql) | **REQUIRED** |
| `VIRUSTOTAL_API_KEY` | ✅ Yes | 3 | ci-cd.yaml (virustotal-scan job) | **REQUIRED** |
| `VT_API_KEY` | ⚠️ Variable | 1 | manual-release.yml | **INCONSISTENT** |

## Detailed Analysis

### VBR Credentials (All Required)

#### `VBR_HOST`
**Status**: ✅ **REQUIRED** - Used in all 3 integration test jobs

**Locations**:
```yaml
# ci-cd.yaml:155 - integration-test-vbr (CONSOLE_HOST runner)
env:
  VBR_HOST: ${{ secrets.VBR_HOST }}
  VBR_USERNAME: ${{ secrets.VBR_USERNAME }}
  VBR_PASSWORD: ${{ secrets.VBR_PASSWORD2 }}

# ci-cd.yaml:371 - integration-test-vbr-13-sql (vbr-13-sql runner)
env:
  VBR_HOST: ${{ secrets.VBR_HOST }}
  VBR_USERNAME: ${{ secrets.VBR_WIN_USER }}
  VBR_PASSWORD: ${{ secrets.VBR_WIN_PWD }}

# ci-cd.yaml:587 - integration-test-vbr-12 (vbr-v12 runner)
env:
  VBR_HOST: ${{ secrets.VBR_HOST }}
  VBR_USERNAME: ${{ secrets.VBR_USERNAME }}
  VBR_PASSWORD: ${{ secrets.VBR_PASSWORD }}
```

**Purpose**: Target VBR server hostname for health check execution.

---

#### `VBR_USERNAME` and `VBR_PASSWORD`
**Status**: ✅ **REQUIRED** - Used for VBR v12 integration test

**Locations**:
```yaml
# ci-cd.yaml:588-589 - integration-test-vbr-12
env:
  VBR_USERNAME: ${{ secrets.VBR_USERNAME }}
  VBR_PASSWORD: ${{ secrets.VBR_PASSWORD }}
```

**Purpose**: Credentials for authenticating to VBR v12 environment.

**Also used in**: `integration-test-vbr` job (line 156) but with different password.

---

#### `VBR_PASSWORD2`
**Status**: ✅ **REQUIRED** - Used for remote VBR integration test

**Locations**:
```yaml
# ci-cd.yaml:157 - integration-test-vbr (CONSOLE_HOST runner)
env:
  VBR_PASSWORD: ${{ secrets.VBR_PASSWORD2 }}
```

**Purpose**: Alternative password for remote VBR testing (same username as `VBR_USERNAME`).

**Why separate?**: Likely different environments with different password policies or rotation schedules.

---

#### `VBR_WIN_USER` and `VBR_WIN_PWD`
**Status**: ✅ **REQUIRED** - Used for VBR v13 SQL integration test

**Locations**:
```yaml
# ci-cd.yaml:372-373 - integration-test-vbr-13-sql
env:
  VBR_USERNAME: ${{ secrets.VBR_WIN_USER }}
  VBR_PASSWORD: ${{ secrets.VBR_WIN_PWD }}
```

**Purpose**: Windows user credentials for VBR v13 with SQL Server environment.

**Why separate?**: Different authentication requirements for SQL-backed VBR installation.

---

### VirusTotal API Key

#### `VIRUSTOTAL_API_KEY` (Secret)
**Status**: ✅ **REQUIRED** - Used in ci-cd.yaml

**Locations**:
```yaml
# ci-cd.yaml:799 - Submit ZIP to VirusTotal
$api = "${{ secrets.VIRUSTOTAL_API_KEY }}"

# ci-cd.yaml:871 - Poll ZIP scan result
$api = "${{ secrets.VIRUSTOTAL_API_KEY }}"

# ci-cd.yaml:976 - Get detailed detection info
$api = "${{ secrets.VIRUSTOTAL_API_KEY }}"
```

**Purpose**: Authenticates with VirusTotal API for automated malware scanning of release artifacts.

**Job**: `virustotal-scan` (runs after integration tests, before release creation)

---

#### `VT_API_KEY` (Variable, not Secret)
**Status**: ⚠️ **INCONSISTENT** - Used in manual-release.yml

**Locations**:
```yaml
# manual-release.yml:102,107 - VirusTotal Scan step
if: ${{ !inputs.skip_virustotal && vars.VT_API_KEY != '' }}
$apiKey = "${{ vars.VT_API_KEY }}"
```

**Issue**: Uses `vars.VT_API_KEY` (repository variable) instead of `secrets.VIRUSTOTAL_API_KEY`

**Security Concern**: Variables are NOT encrypted and visible to all contributors with read access. API keys should ALWAYS be secrets.

**Recommendation**: Update manual-release.yml to use `secrets.VIRUSTOTAL_API_KEY` instead of `vars.VT_API_KEY`.

---

## Credential Matrix by Job

| Job Name | Runner | VBR_HOST | Username Secret | Password Secret |
|----------|--------|----------|----------------|-----------------|
| `integration-test-vbr` | self-hosted, CONSOLE_HOST | ✅ | VBR_USERNAME | VBR_PASSWORD2 |
| `integration-test-vbr-13-sql` | self-hosted, vbr-13-sql | ✅ | VBR_WIN_USER | VBR_WIN_PWD |
| `integration-test-vbr-12` | self-hosted, vbr-v12 | ✅ | VBR_USERNAME | VBR_PASSWORD |

**Pattern**: Each integration test environment uses the same `VBR_HOST` but different credentials, likely due to:
1. Different VBR versions (v12 vs v13)
2. Different authentication methods (local vs domain accounts)
3. Different security policies per environment

---

## Recommendations

### 1. Fix VirusTotal API Key Inconsistency
**Problem**: manual-release.yml uses `vars.VT_API_KEY` (unencrypted variable)
**Solution**: Update to use `secrets.VIRUSTOTAL_API_KEY` for consistency and security

**Change needed in manual-release.yml**:
```yaml
# BEFORE (line 102)
if: ${{ !inputs.skip_virustotal && vars.VT_API_KEY != '' }}

# AFTER
if: ${{ !inputs.skip_virustotal && secrets.VIRUSTOTAL_API_KEY != '' }}

# BEFORE (line 107)
$apiKey = "${{ vars.VT_API_KEY }}"

# AFTER
$apiKey = "${{ secrets.VIRUSTOTAL_API_KEY }}"
```

### 2. Consolidate Password Secrets (Optional)
**Current**: 3 separate password secrets for same host
- `VBR_PASSWORD` (v12)
- `VBR_PASSWORD2` (remote/CONSOLE_HOST)
- `VBR_WIN_PWD` (v13 SQL)

**Options**:
- **Keep as-is**: If passwords must be different due to security policies
- **Consolidate**: If all environments can use the same password, reduce to one secret
- **Document**: Add comments explaining why multiple passwords exist

**Recommendation**: Keep as-is if environments have different password requirements. Document in README.md.

### 3. Secret Naming Convention
**Current mixing**:
- `VBR_PASSWORD` (clear)
- `VBR_PASSWORD2` (unclear why "2")
- `VBR_WIN_PWD` (abbreviation)

**Suggested standardization**:
- `VBR_V12_PASSWORD` - Clearly indicates VBR v12
- `VBR_V13_REMOTE_PASSWORD` - Clearly indicates VBR v13 remote
- `VBR_V13_SQL_PASSWORD` - Clearly indicates VBR v13 SQL

**Benefit**: Self-documenting secrets, easier maintenance.

---

## Migration Checklist Update

Based on this audit, update the migration checklist:

### Required Secrets (All 7 Must Be Configured)

✅ **VBR Environment Secrets** (6 total):
```bash
VBR_HOST="your-vbr-hostname"           # Used by all 3 integration tests
VBR_USERNAME="vbr-user"                # Used by vbr + vbr-12 tests
VBR_PASSWORD="password-for-v12"        # Used by vbr-12 test
VBR_PASSWORD2="password-for-remote"    # Used by vbr (CONSOLE_HOST) test
VBR_WIN_USER="domain\user"             # Used by vbr-13-sql test
VBR_WIN_PWD="password-for-v13-sql"     # Used by vbr-13-sql test
```

✅ **VirusTotal Secret** (1 total):
```bash
VIRUSTOTAL_API_KEY="your-vt-api-key"   # Used by ci-cd.yaml virustotal-scan
```

### Optional Repository Variable (Should Be Removed)
❌ **Remove**: `VT_API_KEY` (repository variable)
   - Currently used by manual-release.yml
   - Should be migrated to use `secrets.VIRUSTOTAL_API_KEY` instead

---

## Security Notes

1. **All secrets are actually required** - None are deprecated or unused
2. **VirusTotal inconsistency** - manual-release.yml should be updated to use secret instead of variable
3. **Password separation is intentional** - Different environments require different credentials
4. **No secrets are exposed to PRs** - All integration tests skip on `pull_request` events
5. **Manual release workflow** - Currently has a security flaw (API key as variable instead of secret)

---

## Test Commands

To verify secrets are configured correctly after migration:

```bash
# Test VBR connectivity (run on self-hosted runner)
$ErrorActionPreference = "Stop"
$host = $env:VBR_HOST
if (Test-Connection $host -Count 1 -Quiet) {
    Write-Host "✅ VBR_HOST ($host) is reachable"
} else {
    Write-Host "❌ VBR_HOST ($host) is NOT reachable"
}

# Test VirusTotal API key (run from GitHub Actions)
$api = $env:VIRUSTOTAL_API_KEY
if ($api -match "^[a-f0-9]{64}$") {
    Write-Host "✅ VIRUSTOTAL_API_KEY format looks correct (64 hex chars)"
} else {
    Write-Host "⚠️ VIRUSTOTAL_API_KEY format may be incorrect"
}
```

---

## Conclusion

**All 7 secrets listed in the migration checklist are actively used and required.**

The only issue found is:
- **manual-release.yml uses `vars.VT_API_KEY` instead of `secrets.VIRUSTOTAL_API_KEY`**

This should be fixed before or during migration to ensure consistent security practices.
