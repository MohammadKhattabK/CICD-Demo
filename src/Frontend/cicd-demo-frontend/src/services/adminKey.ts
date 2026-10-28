const STORAGE_KEY = 'cicd-demo-admin-key'

// sessionStorage keeps the key only for this tab and never in the bundle or the repo.
// Storage can be unavailable (private mode, blocked cookies), so every access is guarded.

export function getAdminKey(): string {
  try {
    return sessionStorage.getItem(STORAGE_KEY) ?? ''
  } catch {
    return ''
  }
}

export function setAdminKey(key: string): void {
  try {
    sessionStorage.setItem(STORAGE_KEY, key)
  } catch {
    /* ignore: the key then only lasts until the page reloads */
  }
}

export function clearAdminKey(): void {
  try {
    sessionStorage.removeItem(STORAGE_KEY)
  } catch {
    /* ignore */
  }
}
