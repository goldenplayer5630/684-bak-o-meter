<template>
    <div class="arcade-page">
        <h1 class="arcade-title arcade-title--small">INSTELLINGEN</h1>

        <div class="settings-section mt-3">
            <VolumeSettings />
        </div>

        <div class="arcade-menu arcade-menu--narrow mt-3">
            <button class="arcade-btn arcade-btn--back"
                    :class="{ selected: true }"
                    @click="goBack">
                &lsaquo; TERUG (DRUK ESC)
            </button>
        </div>

        <!-- Key hint -->
        <div class="arcade-subtitle" style="font-size:.4rem;">
            ESC TERUG
        </div>
    </div>
</template>

<script setup>
import { onMounted, onUnmounted } from 'vue';
import VolumeSettings from './VolumeSettings.vue';
import { useDifferentBgMode } from '../composables/useDifferentBgMode.js';
import { useBgMusic } from '../composables/useBgMusic.js';

useBgMusic('/music/trekeenbak.mp3');

function goBack() {
    window.location.href = '/';
}

const bgMode = useDifferentBgMode();

function onKeyDown(e) {
    bgMode.feedKey(e.code);

    if (e.code === 'Escape') {
        e.preventDefault();
        goBack();
    }
}

onMounted(() => {
    document.addEventListener('keydown', onKeyDown);
});

onUnmounted(() => {
    document.removeEventListener('keydown', onKeyDown);
});
</script>

<style scoped>
.settings-section {
    width: 100%;
    max-width: 340px;
    margin-left: auto;
    margin-right: auto;
}
</style>
