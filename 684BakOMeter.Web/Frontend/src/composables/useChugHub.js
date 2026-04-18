import { ref, readonly } from 'vue';
import * as signalR from '@microsoft/signalr';

/**
 * Composable that connects to the ChugHub SignalR endpoint
 * and exposes reactive state for scale 1 and scale 2 chug sessions.
 */
export function useChugHub() {
    const scale1 = ref({ state: 'WaitingForFull', currentAverage: 0, elapsedMs: 0, durationMs: null });
    const scale2 = ref({ state: 'WaitingForFull', currentAverage: 0, elapsedMs: 0, durationMs: null });

    // Local timer interpolation for smooth display while Running
    let localTimer1 = null;
    let localTimer2 = null;
    let localStart1 = 0;
    let localStart2 = 0;
    const elapsed1 = ref(0);
    const elapsed2 = ref(0);

    let connection = null;
    let onCompleteFn = null;
    let onInvalidFn = null;

    function getScaleRef(scaleNumber) {
        return scaleNumber === 1 ? scale1 : scale2;
    }

    function startLocalTimer(scaleNumber) {
        if (scaleNumber === 1) {
            localStart1 = performance.now();
            clearInterval(localTimer1);
            localTimer1 = setInterval(() => {
                elapsed1.value = Math.round(performance.now() - localStart1);
            }, 31);
        } else {
            localStart2 = performance.now();
            clearInterval(localTimer2);
            localTimer2 = setInterval(() => {
                elapsed2.value = Math.round(performance.now() - localStart2);
            }, 31);
        }
    }

    function stopLocalTimer(scaleNumber, finalMs) {
        if (scaleNumber === 1) {
            clearInterval(localTimer1);
            localTimer1 = null;
            elapsed1.value = finalMs;
        } else {
            clearInterval(localTimer2);
            localTimer2 = null;
            elapsed2.value = finalMs;
        }
    }

    async function connect(onComplete, onInvalid) {
        onCompleteFn = onComplete;
        onInvalidFn = onInvalid ?? null;

        connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/chug')
            .withAutomaticReconnect()
            .build();

        connection.on('ChugUpdate', (data) => {
            const s = getScaleRef(data.scaleNumber);
            s.value = data;

            // Freeze the local timer as soon as the glass is placed back (validating)
            if (data.state === 'Validating' && data.durationMs != null) {
                stopLocalTimer(data.scaleNumber, data.durationMs);
            }
        });

        connection.on('ChugStarted', (data) => {
            startLocalTimer(data.scaleNumber);
        });

        connection.on('ChugTimerStop', (data) => {
            stopLocalTimer(data.scaleNumber, data.durationMs);
        });

        connection.on('ChugCompleted', (data) => {
            stopLocalTimer(data.scaleNumber, data.durationMs);
            if (onCompleteFn) onCompleteFn(data);
        });

        connection.on('ChugInvalid', (data) => {
            stopLocalTimer(data.scaleNumber, 0);
            if (onInvalidFn) onInvalidFn(data);
        });

        await connection.start();
    }

    async function disconnect() {
        clearInterval(localTimer1);
        clearInterval(localTimer2);
        localTimer1 = null;
        localTimer2 = null;
        if (connection) {
            await connection.stop();
            connection = null;
        }
    }

    function reset() {
        scale1.value = { state: 'WaitingForFull', currentAverage: 0, elapsedMs: 0, durationMs: null };
        scale2.value = { state: 'WaitingForFull', currentAverage: 0, elapsedMs: 0, durationMs: null };
        elapsed1.value = 0;
        elapsed2.value = 0;
        clearInterval(localTimer1);
        clearInterval(localTimer2);
        localTimer1 = null;
        localTimer2 = null;
    }

    return {
        scale1: readonly(scale1),
        scale2: readonly(scale2),
        elapsed1,
        elapsed2,
        connect,
        disconnect,
        reset,
    };
}
