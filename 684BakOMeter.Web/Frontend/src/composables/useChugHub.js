import { ref, readonly } from 'vue';
import * as signalR from '@microsoft/signalr';

/**
 * Composable that connects to the ChugHub SignalR endpoint and exposes reactive
 * state for scale 1 and scale 2 chug sessions.
 *
 * State machine mirrored from the backend:
 *   WaitingForBaseline → ReadyToLift → Running → Completed
 */
export function useChugHub() {
    const initialScale = () => ({
        state: 'WaitingForBaseline',
        currentAverage: 0,
        baselineWeight: null,
        elapsedMs: 0,
        durationMs: null,
        isWaitingForBaseline: true,
        isReadyToLift: false,
        isRunning: false,
        isCompleted: false,
    });

    const scale1 = ref(initialScale());
    const scale2 = ref(initialScale());

    // Local timer interpolation for smooth display while Running
    let localTimer1 = null;
    let localTimer2 = null;
    let localStart1 = 0;
    let localStart2 = 0;
    const elapsed1 = ref(0);
    const elapsed2 = ref(0);

    let connection = null;
    let onCompleteFn = null;

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

    async function connect(onComplete) {
        onCompleteFn = onComplete;

        connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/chug')
            .withAutomaticReconnect()
            .build();

        connection.on('ChugUpdate', (data) => {
            const s = getScaleRef(data.scaleNumber);
            s.value = data;
        });

        connection.on('ChugStarted', (data) => {
            startLocalTimer(data.scaleNumber);
        });

        connection.on('ChugCompleted', (data) => {
            stopLocalTimer(data.scaleNumber, data.durationMs);
            if (onCompleteFn) onCompleteFn(data);
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
        scale1.value = initialScale();
        scale2.value = initialScale();
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

