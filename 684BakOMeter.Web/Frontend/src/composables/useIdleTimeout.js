import { ref, onUnmounted } from 'vue';

/**
 * Manages an idle timeout that fires a callback after a period of inactivity.
 *
 * @param {number} timeoutMs - milliseconds of idle before triggering
 * @param {Function} onTimeout - callback when idle timeout fires
 */
export function useIdleTimeout(timeoutMs, onTimeout) {
    let timer = null;
    const isIdle = ref(false);

    function start() {
        stop();
        isIdle.value = false;
        timer = setTimeout(() => {
            isIdle.value = true;
            onTimeout?.();
        }, timeoutMs);
    }

    function reset() {
        start();
    }

    function stop() {
        if (timer) {
            clearTimeout(timer);
            timer = null;
        }
        isIdle.value = false;
    }

    onUnmounted(() => stop());

    return { isIdle, start, reset, stop };
}
