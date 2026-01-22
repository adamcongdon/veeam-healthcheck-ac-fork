# Secret Quick Reference

## All Secrets Are Required ✅

Every secret listed below is actively used in workflows. **Do not skip any when configuring the upstream repository.**

## Secret → Job Mapping

| Secret Name | Job(s) Using It | Workflow File | Purpose |
|-------------|----------------|---------------|---------|
| `VBR_HOST` | integration-test-vbr<br>integration-test-vbr-13-sql<br>integration-test-vbr-12 | ci-cd.yaml | VBR server hostname |
| `VBR_USERNAME` | integration-test-vbr<br>integration-test-vbr-12 | ci-cd.yaml | VBR username for v12 + remote tests |
| `VBR_PASSWORD` | integration-test-vbr-12 | ci-cd.yaml | Password for VBR v12 environment |
| `VBR_PASSWORD2` | integration-test-vbr | ci-cd.yaml | Password for remote VBR (CONSOLE_HOST) |
| `VBR_WIN_USER` | integration-test-vbr-13-sql | ci-cd.yaml | Windows user for VBR v13 SQL environment |
| `VBR_WIN_PWD` | integration-test-vbr-13-sql | ci-cd.yaml | Password for VBR v13 SQL environment |
| `VIRUSTOTAL_API_KEY` | virustotal-scan | ci-cd.yaml + manual-release.yml | VirusTotal API authentication |

## Why Multiple VBR Passwords?

Different VBR test environments require different credentials:

```
┌─────────────────────────────────────────────────────────────┐
│ Integration Test: vbr (remote)                              │
│ Runner: self-hosted, CONSOLE_HOST                           │
│ Auth: VBR_USERNAME + VBR_PASSWORD2                          │
├─────────────────────────────────────────────────────────────┤
│ Integration Test: vbr-13-sql                                │
│ Runner: self-hosted, vbr-13-sql                             │
│ Auth: VBR_WIN_USER + VBR_WIN_PWD                            │
├─────────────────────────────────────────────────────────────┤
│ Integration Test: vbr-12                                    │
│ Runner: self-hosted, vbr-v12                                │
│ Auth: VBR_USERNAME + VBR_PASSWORD                           │
└─────────────────────────────────────────────────────────────┘
```

**Reason**: Each environment has different authentication requirements (local vs domain accounts, different password policies, etc.)

## Configuration Template

Copy this template when setting up secrets in GitHub:

```bash
# VBR Connection (all tests use this)
VBR_HOST=your-vbr-hostname.domain.com

# VBR v12 + Remote Test Credentials
VBR_USERNAME=vbr-service-account
VBR_PASSWORD=password-for-v12-environment
VBR_PASSWORD2=password-for-remote-environment

# VBR v13 SQL Test Credentials
VBR_WIN_USER=DOMAIN\service-account
VBR_WIN_PWD=password-for-v13-sql-environment

# VirusTotal API
VIRUSTOTAL_API_KEY=your-64-character-hex-api-key
```

## Fixed Issues

### VirusTotal API Key Inconsistency (Fixed in this commit)

**Problem**: manual-release.yml was using `vars.VT_API_KEY` (unencrypted variable)
**Solution**: Updated to use `secrets.VIRUSTOTAL_API_KEY` (encrypted secret)
**Files changed**: `.github/workflows/manual-release.yml` (lines 102, 107, 157)

**Before**:
```yaml
if: ${{ !inputs.skip_virustotal && vars.VT_API_KEY != '' }}
$apiKey = "${{ vars.VT_API_KEY }}"
```

**After**:
```yaml
if: ${{ !inputs.skip_virustotal && secrets.VIRUSTOTAL_API_KEY != '' }}
$apiKey = "${{ secrets.VIRUSTOTAL_API_KEY }}"
```

## Verification Commands

After configuring secrets, verify they're working:

### Test 1: Check VBR Connectivity
```powershell
# Run this on a self-hosted runner
$host = $env:VBR_HOST
Test-Connection $host -Count 1 -Quiet
# Should return: True
```

### Test 2: Check VirusTotal API Key Format
```powershell
# Run this in a GitHub Actions workflow
$api = $env:VIRUSTOTAL_API_KEY
if ($api -match "^[a-f0-9]{64}$") {
    Write-Host "✅ Valid format"
} else {
    Write-Host "❌ Invalid format"
}
```

### Test 3: Full Integration Test (Best Validation)
```bash
# Trigger ci-cd.yaml workflow by pushing to master
git push origin master

# Watch workflow run:
gh run watch
```

If all three integration tests pass, secrets are configured correctly.

## Security Notes

1. ✅ All secrets are encrypted and never visible in workflow logs
2. ✅ Secrets are masked with `***` if accidentally printed
3. ✅ Secrets are NOT exposed to fork PRs (GitHub security feature)
4. ✅ Integration tests skip on PRs, so secrets only used on trusted branches
5. ✅ VirusTotal API key now properly configured as secret (was variable before)

## Common Issues

### Issue: "VBR_HOST is unreachable"
**Cause**: Firewall blocking self-hosted runner → VBR connection
**Solution**: Verify network connectivity from runner to VBR server

### Issue: "Authentication failed"
**Cause**: Incorrect username/password for that specific environment
**Solution**: Double-check which credential pair the failing test uses (see table above)

### Issue: "VirusTotal scan failed: 401 Unauthorized"
**Cause**: Invalid or expired VirusTotal API key
**Solution**: Generate new API key from VirusTotal account, update secret

### Issue: "Secret not found"
**Cause**: Secret name typo or not configured in repository settings
**Solution**: Verify exact secret names match (case-sensitive) in Settings → Secrets

---

For detailed analysis, see: [SECRET-AUDIT.md](./SECRET-AUDIT.md)
