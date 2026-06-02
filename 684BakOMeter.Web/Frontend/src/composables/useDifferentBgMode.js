import { ref, watch } from 'vue';
import { ApplicationModes, useAppMode } from './useAppMode.js';

/**
 * Composable for secret easter egg background modes.
 *
 * Key sequences:
 *   666        => Diabolo  (black + red corner + geheim-logo)
 *   klc        => Lion     (deep red + white leeuwgod watermark)
 *   dms        => Dms      (unofficial play: typed name instead of NFC)
 *   427        => Dysis    (dysis.jpg background)
 *   kwaak      => Kwaak    (kwaak.jpg background)
 *   kompas     => Kompas   (kompas.jpg background)
 *   klootviool => Klootviool (klootviool.png background)
 *
 * Modes are mutually exclusive; re-entering the active code reverts to Official.
 * On startup the backend-authoritative mode is loaded and the correct CSS class is applied.
 * Every toggle is persisted to the backend via POST /api/mode.
 * Call feedKey(e.code) from every keydown handler.
 */

const SECRET_DIABOLO    = ['Digit6', 'Digit6', 'Digit6'];
const SECRET_LION       = ['KeyK', 'KeyL', 'KeyC'];
const SECRET_DEMOS      = ['KeyD', 'KeyM', 'KeyS'];
const SECRET_DYSIS      = ['Digit4', 'Digit2', 'Digit7'];
const SECRET_KWAAK      = ['KeyK', 'KeyW', 'KeyA', 'KeyA', 'KeyK'];
const SECRET_KOMPAS     = ['KeyK', 'KeyO', 'KeyM', 'KeyP', 'KeyA', 'KeyS'];
const SECRET_KLOOTVIOOL = ['KeyK', 'KeyL', 'KeyO', 'KeyO', 'KeyT', 'KeyV', 'KeyI', 'KeyO', 'KeyO', 'KeyL'];
const SECRET_MAX = Math.max(
    SECRET_DIABOLO.length, SECRET_LION.length, SECRET_DEMOS.length,
    SECRET_DYSIS.length, SECRET_KWAAK.length, SECRET_KOMPAS.length,
    SECRET_KLOOTVIOOL.length
);
const buffer = [];

const CSS_CLASS = {
    [ApplicationModes.Diabolo]: 'diabolo-mode',
    [ApplicationModes.Lion]: 'lion-mode',
    [ApplicationModes.Dms]: 'demos-mode',
    [ApplicationModes.Dysis]: 'dysis-mode',
    [ApplicationModes.Kwaak]: 'kwaak-mode',
    [ApplicationModes.Kompas]: 'kompas-mode',
    [ApplicationModes.Klootviool]: 'klootviool-mode',
};

function applyBodyClass(modeName) {
    Object.values(CSS_CLASS).forEach(c => document.body.classList.remove(c));
    if (modeName && modeName !== 'Official') {
        const cls = CSS_CLASS[modeName];
        if (cls) document.body.classList.add(cls);
    }
}

export function useDifferentBgMode() {
    const { activeMode, setMode, loadMode } = useAppMode();

    const diaboloActive    = ref(false);
    const lionActive       = ref(false);
    const demosActive      = ref(false);
    const dysisActive      = ref(false);
    const kwaakActive      = ref(false);
    const kompasActive     = ref(false);
    const klootvioolActive = ref(false);

    function syncFromMode(modeName) {
        diaboloActive.value    = modeName === ApplicationModes.Diabolo;
        lionActive.value       = modeName === ApplicationModes.Lion;
        demosActive.value      = modeName === ApplicationModes.Dms;
        dysisActive.value      = modeName === ApplicationModes.Dysis;
        kwaakActive.value      = modeName === ApplicationModes.Kwaak;
        kompasActive.value     = modeName === ApplicationModes.Kompas;
        klootvioolActive.value = modeName === ApplicationModes.Klootviool;
        applyBodyClass(modeName);
    }

    watch(activeMode, syncFromMode, { immediate: false });

    loadMode().then(() => syncFromMode(activeMode.value));

    async function activateMode(modeName) {
        const next = activeMode.value === modeName ? ApplicationModes.Official : modeName;
        await setMode(next);
        window.dispatchEvent(new CustomEvent('mode-change', { detail: { mode: next } }));
    }

    function toggleDiabolo()    { return activateMode(ApplicationModes.Diabolo); }
    function toggleLion()       { return activateMode(ApplicationModes.Lion); }
    function toggleDemos()      { return activateMode(ApplicationModes.Dms); }
    function toggleDysis()      { return activateMode(ApplicationModes.Dysis); }
    function toggleKwaak()      { return activateMode(ApplicationModes.Kwaak); }
    function toggleKompas()     { return activateMode(ApplicationModes.Kompas); }
    function toggleKlootviool() { return activateMode(ApplicationModes.Klootviool); }

    function matchSequence(secret) {
        const tail = buffer.slice(-secret.length);
        return tail.length === secret.length && tail.every((k, i) => k === secret[i]);
    }

    function feedKey(code) {
        buffer.push(code);
        if (buffer.length > SECRET_MAX) buffer.shift();

        if (matchSequence(SECRET_DIABOLO))    { buffer.length = 0; toggleDiabolo();    return true; }
        if (matchSequence(SECRET_LION))       { buffer.length = 0; toggleLion();       return true; }
        if (matchSequence(SECRET_DEMOS))      { buffer.length = 0; toggleDemos();      return true; }
        if (matchSequence(SECRET_DYSIS))      { buffer.length = 0; toggleDysis();      return true; }
        if (matchSequence(SECRET_KWAAK))      { buffer.length = 0; toggleKwaak();      return true; }
        if (matchSequence(SECRET_KOMPAS))     { buffer.length = 0; toggleKompas();     return true; }
        if (matchSequence(SECRET_KLOOTVIOOL)) { buffer.length = 0; toggleKlootviool(); return true; }

        return false;
    }

    return {
        activeMode,
        diaboloActive, lionActive, demosActive, dysisActive,
        kwaakActive, kompasActive, klootvioolActive,
        feedKey,
    };
}