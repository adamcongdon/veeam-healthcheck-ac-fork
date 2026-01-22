# Workflow Security Setup for Self-Hosted Runners

This guide explains how to configure VeeamHub/veeam-healthcheck to work directly with self-hosted runners securely, eliminating the need for a separate fork.

## Current Security Status

Your workflows already implement **partial protection**:
- Integration tests have `if: github.event_name != 'pull_request'` (lines 121, 337, 553 in ci-cd.yaml)
- This prevents self-hosted runners from executing on external PRs

However, this doesn't protect against:
- Internal contributors with write access creating malicious branches
- Lack of workflow approval requirements

## Complete Security Configuration

### 1. Repository Settings - Actions Security

Navigate to: **Settings → Actions → General**

#### Required Approvals for Workflows
- **Fork pull request workflows from outside collaborators**: Select "Require approval for first-time contributors"
- This ensures any workflow from a new contributor (including fork PRs) requires manual approval before running

#### Runner Group Configuration (if using runner groups)
- **Settings → Actions → Runner groups**
- Create a runner group named "production-runners"
- Add your self-hosted runners to this group
- Under "Repository access": Select specific repositories
- Under "Workflow access": Select "Selected workflows" and choose only trusted workflows

### 2. Branch Protection Rules

Navigate to: **Settings → Branches → Add branch protection rule**

#### For `master` branch:
```
Branch name pattern: master

Required settings:
☑ Require a pull request before merging
  ☑ Require approvals (at least 1)
  ☑ Dismiss stale pull request approvals when new commits are pushed
☑ Require status checks to pass before merging
  ☑ Require branches to be up to date before merging
  Status checks required:
    - build-and-test (from ci-cd.yaml)
☑ Require conversation resolution before merging
☑ Do not allow bypassing the above settings
☐ Allow force pushes (keep this OFF)
☑ Require deployments to succeed before merging (optional)
```

#### For `dev` branch (if used):
```
Branch name pattern: dev

☑ Require a pull request before merging
  ☑ Require approvals (at least 1)
☑ Require status checks to pass before merging
```

### 3. Enhanced Workflow Security (Optional but Recommended)

Update `ci-cd.yaml` to use conditional runner selection based on event type and repository:

#### Current approach (lines 120-121, 336-337, 552-553):
```yaml
runs-on: [self-hosted, CONSOLE_HOST]
if: github.event_name != 'pull_request'
```

#### Enhanced approach (more explicit):
```yaml
runs-on: [self-hosted, CONSOLE_HOST]
if: |
  github.event_name != 'pull_request' &&
  (github.actor == github.repository_owner ||
   contains(fromJson('["trusted-user1", "trusted-user2"]'), github.actor))
```

This adds an explicit allowlist of trusted users who can trigger self-hosted runners.

### 4. Workflow Permissions

Verify in each workflow file:

```yaml
permissions:
  contents: write      # Only if creating releases
  pull-requests: read  # Only if reading PR data
  checks: write        # Only if publishing test results
```

**Principle**: Grant minimum necessary permissions.

### 5. Secret Management

Navigate to: **Settings → Secrets and variables → Actions**

#### Repository Secrets (encrypted)
- `VBR_HOST` - VBR server hostname
- `VBR_USERNAME` - Credentials
- `VBR_PASSWORD`, `VBR_PASSWORD2`, `VBR_WIN_PWD` - Credentials
- `VIRUSTOTAL_API_KEY` - VirusTotal API key

#### Repository Variables (plaintext, non-sensitive)
- `VT_API_KEY` - Only if using variables instead of secrets

**Important**: Secrets are NOT exposed to workflows from forks unless explicitly configured.

## Migration Plan: Fork → Upstream

### Option A: Clean Start (Recommended)
1. **On VeeamHub/veeam-healthcheck**: Configure all security settings above
2. **Test security**: Create a test PR from a fork or branch to verify approval workflow
3. **Migrate secrets**: Add all secrets from your fork to VeeamHub/veeam-healthcheck
4. **Update remote in your local repo**:
   ```bash
   cd /Users/adam/code/veeam-healthcheck-ac-fork
   git remote rename origin fork
   git remote add origin https://github.com/VeeamHub/veeam-healthcheck.git
   git fetch origin
   git branch --set-upstream-to=origin/master master
   ```
5. **Push your current branch to upstream**:
   ```bash
   git push origin docs/update-version-compatibility
   ```
6. **Create PR directly in VeeamHub/veeam-healthcheck**

### Option B: Keep Fork as Backup
1. Keep your fork for experimental work
2. Configure security on VeeamHub/veeam-healthcheck
3. PR directly to upstream for production changes
4. Use fork only for testing/experimental features

## Testing the Security Configuration

### Test 1: External Fork PR
1. Ask a colleague to fork the repo
2. Have them create a PR with a workflow change
3. Verify you receive an approval prompt before the workflow runs

### Test 2: Self-Hosted Runner Isolation
1. Create a PR in VeeamHub/veeam-healthcheck
2. Verify that only `build-and-test` (on GitHub-hosted `windows-latest`) runs
3. Verify integration tests (self-hosted) do NOT run on the PR
4. Merge the PR
5. Verify integration tests run on the `master` branch push

### Test 3: Secret Protection
1. Add a test secret: `TEST_SECRET=sensitive-value`
2. Create a workflow that tries to echo secrets
3. Verify secrets are masked in logs: `***`

## How This Protects Self-Hosted Runners

### Threat: Malicious PR from external contributor
- **Protection**: Workflow approval required before execution
- **Result**: You manually review code before it runs on your infrastructure

### Threat: Compromised external fork
- **Protection**: Secrets not exposed to fork workflows
- **Result**: Even if workflow runs, it cannot access your VBR credentials

### Threat: PR-based crypto mining
- **Protection**: `if: github.event_name != 'pull_request'` prevents self-hosted execution
- **Result**: PR workflows run on GitHub-hosted runners (safe, isolated, billed to GitHub)

### Threat: Internal contributor with malicious intent
- **Protection**: Branch protection + required reviews
- **Result**: All code is reviewed before merge, preventing malicious code in protected branches

## Workflow Execution Flow (After Configuration)

```
┌─────────────────────────────────────────────────────────────────┐
│ External PR from Fork                                           │
├─────────────────────────────────────────────────────────────────┤
│ 1. PR created                                                   │
│ 2. Workflow approval prompt appears                             │
│ 3. Maintainer reviews code                                      │
│ 4. Maintainer approves workflow                                 │
│ 5. build-and-test runs on windows-latest (GitHub-hosted)       │
│ 6. Integration tests SKIP (if condition)                       │
│ 7. No secrets exposed, no self-hosted runner access            │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ Push to master (after PR merge)                                 │
├─────────────────────────────────────────────────────────────────┤
│ 1. Code merged to master                                        │
│ 2. Workflow runs automatically (trusted code)                   │
│ 3. build-and-test runs on windows-latest                        │
│ 4. Integration tests run on self-hosted runners                 │
│ 5. Secrets available (VBR credentials, etc.)                    │
│ 6. Release created if tests pass                                │
└─────────────────────────────────────────────────────────────────┘
```

## Maintenance

### Regular Security Audits
- Review workflow runs monthly: **Actions → All workflows**
- Check for unusual patterns (failed approval prompts, unexpected workflow runs)
- Update runner tokens regularly: **Settings → Actions → Runners**

### Runner Hardening
- Run self-hosted runners as a non-admin service account
- Isolate runners in a separate network segment
- Use ephemeral runners (spin up fresh for each job) if possible
- Monitor runner logs for suspicious activity

### Dependency Updates
- Keep workflow actions up to date (e.g., `actions/checkout@v4`)
- Review security advisories for action dependencies
- Pin action versions by SHA for critical workflows (more secure than tags)

## Additional Resources

- [GitHub: Securing your workflows](https://docs.github.com/en/actions/security-guides/security-hardening-for-github-actions)
- [GitHub: Approving workflow runs from public forks](https://docs.github.com/en/actions/managing-workflow-runs/approving-workflow-runs-from-public-forks)
- [GitHub: Using self-hosted runners in a workflow](https://docs.github.com/en/actions/hosting-your-own-runners/using-self-hosted-runners-in-a-workflow)

## Summary

**Key Takeaway**: With proper configuration, you can work directly on VeeamHub/veeam-healthcheck while keeping your self-hosted runners secure. The combination of workflow approval, branch protection, and conditional runner selection provides defense-in-depth.

**Next Steps**:
1. Apply GitHub settings (sections 1-2)
2. Verify existing workflow security (section 3)
3. Test the configuration (section "Testing the Security Configuration")
4. Migrate from fork to upstream (section "Migration Plan")
