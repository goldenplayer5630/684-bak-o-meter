<template>
    <div class="arcade-page">
        <!-- WIZARD STEPS -->
        <template v-if="step < steps.length">
            <h1 class="arcade-title arcade-title--small" style="color: var(--accent-purple);">
                KALIBRATIE {{ step + 1 }}/{{ steps.length }}
            </h1>

            <h2 class="arcade-subtitle mt-2" style="font-size:.55rem;">
                {{ steps[step].title }}
            </h2>

            <div class="arcade-subtitle mt-1" style="font-size:.4rem; opacity:.7;">
                {{ steps[step].instruction }}
            </div>

            <div class="calibration-live mt-2">
                {{ liveValue.toFixed(0) }}
            </div>

            <div v-if="captured" class="arcade-subtitle mt-1" style="color: var(--accent-green, #0f0);">
                ✓ OPGESLAGEN: {{ captured.toFixed(0) }}
            </div>

            <div class="arcade-prompt arcade-blink mt-2">
                DRUK OP SPATIE OM WAARDE OP TE SLAAN
            </div>

            <div class="arcade-subtitle mt-2" style="font-size:.35rem; opacity:.5;">
                ESC OM TE ANNULEREN
            </div>
        </template>

        <!-- OVERVIEW / CONFIRM -->
        <template v-else>
            <h1 class="arcade-title arcade-title--small" style="color: var(--accent-purple);">
                KALIBRATIE OVERZICHT
            </h1>

            <div class="calibration-overview mt-2">
                <div v-for="(s, i) in steps" :key="i" class="calibration-row">
                    <span class="calibration-label">{{ s.title }}</span>
                    <span class="calibration-value">{{ values[s.key]?.toFixed(0) ?? '—' }}</span>
                </div>
            </div>

            <div class="arcade-prompt arcade-blink mt-2">
                DRUK OP SPATIE OM OP TE SLAAN
            </div>

            <div v-if="saving" class="arcade-subtitle mt-1" style="color: var(--accent-green, #0f0);">
                OPSLAAN...
            </div>
            <div v-if="saved" class="arcade-subtitle mt-1" style="color: var(--accent-green, #0f0);">
                ✓ OPGESLAGEN! TERUG OVER 3 SECONDEN...
            </div>

            <div class="arcade-subtitle mt-2" style="font-size:.35rem; opacity:.5;">
                ESC OM TE ANNULEREN
            </div>
        </template>
    </div>
</template>

<script setup>
import { ref, onUnmounted } from 'vue';
import * as signalR from '@microsoft/signalr';
import { useKeyController } from '../composables/useKeyController.js';
import { useBgMusic } from '../composables/useBgMusic.js';

useBgMusic('/music/trekeenbak.mp3');

const steps = [
    { key: 'nothing',    title: 'NIETS',       instruction: 'Zorg dat er NIETS op de weegschaal staat.' },
    { key: 'emptyGlass', title: 'LEEG GLAS',   instruction: 'Plaats een LEEG GLAS op de weegschaal.' },
    { key: 'fullGlass',  title: 'VOL GLAS',    instruction: 'Plaats een VOL GLAS op de weegschaal.' },
    { key: 'emptyPul',   title: 'LEGE PUL',    instruction: 'Plaats een LEGE PUL op de weegschaal.' },
    { key: 'fullPul',    title: 'VOLLE PUL',   instruction: 'Plaats een VOLLE PUL op de weegschaal.' },
];

const step = ref(0);
const values = ref({});
const captured = ref(null);
const saving = ref(false);
const saved = ref(false);
const liveValue = ref(0);

// Debounce guard: prevent double capture from a held space bar
let lastCaptureTime = 0;

// Direct SignalR connection listening to the raw scale broadcast
let connection = null;

async function connectRaw() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/chug')
        .withAutomaticReconnect()
        .build();

    connection.on('ScaleRaw', (data) => {
        if (data.scaleNumber === 1) {
            liveValue.value = data.value;
        }
    });

    await connection.start();
}

connectRaw();

onUnmounted(() => {
    connection?.stop();
});

async function saveCalibration() {
    saving.value = true;
    try {
        await fetch('/api/calibration', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(values.value),
        });
        saved.value = true;
        setTimeout(() => { window.location.href = '/'; }, 3000);
    } catch {
        saving.value = false;
    }
}

function handleActivate() {
    const now = Date.now();
    if (now - lastCaptureTime < 500) return;
    lastCaptureTime = now;

    if (step.value < steps.length) {
        const key = steps[step.value].key;
        values.value[key] = liveValue.value;
        captured.value = liveValue.value;
        setTimeout(() => {
            captured.value = null;
            step.value++;
        }, 800);
    } else if (!saving.value && !saved.value) {
        saveCalibration();
    }
}

useKeyController({
    onActivate: () => handleActivate(),
    onEscape: () => { window.location.href = '/'; },
});
</script>

<style scoped>
.calibration-live {
    font-size: 2rem;
    font-weight: bold;
    color: var(--accent-cyan, #0ff);
    font-family: 'Press Start 2P', monospace;
    text-align: center;
}

.calibration-overview {
    display: flex;
    flex-direction: column;
    gap: 0.4rem;
    width: 100%;
    max-width: 400px;
}

.calibration-row {
    display: flex;
    justify-content: space-between;
    padding: 0.2rem 0.5rem;
    font-size: 0.45rem;
    font-family: 'Press Start 2P', monospace;
}

.calibration-label {
    color: var(--text-secondary, #ccc);
}

.calibration-value {
    color: var(--accent-cyan, #0ff);
    font-weight: bold;
}
</style>
