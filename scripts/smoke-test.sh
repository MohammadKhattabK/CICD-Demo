#!/usr/bin/env bash
# Post-deployment smoke test for the deployed API and frontend.
#
# Env vars:
#   API_URL         Base URL of the API        (default: Render API)
#   FRONTEND_URL    Base URL of the frontend   (default: Render static site)
#   EXPECTED_SHA    If set, wait until the API reports this commit as deployed
#   DEPLOY_TIMEOUT  Seconds to wait for EXPECTED_SHA to go live (default: 900)
set -euo pipefail

API_URL="${API_URL:-https://cicd-demo-api-cdyb.onrender.com}"
FRONTEND_URL="${FRONTEND_URL:-https://cicd-demo-dzry.onrender.com}"
EXPECTED_SHA="${EXPECTED_SHA:-}"
DEPLOY_TIMEOUT="${DEPLOY_TIMEOUT:-900}"

summary() {
  [ -n "${GITHUB_STEP_SUMMARY:-}" ] && echo "$*" >> "$GITHUB_STEP_SUMMARY"
  return 0
}

fail() {
  echo "::error title=Smoke test failed::$*"
  summary "### :x: Smoke test failed"
  summary "$*"
  exit 1
}

# Render's free plan puts idle services to sleep; the first request can take
# up to a minute to wake the service, so retry generously.
fetch() {
  curl --fail --silent --show-error \
    --retry 8 --retry-delay 15 --retry-connrefused --retry-all-errors \
    --max-time 60 "$@"
}

echo "::group::API liveness"
live=$(fetch "$API_URL/health/live") || fail "GET $API_URL/health/live did not succeed (service down or still waking up)"
echo "response: $live"
[ "$live" = "Healthy" ] || fail "/health/live returned '$live', expected 'Healthy'"
echo "::endgroup::"

echo "::group::API readiness"
ready=$(fetch "$API_URL/health/ready") || fail "GET $API_URL/health/ready did not succeed"
echo "response: $ready"
[ "$ready" = "Healthy" ] || fail "/health/ready returned '$ready', expected 'Healthy'"
echo "::endgroup::"

echo "::group::Deployed version"
deadline=$((SECONDS + DEPLOY_TIMEOUT))
while true; do
  root=$(fetch "$API_URL/") || fail "GET $API_URL/ did not succeed"
  echo "response: $root"

  status=$(jq -r '.status // empty' <<<"$root")
  [ "$status" = "Running" ] || fail "API root status is '$status', expected 'Running'"

  live_sha=$(jq -r '.commit // empty' <<<"$root")

  if [ -z "$EXPECTED_SHA" ]; then
    # Scheduled run: no specific commit to compare against, but the API must
    # still report that it knows which commit it is running.
    { [ -n "$live_sha" ] && [ "$live_sha" != "unknown" ]; } \
      || fail "API does not report a deployed commit (got '$live_sha')"
    break
  fi

  [ "$live_sha" = "$EXPECTED_SHA" ] && break

  [ "$SECONDS" -lt "$deadline" ] \
    || fail "Timed out after ${DEPLOY_TIMEOUT}s: API runs commit '$live_sha', expected '$EXPECTED_SHA'. Check the Render deploy logs."

  echo "API runs ${live_sha:-?}, waiting for $EXPECTED_SHA ..."
  sleep 20
done
echo "Deployed commit: $live_sha"
echo "::endgroup::"

echo "::group::Products endpoint"
products=$(fetch "$API_URL/api/products") || fail "GET $API_URL/api/products did not succeed"
count=$(jq -r '.totalCount // 0' <<<"$products")
[ "$count" -gt 0 ] || fail "/api/products returned totalCount=$count, expected > 0"
echo "totalCount: $count"
echo "::endgroup::"

echo "::group::Frontend"
html=$(fetch "$FRONTEND_URL/") || fail "GET $FRONTEND_URL/ did not succeed"
grep -q 'id="app"' <<<"$html" || fail "Frontend HTML does not contain the app root element"
echo "frontend OK"
echo "::endgroup::"

summary "### :white_check_mark: Smoke test passed"
summary "- API commit: \`$live_sha\`"
summary "- Products: $count"
echo "Smoke test passed."
