<template>
    <div>
        <div class="arcade-subtitle">SCAN EEN NIEUWE NFC TAG</div>

        <div class="nfc-scan-icon mt-2">
            <svg class="nfc-icon-small arcade-blink" viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg">
                <rect x="10" y="18" width="28" height="36" rx="4" stroke="currentColor" stroke-width="2.5"/>
                <path d="M42 40a8 8 0 000-16" stroke="currentColor" stroke-width="2.5" stroke-linecap="round"/>
                <path d="M48 44a14 14 0 000-24" stroke="currentColor" stroke-width="2.5" stroke-linecap="round"/>
                <path d="M54 48a20 20 0 000-32" stroke="currentColor" stroke-width="2.5" stroke-linecap="round"/>
            </svg>
        </div>

        <div class="arcade-prompt arcade-blink mt-2" style="font-size: 0.45rem;">
            HOUD JE NIEUWE TAG TEGEN DE LEZER
        </div>

        <div v-if="error" class="nfc-error mt-1">{{ error }}</div>
        <div v-if="success" class="nfc-success mt-1">TAG GEKOPPELD!</div>

        <div class="arcade-menu arcade-menu--narrow mt-2">
            <button class="arcade-btn arcade-btn--back"
                    @click="$emit('cancel')">
                &lsaquo; ANNULEREN
            </button>
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue';

const props = defineProps({
    playerId: { type: Number, required: true },
});

const emit = defineEmits(['added', 'cancel']);

const error = ref('');
const success = ref(false);
let pollInterval = null;

async function pollForScan() {
    try {
        const res = await fetch('/api/nfc/poll-scan', { method: 'GET' });
        if (res.ok) {
            const data = await res.json();
            if (data.uid) {
                stopPolling();
                await linkTag(data.uid);
            }
        }
    } catch {
        // Silently retry
    }
}

async function linkTag(uid) {
    error.value = '';
    try {
        const res = await fetch('/api/nfc/link', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ playerId: props.playerId, uid }),
        });
        const data = await res.json();
        if (!res.ok) {
            error.value = data.error || 'Koppelen mislukt.';
            // Resume polling so the user can try a different tag
            startPolling();
            return;
        }
        success.value = true;
        setTimeout(() => emit('added'), 1500);
    } catch {
        error.value = 'Netwerkfout.';
        startPolling();
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

onMounted(() => startPolling());
onUnmounted(() => stopPolling());
</script>

<style scoped>
.nfc-scan-icon {
    display: flex;
    justify-content: center;
}
.nfc-icon-small {
    width: 50px;
    height: 50px;
    color: var(--accent-yellow);
}
.nfc-error {
    color: var(--accent-red);
    font-family: var(--font-arcade);
    font-size: 0.5rem;
    text-align: center;
}
.nfc-success {
    color: var(--accent-green);
    font-family: var(--font-arcade);
    font-size: 0.55rem;
    text-align: center;
}
</style>
