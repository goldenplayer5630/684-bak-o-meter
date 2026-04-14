<template>
    <div class="volume-settings">
        <div class="arcade-label">MUZIEK VOLUME</div>
        <div class="volume-bar mt-1">
            <input type="range" class="volume-slider"
                   min="0" max="100" step="5"
                   v-model.number="volume"
                   @input="onVolumeChange" />
            <span class="volume-value">{{ volume }}%</span>
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';

const STORAGE_KEY = 'bak-o-meter-volume';
const volume = ref(50);

function onVolumeChange() {
    localStorage.setItem(STORAGE_KEY, String(volume.value));
    // Dispatch custom event so any audio elements can react
    window.dispatchEvent(new CustomEvent('volume-change', { detail: volume.value }));
}

onMounted(() => {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored !== null) {
        volume.value = parseInt(stored, 10);
    }
});
</script>

<style scoped>
.volume-settings {
    width: 100%;
    max-width: 300px;
}
.volume-bar {
    display: flex;
    align-items: center;
    gap: 0.8rem;
}
.volume-slider {
    flex: 1;
    accent-color: var(--accent-yellow);
    cursor: pointer;
    height: 6px;
}
.volume-value {
    font-family: var(--font-arcade);
    font-size: 0.5rem;
    color: var(--accent-yellow);
    min-width: 3em;
    text-align: right;
}
</style>
