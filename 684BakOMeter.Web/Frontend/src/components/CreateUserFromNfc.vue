<template>
    <div class="arcade-page">
        <h1 class="arcade-title arcade-title--small">NIEUWE SPELER</h1>
        <div class="arcade-subtitle mt-2">ONBEKENDE TAG — MAAK EEN ACCOUNT</div>

        <div class="arcade-input-group mt-3">
            <label class="arcade-label">KIES JE NAAM</label>
            <input ref="nameInput" class="arcade-input" type="text"
                   v-model="name" placeholder="typ je naam..."
                   @keydown.enter="submit" maxlength="20" autocomplete="off" />
        </div>

        <div v-if="error" class="create-error mt-2">{{ error }}</div>

        <div class="arcade-menu arcade-menu--narrow mt-3">
            <button class="arcade-btn arcade-btn--primary"
                    :class="{ selected: btnIndex === 0 }"
                    @click="submit"
                    @mouseenter="btnIndex = 0">
                <!-- checkmark icon -->
                <svg class="btn-icon" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <path d="M5 13l4 4L19 7" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
                </svg>
                AANMAKEN
            </button>
            <button class="arcade-btn arcade-btn--back"
                    :class="{ selected: btnIndex === 1 }"
                    @click="$emit('cancel')"
                    @mouseenter="btnIndex = 1">
                &lsaquo; ANNULEREN
            </button>
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, nextTick } from 'vue';

const props = defineProps({
    tagUid: { type: String, required: true },
});

const emit = defineEmits(['created', 'cancel']);

const name = ref('');
const error = ref('');
const btnIndex = ref(0);
const nameInput = ref(null);
const submitting = ref(false);

async function submit() {
    error.value = '';
    const trimmed = name.value.trim();

    if (!trimmed) {
        error.value = 'Voer een naam in.';
        return;
    }

    if (submitting.value) return;
    submitting.value = true;

    try {
        const res = await fetch('/api/users/create-from-nfc', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ name: trimmed, tagUid: props.tagUid }),
        });

        const data = await res.json();

        if (!res.ok) {
            error.value = data.error || 'Er is iets misgegaan.';
            return;
        }

        emit('created', {
            playerId: data.playerId,
            playerName: data.playerName,
        });
    } catch {
        error.value = 'Netwerkfout. Probeer opnieuw.';
    } finally {
        submitting.value = false;
    }
}

function onKeyDown(e) {
    if (e.code === 'Escape') {
        e.preventDefault();
        emit('cancel');
    }
    if (e.code === 'ArrowUp') { e.preventDefault(); btnIndex.value = Math.max(0, btnIndex.value - 1); }
    if (e.code === 'ArrowDown') { e.preventDefault(); btnIndex.value = Math.min(1, btnIndex.value + 1); }
}

onMounted(() => {
    document.addEventListener('keydown', onKeyDown);
    nextTick(() => nameInput.value?.focus());
});

onUnmounted(() => {
    document.removeEventListener('keydown', onKeyDown);
});
</script>

<style scoped>
.create-error {
    color: var(--accent-red);
    font-family: var(--font-arcade);
    font-size: 0.55rem;
    text-align: center;
}
</style>
