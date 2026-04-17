<template>
    <div class="arcade-page">
        <h1 class="arcade-title arcade-title--small">SCAN JE NFC TAG</h1>

        <div class="nfc-scan-icon mt-3">
            <!-- NFC / contactless icon -->
            <svg class="nfc-icon arcade-blink" viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg">
                <rect x="10" y="18" width="28" height="36" rx="4" stroke="currentColor" stroke-width="2.5"/>
                <path d="M42 40a8 8 0 000-16" stroke="currentColor" stroke-width="2.5" stroke-linecap="round"/>
                <path d="M48 44a14 14 0 000-24" stroke="currentColor" stroke-width="2.5" stroke-linecap="round"/>
                <path d="M54 48a20 20 0 000-32" stroke="currentColor" stroke-width="2.5" stroke-linecap="round"/>
            </svg>
        </div>

        <div class="arcade-prompt arcade-blink mt-3">
            HOUD JE TAG TEGEN DE LEZER
        </div>

        <div v-if="error" class="nfc-error mt-2">{{ error }}</div>

        <div class="arcade-menu arcade-menu--narrow mt-3">
            <button class="arcade-btn arcade-btn--back"
                    :class="{ selected: true }"
                    @click="$emit('back')">
                &lsaquo; TERUG (DRUK ESC)
            </button>
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue';
import { useKeyController } from '../composables/useKeyController.js';

const emit = defineEmits(['scanned', 'back']);

const error = ref('');
let pollInterval = null;

useKeyController({
    onEscape: () => emit('back'),
});

// Poll the backend for NFC scan events.
// In a production system this would use WebSocket/SignalR from the serial reader.
// For now we poll a simple endpoint that the serial bridge POSTs scanned UIDs to.
async function pollForScan() {
    try {
        const res = await fetch('/api/nfc/poll-scan', { method: 'GET' });
        if (res.ok) {
            const data = await res.json();
            if (data.uid) {
                stopPolling();
                emit('scanned', data.uid);
            }
        }
    } catch {
        // Silently retry on network errors
    }
}

function startPolling() {
    pollInterval = setInterval(pollForScan, 500);
}

function stopPolling() {
    if (pollInterval) {
        clearInterval(pollInterval);
        pollInterval = null;
    }
}

onMounted(() => { startPolling(); });
onUnmounted(() => { stopPolling(); });
</script>

<style scoped>
.nfc-scan-icon {
    display: flex;
    justify-content: center;
}
.nfc-icon {
    width: 80px;
    height: 80px;
    color: var(--accent-yellow);
}
.nfc-error {
    color: var(--accent-red);
    font-family: var(--font-arcade);
    font-size: 0.55rem;
    text-align: center;
}
</style>
