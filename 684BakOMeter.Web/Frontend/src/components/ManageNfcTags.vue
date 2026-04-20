<template>
    <div>
        <div class="arcade-label">NFC TAGS</div>

        <!-- Adding mode -->
        <template v-if="adding">
            <AddNfcTag :playerId="playerId"
                       @added="onTagAdded"
                       @cancel="adding = false" />
        </template>

        <!-- Normal list mode -->
        <template v-else>
            <div v-if="loading" class="nfc-loading mt-1">Laden...</div>
            <div v-else-if="error" class="nfc-error mt-1">{{ error }}</div>
            <div v-else-if="tags.length === 0" class="nfc-empty mt-1">Geen tags gevonden.</div>
            <div v-else class="nfc-tag-list mt-1">
                <div v-for="tag in tags" :key="tag.id" class="nfc-tag-row">
                    <span class="nfc-tag-uid">{{ tag.uid }}</span>
                    <span v-if="tag.label" class="nfc-tag-label">{{ tag.label }}</span>
                    <button class="nfc-tag-remove"
                            :disabled="tags.length <= 1 || removing"
                            :title="tags.length <= 1 ? 'Laatste tag kan niet worden verwijderd' : 'Verwijderen'"
                            @click="removeTag(tag.id)">
                        ✕
                    </button>
                </div>
            </div>
            <div class="arcade-menu arcade-menu--narrow mt-2">
                <button ref="addBtnRef"
                        class="arcade-btn arcade-btn--primary"
                        :class="{ selected: keyboardSelected }"
                        @click="adding = true">
                    + TAG TOEVOEGEN
                </button>
            </div>
        </template>
    </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import AddNfcTag from './AddNfcTag.vue';

const props = defineProps({
    playerId: { type: Number, required: true },
    keyboardSelected: { type: Boolean, default: false },
});

const tags = ref([]);
const loading = ref(true);
const error = ref('');
const removing = ref(false);
const adding = ref(false);
const addBtnRef = ref(null);

async function loadTags() {
    loading.value = true;
    error.value = '';
    try {
        const res = await fetch(`/api/nfc/by-player/${props.playerId}`);
        if (res.ok) {
            tags.value = await res.json();
        }
    } catch {
        error.value = 'Laden mislukt.';
    } finally {
        loading.value = false;
    }
}

async function removeTag(tagId) {
    if (removing.value) return;
    removing.value = true;
    error.value = '';

    try {
        const res = await fetch(`/api/nfc/${tagId}?playerId=${props.playerId}`, {
            method: 'DELETE',
        });
        const data = await res.json();
        if (!res.ok) {
            error.value = data.error || 'Verwijderen mislukt.';
            return;
        }
        await loadTags();
    } catch {
        error.value = 'Netwerkfout.';
    } finally {
        removing.value = false;
    }
}

function onTagAdded() {
    adding.value = false;
    loadTags();
}

function activatePrimaryAction() {
    if (adding.value || loading.value) return;
    addBtnRef.value?.click();
}

defineExpose({
    activatePrimaryAction,
});

onMounted(() => loadTags());
</script>

<style scoped>
.nfc-loading, .nfc-empty {
    font-family: var(--font-arcade);
    font-size: 0.45rem;
    color: var(--text-muted);
    text-align: center;
}
.nfc-tag-list {
    display: flex;
    flex-direction: column;
    gap: 0.4rem;
    max-width: 300px;
    margin: 0 auto;
}
.nfc-tag-row {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    background: var(--bg-card);
    border-radius: var(--radius);
    padding: 0.4rem 0.6rem;
    font-family: var(--font-arcade);
    font-size: 0.4rem;
}
.nfc-tag-uid {
    flex: 1;
    color: var(--accent-yellow);
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}
.nfc-tag-label {
    color: var(--text-muted);
    font-size: 0.35rem;
}
.nfc-tag-remove {
    background: none;
    border: 1px solid var(--accent-red);
    color: var(--accent-red);
    font-family: var(--font-arcade);
    font-size: 0.4rem;
    padding: 0.15rem 0.3rem;
    border-radius: 4px;
    cursor: pointer;
}
.nfc-tag-remove:hover:not(:disabled) {
    background: var(--accent-red);
    color: var(--bg-dark);
}
.nfc-tag-remove:disabled {
    opacity: 0.25;
    cursor: not-allowed;
    border-color: var(--text-muted);
    color: var(--text-muted);
}
.nfc-error {
    color: var(--accent-red);
    font-family: var(--font-arcade);
    font-size: 0.45rem;
    text-align: center;
}
</style>
