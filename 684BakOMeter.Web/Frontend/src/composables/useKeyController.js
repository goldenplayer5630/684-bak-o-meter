import { onMounted, onUnmounted } from 'vue';
import { useDifferentBgMode } from './useDifferentBgMode.js';

/**
 * Centralized keyboard controller composable.
 *
 * Automatically registers/unregisters the keydown listener, feeds the
 * diabolo easter-egg tracker, and dispatches key events to a set of
 * named action handlers.
 *
 * @param {Object} options
 * @param {Function} [options.onEscape]        — called on Escape
 * @param {Function} [options.onUp]            — called on ArrowUp
 * @param {Function} [options.onDown]          — called on ArrowDown
 * @param {Function} [options.onLeft]          — called on ArrowLeft
 * @param {Function} [options.onRight]         — called on ArrowRight
 * @param {Function} [options.onActivate]      — called on Space or Enter
 * @param {Function} [options.onKey]           — catch-all, called with the
 *                                               KeyboardEvent for every key.
 *                                               Runs *before* the named actions.
 *                                               Return `true` to suppress the
 *                                               default named-action dispatch.
 * @param {Function} [options.feedSecrets]     — optional extra secret-code
 *                                               handler (e.g. ArcadeMenu 684).
 *                                               Receives `e.code` and `e`.
 * @param {boolean}  [options.enableModeSwitching=false]
 *                                               when true, feeds global mode
 *                                               secret sequences (DMS/666/etc).
 *
 * All named action callbacks receive the raw KeyboardEvent so the
 * component can inspect modifiers, do conditional logic, etc.
 * `preventDefault()` is called automatically for recognised keys.
 *
 * Usage:
 * ```js
 * useKeyController({
 *     onEscape: () => goBack(),
 *     onUp:    (e) => moveUp(),
 *     onDown:  (e) => moveDown(),
 *     onActivate: (e) => activate(),
 * });
 * ```
 */
export function useKeyController(options = {}) {
    const bgMode = useDifferentBgMode();

    const ACTION_MAP = {
        Escape:     'onEscape',
        ArrowUp:    'onUp',
        ArrowDown:  'onDown',
        ArrowLeft:  'onLeft',
        ArrowRight: 'onRight',
        Space:      'onActivate',
        Enter:      'onActivate',
    };

    function handleKeyDown(e) {
        const target = e.target;
        const isTypingTarget = target instanceof HTMLInputElement
            || target instanceof HTMLTextAreaElement
            || target instanceof HTMLSelectElement
            || (target instanceof HTMLElement && target.isContentEditable);

        // Feed easter-egg trackers
        if (options.enableModeSwitching === true) {
            bgMode.feedKey(e.code);
        }
        options.feedSecrets?.(e.code, e);

        // Let the component intercept any key before named dispatch
        if (options.onKey?.(e) === true) return;

        // Do not hijack Enter/arrow keys while typing in inputs.
        if (isTypingTarget) return;

        const actionName = ACTION_MAP[e.code];
        if (!actionName) return;

        const handler = options[actionName];
        if (handler) {
            e.preventDefault();
            handler(e);
        }
    }

    onMounted(() => document.addEventListener('keydown', handleKeyDown));
    onUnmounted(() => document.removeEventListener('keydown', handleKeyDown));
}
