# Deployment Runbook

## Pipeline overview

1. Pull request to `main` -> `CICD-Demo CI` runs (`Build Backend`, `Build Frontend`).
   The `Protect main - Require CI` ruleset blocks the merge until both pass.
2. Merge to `main` -> Render deploys API and frontend (**After CI Checks Pass**).
3. The same push triggers `Post-Deployment Smoke Test`, which waits (up to 15 min)
   until the API reports the expected commit in `GET /` (`commit` field, taken from
   Render's `RENDER_GIT_COMMIT`), then checks health, products and the frontend.
   The expected commit is the latest `main` commit that touched `src/Backend/CICD.Demo.Api`,
   because Render does not redeploy the API for frontend-only merges. If Render's root
   directory for the API changes, update `API_PATH` in the workflow.
4. The smoke test also runs hourly. On failure it opens (or comments on) an issue
   labelled `smoke-test-failure`, and closes it when a later run passes.

Run it manually: Actions -> Post-Deployment Smoke Test -> Run workflow
(optionally pass `expected_sha`). Locally: `bash scripts/smoke-test.sh` (needs `curl`, `jq`).

## Verifying a change end to end

1. Open a PR with a small visible change; confirm both checks pass.
2. Merge it.
3. Watch the "Post-Deployment Smoke Test" run for the merge commit: it passes only once
   the API serves that commit.
4. Open the frontend and confirm the visible change is live. (The frontend does not
   report a commit, so the smoke test only checks that it is reachable and renders the app root.)

## Reading a smoke test failure

| Message | Likely cause |
| --- | --- |
| `/health/live` or `/health/ready` did not succeed | Service down, crashed on start, or still waking up |
| `Timed out ... API runs commit X, expected Y` | Render deploy failed or is still building: check Render deploy logs |
| `API does not report a deployed commit` | `RENDER_GIT_COMMIT` missing: service not deployed from Git |
| `/api/products returned totalCount=0` | App up but broken |
| Frontend HTML does not contain the app root | Frontend deploy broken or wrong build |

Render's free plan sleeps idle services. The script retries for about two minutes to
absorb the cold start, so a single slow request is not a failure.

## Rollback

Pick the fastest option that fits:

1. **Render rollback (fastest, minutes).** Render dashboard -> the service -> Events/Deploys ->
   choose the last good deploy -> **Rollback**. Do this for both the API and the frontend if both changed.
   Rolling back pauses auto-deploy for that service; re-enable it after the fix is on `main`.
2. **Git revert (keeps `main` honest).** Revert the bad merge through a PR:
   `git revert -m 1 <merge-sha>`, push a branch, open a PR, merge once CI passes.
   Render redeploys and the smoke test confirms the reverted commit is live.
3. **Technically successful but wrong deploy** (health checks pass, behaviour is bad):
   the smoke test will not catch it. Roll back first via option 1, then add a test or
   smoke assertion that covers the failure before re-shipping.

After any rollback, run the smoke test manually and check that the reported `commit`
is the one you intended.

Rollback has not been exercised on this project yet; try it once on a harmless
deploy so the first time is not during an incident.

## Secrets and configuration

- No real secrets are stored in Git (scanned: no tokens, keys or production passwords). `appsettings.Development.json` and `docker-compose.yml` contain local-only development credentials.
- `src/Frontend/cicd-demo-frontend/.env.*` contain only public API URLs. Vite embeds `VITE_*`
  values in the browser bundle, so never put secrets in them.
- Real secrets go in Render environment variables (runtime) or GitHub repository secrets
  (workflows). Local overrides belong in `.env.local` / `appsettings.*.local.json`, which are git-ignored.
- The workflows use the default `GITHUB_TOKEN` with minimal permissions
  (`contents: read`, plus `issues: write` only in the smoke test).

## Database (PostgreSQL)

The API stores products in PostgreSQL through EF Core.

- **Migrations** live in `src/Backend/CICD.Demo.Api/Data/Migrations` and are applied automatically when the
  API starts (`Database__MigrateOnStartup`). CI applies them to a real PostgreSQL and fails if the model changed
  without a migration. Keep migrations backward compatible (add first, remove later) so rolling the app back
  still works against the newer schema.
- **Readiness** (`/health/ready`) checks the database; liveness (`/health/live`) does not, so a database
  outage makes the service "not ready" without restarting it.
- **Backups:** enable them on the database provider and do a test restore before relying on it. Render's free
  PostgreSQL expires after 30 days and has no backups, so treat it as demo data only.

### Render setup (do this before merging the database change)

1. Create a Render PostgreSQL instance (same region as the API).
2. On the **API service** set environment variables:
   - `DATABASE_URL` = the database's **Internal Database URL**
   - `Admin__ApiKey` = a long random value (only needed for write endpoints)
3. Merge. If the variables are missing the new deploy fails to start and Render keeps serving the previous deploy.

The frontend needs no change. After the deploy, the smoke test checks `/health/ready` (database reachable)
and `/api/products` (migrated and seeded).
