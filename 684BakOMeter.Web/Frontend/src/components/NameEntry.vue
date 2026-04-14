<template>
    <div>
        <div class="arcade-input-group mt-3">
            <label class="arcade-label">{{ multiplayer ? 'SPELER 1' : 'NAAM' }}</label>
            <input ref="input1" class="arcade-input" type="text"
                   v-model="name1" placeholder="typ je naam..."
                   @keydown.enter="submit" maxlength="20" autocomplete="off" />
        </div>

        <div v-if="multiplayer" class="arcade-input-group mt-2">
            <label class="arcade-label">SPELER 2</label>
            <input ref="input2" class="arcade-input" type="text"
                   v-model="name2" placeholder="typ je naam..."
                   @keydown.enter="submit" maxlength="20" autocomplete="off" />
        </div>

        <div v-if="error" class="name-error mt-2">{{ error }}</div>
    </div>
</template>

<script setup>
import { ref, onMounted, nextTick } from 'vue';

const props = defineProps({
    multiplayer: { type: Boolean, default: false },
});

const emit = defineEmits(['submit']);

const name1 = ref('');
const name2 = ref('');
const error = ref('');
const input1 = ref(null);
const input2 = ref(null);

function submit() {
    error.value = '';
    const n1 = name1.value.trim().toLowerCase();
    if (!n1) { error.value = 'Voer een naam in.'; return; }

    if (props.multiplayer) {
        const n2 = name2.value.trim().toLowerCase();
        if (!n2) { error.value = 'Voer beide namen in.'; return; }
        if (n1 === n2) { error.value = 'Namen moeten verschillend zijn.'; return; }
        name1.value = n1;
        name2.value = n2;
        emit('submit', { name1: n1, name2: n2 });
    } else {
        name1.value = n1;
        emit('submit', { name1: n1, name2: null });
    }
}

function focusFirst() {
    nextTick(() => { input1.value?.focus(); });
}

onMounted(() => focusFirst());

defineExpose({ submit, focusFirst });
</script>

<style scoped>
.name-error {
    color: var(--accent-red);
    font-family: var(--font-arcade);
    font-size: 0.6rem;
    text-align: center;
}
</style>
