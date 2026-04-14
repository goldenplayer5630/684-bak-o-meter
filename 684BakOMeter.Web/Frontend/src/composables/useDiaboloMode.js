import { ref } from 'vue';

const STORAGE_DIABOLO = 'diabolo';
const STORAGE_LION    = 'lionmode';

const SECRET_DIABOLO = ['Digit6', 'Digit6', 'Digit6'];
const SECRET_LION    = ['KeyK', 'KeyL', 'KeyC'];
const SECRET_MAX     = Math.max(SECRET_DIABOLO.length, SECRET_LION.length);
const buffer         = [];

/**
 * Composable for secret easter egg modes.
 * - 666  → diabolo-mode (black + red corner + geheim-logo)
 * - klc  → lion-mode   (deep red + white leeuwgod watermark)
 * Modes are mutually exclusive; re-entering the active code reverts to normal.
 * Call `feedKey(e.code)` from every keydown handler.
 */
export function useDiaboloMode() {
    const diaboloActive = ref(document.body.classList.contains('diabolo-mode'));
    const lionActive    = ref(document.body.classList.contains('lion-mode'));

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

    function toggleDiabolo() {
        const next = !diaboloActive.value;
        if (next) setLion(false);   // mutually exclusive
        setDiabolo(next);
    }

    function toggleLion() {
        const next = !lionActive.value;
        if (next) setDiabolo(false); // mutually exclusive
        setLion(next);
    }

    /** Feed every keydown code; returns true when a sequence triggers. */
    function feedKey(code) {
        buffer.push(code);
        if (buffer.length > SECRET_MAX) buffer.shift();

        const tail666 = buffer.slice(-SECRET_DIABOLO.length);
        if (tail666.length === SECRET_DIABOLO.length && tail666.every((k, i) => k === SECRET_DIABOLO[i])) {
            buffer.length = 0;
            toggleDiabolo();
            return true;
        }

        const tailKlc = buffer.slice(-SECRET_LION.length);
        if (tailKlc.length === SECRET_LION.length && tailKlc.every((k, i) => k === SECRET_LION[i])) {
            buffer.length = 0;
            toggleLion();
            return true;
        }

        return false;
    }

    return { diaboloActive, lionActive, feedKey };
}
