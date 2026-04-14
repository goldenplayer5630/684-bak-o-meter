<template>
    <div class="timer-display"
         :class="{
             'timer-display--running': state === 'running',
             'timer-display--stopped': state === 'stopped'
         }">
        {{ formatted }}
    </div>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
    elapsed: { type: Number, default: 0 },
    state: { type: String, default: 'idle' }, // idle | running | stopped
    small: { type: Boolean, default: false },
});

const formatted = computed(() => {
    const sec  = Math.floor(props.elapsed / 1000);
    const frac = String(props.elapsed % 1000).padStart(3, '0');
    return `${sec}.${frac}`;
});
</script>

<style scoped>
/* timer-display styles live in arcade.css */
</style>
