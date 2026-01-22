# Migration Checklist: Fork → VeeamHub/veeam-healthcheck

Quick reference checklist for migrating from your fork to working directly on the upstream repository.

## Prerequisites
- [ ] Admin access to VeeamHub/veeam-healthcheck repository
- [ ] Self-hosted runners currently configured and working
- [ ] All secrets documented (VBR credentials, VirusTotal API key, etc.)

## Phase 1: Configure Upstream Security (30 minutes)

### GitHub Settings → Actions → General
- [ ] Set "Fork pull request workflows from outside collaborators" to:
      **"Require approval for first-time contributors"**
- [ ] Verify "Workflow permissions" is set to:
      **"Read repository contents and packages permissions"**

### GitHub Settings → Branches → Add branch protection rule
- [ ] **Branch name pattern**: `master`
- [ ] **Require a pull request before merging**: ✓
  - [ ] Require approvals: 1 (minimum)
  - [ ] Dismiss stale pull request approvals: ✓
- [ ] **Require status checks to pass before merging**: ✓
  - [ ] Require branches to be up to date: ✓
  - [ ] Status check: `build-and-test`
- [ ] **Require conversation resolution before merging**: ✓
- [ ] **Do not allow bypassing the above settings**: ✓
- [ ] **Allow force pushes**: ✗ (OFF)

### GitHub Settings → Secrets and variables → Actions

**All 7 secrets below are REQUIRED** (see docs/SECRET-AUDIT.md for detailed analysis):

- [ ] Add secret: `VBR_HOST` (used by all 3 integration tests)
- [ ] Add secret: `VBR_USERNAME` (used by vbr + vbr-12 integration tests)
- [ ] Add secret: `VBR_PASSWORD` (used by vbr-12 integration test)
- [ ] Add secret: `VBR_PASSWORD2` (used by vbr/CONSOLE_HOST integration test)
- [ ] Add secret: `VBR_WIN_USER` (used by vbr-13-sql integration test)
- [ ] Add secret: `VBR_WIN_PWD` (used by vbr-13-sql integration test)
- [ ] Add secret: `VIRUSTOTAL_API_KEY` (used by ci-cd.yaml virustotal-scan job)
- [ ] Verify all secrets are marked as "Secret" (encrypted), not "Variable"
- [ ] Remove variable `VT_API_KEY` if it exists (security issue - should be secret not variable)

### GitHub Settings → Actions → Runners
- [ ] Verify self-hosted runners are online and healthy
- [ ] Check runner labels match workflow files:
  - [ ] `CONSOLE_HOST`
  - [ ] `vbr-13-sql`
  - [ ] `vbr-v12`

## Phase 2: Test Security Configuration (15 minutes)

### Test 1: Verify Current Workflow Protection
- [ ] Create a test branch in VeeamHub/veeam-healthcheck
- [ ] Push a trivial change (e.g., update README)
- [ ] Create PR
- [ ] Verify **only** `build-and-test` runs (GitHub-hosted)
- [ ] Verify integration tests are **skipped** (they should not run on PRs)
- [ ] Merge PR
- [ ] Verify integration tests **run on master** after merge

### Test 2: Verify Approval Workflow (if applicable)
- [ ] Ask a colleague to fork the repo (or use a test account)
- [ ] Have them create a PR with a workflow file change
- [ ] Verify you see "Workflow approval required" prompt
- [ ] Approve and verify workflow runs only after approval

## Phase 3: Migrate Local Repository (5 minutes)

### Update Git Remotes
```bash
cd /Users/adam/code/veeam-healthcheck-ac-fork

# Rename current origin to 'fork'
git remote rename origin fork

# Add upstream as new origin
git remote add origin https://github.com/VeeamHub/veeam-healthcheck.git

# Fetch upstream
git fetch origin

# Set master to track upstream
git branch --set-upstream-to=origin/master master

# Verify remotes
git remote -v
```

Expected output:
```
fork    https://github.com/your-username/veeam-healthcheck.git (fetch)
fork    https://github.com/your-username/veeam-healthcheck.git (push)
origin  https://github.com/VeeamHub/veeam-healthcheck.git (fetch)
origin  https://github.com/VeeamHub/veeam-healthcheck.git (push)
```

### Push Current Branch to Upstream
```bash
# Push your current branch to upstream
git push origin docs/update-version-compatibility

# Create PR in VeeamHub/veeam-healthcheck via web UI or gh CLI
gh pr create --base master --head docs/update-version-compatibility \
  --title "Update supported platforms and versions" \
  --body "Updates README.md with current version compatibility information"
```

## Phase 4: Workflow Validation (10 minutes)

### Verify PR Workflow
- [ ] PR created in VeeamHub/veeam-healthcheck
- [ ] `build-and-test` workflow runs on `windows-latest`
- [ ] Integration tests are **skipped** (check workflow logs for "Skipped" status)
- [ ] No self-hosted runner execution on PR
- [ ] Request review from maintainer

### Verify Merge Workflow
- [ ] PR approved by reviewer
- [ ] PR merged to master
- [ ] Full CI/CD pipeline runs on master:
  - [ ] `build-and-test` (GitHub-hosted)
  - [ ] `integration-test-vbr` (self-hosted: CONSOLE_HOST)
  - [ ] `integration-test-vbr-13-sql` (self-hosted: vbr-13-sql)
  - [ ] `integration-test-vbr-12` (self-hosted: vbr-v12)
  - [ ] `virustotal-scan` (GitHub-hosted)
  - [ ] `create-release` (GitHub-hosted, if on master)

## Phase 5: Cleanup (Optional, 5 minutes)

### Option A: Archive Your Fork
- [ ] Navigate to fork settings: `https://github.com/your-username/veeam-healthcheck/settings`
- [ ] Scroll to "Danger Zone"
- [ ] Click "Archive this repository"
- [ ] Confirm archival

### Option B: Keep Fork as Experimental Playground
- [ ] Add a README note: "This is an experimental fork. Production work happens at VeeamHub/veeam-healthcheck"
- [ ] Update fork's description: "Experimental fork - see VeeamHub/veeam-healthcheck for production"

### Update Local Remote (if archiving fork)
```bash
# Remove fork remote (if no longer needed)
git remote remove fork

# Verify only upstream remains
git remote -v
```

## Verification Checklist

After migration, verify:
- [ ] You can create PRs directly in VeeamHub/veeam-healthcheck
- [ ] PRs run only GitHub-hosted runners
- [ ] Merged code runs full test suite with self-hosted runners
- [ ] Secrets are accessible in workflows (check workflow logs for successful authentication)
- [ ] Releases are created automatically on master merges
- [ ] No security warnings in "Settings → Code security and analysis"

## Rollback Plan (If Something Goes Wrong)

If you need to revert:

```bash
# Switch back to fork
git remote rename origin upstream
git remote rename fork origin

# Update tracking branch
git branch --set-upstream-to=origin/master master

# Push to fork
git push origin docs/update-version-compatibility
```

You can always revert these changes - nothing is destructive.

## Support

- **Full documentation**: See `docs/WORKFLOW-SECURITY-SETUP.md`
- **GitHub docs**: https://docs.github.com/en/actions/security-guides
- **Veeam community**: VeeamHub discussions

## Estimated Total Time
- **Security Configuration**: 30 minutes
- **Testing**: 15 minutes
- **Migration**: 5 minutes
- **Validation**: 10 minutes
- **Total**: ~60 minutes

## Success Criteria

✅ Migration is successful when:
1. PRs are created directly in VeeamHub/veeam-healthcheck (no fork intermediary)
2. Self-hosted runners only execute on merged code (master/dev branches)
3. External PRs require approval before running workflows
4. All integration tests pass on master
5. Releases are created automatically
6. You no longer need to maintain a separate fork

---

**Next Steps**: Start with Phase 1 (Configure Upstream Security) and work through each phase sequentially.
