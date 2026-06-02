import { ref } from 'vue';

/**
 * Singleton reactive store for the server-authoritative application mode.
 *
 * Mode values match the backend ApplicationMode enum:
 *   Official | Dms | Diabolo | Lion | Dysis | Kwaak | Kompas | Klootviool
 *
 * Usage:
 *   const { activeMode, setMode, loadMode } = useAppMode();
 *
 * Call loadMode() once on app startup (done automatically by useDifferentBgMode).
 */

const activeMode = ref('Official');   // server-authoritative, reactive
let   loadPromise = null;             // de-duplicate concurrent load calls
const availableModes = ref(['Official']);

export const ApplicationModes = Object.freeze({
    Official: 'Official',
    Dms: 'Dms',
    Diabolo: 'Diabolo',
    Lion: 'Lion',
    Dysis: 'Dysis',
    Kwaak: 'Kwaak',
    Kompas: 'Kompas',
    Klootviool: 'Klootviool',
});

export function useAppMode() {
    /**
     * Fetches the current mode from the backend and updates activeMode.
     * Calling this multiple times before the first resolves is safe — it
     * returns the same promise.
     */
    async function loadMode() {
        if (loadPromise) return loadPromise;
        loadPromise = Promise.all([
            fetch('/api/mode')
                .then(r => r.json())
                .then(d => { activeMode.value = d.mode ?? ApplicationModes.Official; })
                .catch(() => { activeMode.value = ApplicationModes.Official; }),
            fetch('/api/leaderboards/modes')
                .then(r => r.json())
                .then(d => {
                    const slugs = Array.isArray(d)
                        ? d.map(x => x?.slug).filter(Boolean)
                        : [];
                    availableModes.value = slugs.length
                        ? slugs
                        : Object.values(ApplicationModes);
                })
                .catch(() => {
                    availableModes.value = Object.values(ApplicationModes);
                }),
        ])
            .finally(() => { loadPromise = null; });
        return loadPromise;
    }

    /**
     * Persists a new mode to the backend and updates the local reactive state.
     * @param {string} modeName  - Backend enum name, e.g. "Dms", "Diabolo"
     */
    async function setMode(modeName) {
        const validModes = new Set(availableModes.value.length ? availableModes.value : Object.values(ApplicationModes));
        if (!validModes.has(modeName)) return;

        try {
            const r = await fetch('/api/mode', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ mode: modeName }),
            });
            const d = await r.json();
            activeMode.value = d.mode ?? modeName;
        } catch {
            // Best-effort: update locally even if the server call fails
            activeMode.value = modeName;
        }
    }

    return { activeMode, availableModes, setMode, loadMode };
}
