import { ref } from 'vue';

const STORAGE_DIABOLO = 'diabolo';
const STORAGE_LION    = 'lionmode';
const STORAGE_DEMOS   = 'demosmode';
const STORAGE_DYSIS   = 'dysismode';
const STORAGE_KWAAK   = 'kwaakmode';
const STORAGE_KOMPAS  = 'kompasmode';

const SECRET_DIABOLO = ['Digit6', 'Digit6', 'Digit6'];
const SECRET_LION    = ['KeyK', 'KeyL', 'KeyC'];
const SECRET_DEMOS   = ['KeyD', 'KeyM', 'KeyS'];
const SECRET_DYSIS   = ['Digit4', 'Digit2', 'Digit7'];
const SECRET_KWAAK   = ['KeyK', 'KeyW', 'KeyA', 'KeyA', 'KeyK'];
const SECRET_KOMPAS  = ['KeyK', 'KeyO', 'KeyM', 'KeyP', 'KeyA', 'KeyS'];
const SECRET_MAX     = Math.max(
    SECRET_DIABOLO.length, SECRET_LION.length, SECRET_DEMOS.length,
    SECRET_DYSIS.length, SECRET_KWAAK.length, SECRET_KOMPAS.length
);
const buffer         = [];

/**
 * Composable for secret easter egg background modes.
 * - 666    → diabolo-mode (black + red corner + geheim-logo)
 * - klc    → lion-mode   (deep red + white leeuwgod watermark)
 * - dms    → demos-mode
 * - 427    → dysis-mode  (dysis.jpg background)
 * - kwaak  → kwaak-mode  (kwaak.jpg background)
 * - kompas → kompas-mode (kompas.jpg background)
 * Modes are mutually exclusive; re-entering the active code reverts to normal.
 * Call `feedKey(e.code)` from every keydown handler.
 */
export function useDifferentBgMode() {
    const diaboloActive = ref(document.body.classList.contains('diabolo-mode'));
    const lionActive    = ref(document.body.classList.contains('lion-mode'));
    const demosActive   = ref(document.body.classList.contains('demos-mode'));
    const dysisActive   = ref(document.body.classList.contains('dysis-mode'));
    const kwaakActive   = ref(document.body.classList.contains('kwaak-mode'));
    const kompasActive  = ref(document.body.classList.contains('kompas-mode'));

    function clearAll() {
        setDiabolo(false); setLion(false); setDemos(false);
        setDysis(false); setKwaak(false); setKompas(false);
    }

    function setDiabolo(on) {
        diaboloActive.value = on;
        document.body.classList.toggle('diabolo-mode', on);
        localStorage.setItem(STORAGE_DIABOLO, on ? '1' : '0');
    }

    function setLion(on) {
        lionActive.value = on;
        document.body.classList.toggle('lion-mode', on);
        localStorage.setItem(STORAGE_LION, on ? '1' : '0');
    }

    function setDemos(on) {
        demosActive.value = on;
        document.body.classList.toggle('demos-mode', on);
        localStorage.setItem(STORAGE_DEMOS, on ? '1' : '0');
    }

    function setDysis(on) {
        dysisActive.value = on;
        document.body.classList.toggle('dysis-mode', on);
        localStorage.setItem(STORAGE_DYSIS, on ? '1' : '0');
    }

    function setKwaak(on) {
        kwaakActive.value = on;
        document.body.classList.toggle('kwaak-mode', on);
        localStorage.setItem(STORAGE_KWAAK, on ? '1' : '0');
    }

    function setKompas(on) {
        kompasActive.value = on;
        document.body.classList.toggle('kompas-mode', on);
        localStorage.setItem(STORAGE_KOMPAS, on ? '1' : '0');
    }

    function toggleDiabolo() { const next = !diaboloActive.value; if (next) clearAll(); setDiabolo(next); }
    function toggleLion()    { const next = !lionActive.value;    if (next) clearAll(); setLion(next); }
    function toggleDemos()   { const next = !demosActive.value;   if (next) clearAll(); setDemos(next); }
    function toggleDysis()   { const next = !dysisActive.value;   if (next) clearAll(); setDysis(next); }
    function toggleKwaak()   { const next = !kwaakActive.value;   if (next) clearAll(); setKwaak(next); }
    function toggleKompas()  { const next = !kompasActive.value;  if (next) clearAll(); setKompas(next); }

    function matchSequence(secret) {
        const tail = buffer.slice(-secret.length);
        return tail.length === secret.length && tail.every((k, i) => k === secret[i]);
    }

    /** Feed every keydown code; returns true when a sequence triggers. */
    function feedKey(code) {
        buffer.push(code);
        if (buffer.length > SECRET_MAX) buffer.shift();

        if (matchSequence(SECRET_DIABOLO)) { buffer.length = 0; toggleDiabolo(); return true; }
        if (matchSequence(SECRET_LION))    { buffer.length = 0; toggleLion();    return true; }
        if (matchSequence(SECRET_DEMOS))   { buffer.length = 0; toggleDemos();   return true; }
        if (matchSequence(SECRET_DYSIS))   { buffer.length = 0; toggleDysis();   return true; }
        if (matchSequence(SECRET_KWAAK))   { buffer.length = 0; toggleKwaak();   return true; }
        if (matchSequence(SECRET_KOMPAS))  { buffer.length = 0; toggleKompas();  return true; }

        return false;
    }

    return { diaboloActive, lionActive, demosActive, dysisActive, kwaakActive, kompasActive, feedKey };
}
