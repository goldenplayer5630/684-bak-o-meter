<template>
    <div class="arcade-page">
        <h1 class="arcade-title arcade-title--small">PERSOONLIJKE STATS</h1>

        <!-- Name input phase -->
        <template v-if="phase === 'input'">
            <div class="arcade-input-group mt-2">
                <label class="arcade-label">JOUW NAAM</label>
                <input ref="inputRef"
                       class="arcade-input"
                       type="text"
                       v-model="nameInput"
                       placeholder="typ je naam..."
                       maxlength="20"
                       autocomplete="off"
                       @keydown.enter="lookup" />
            </div>

            <div v-if="error" class="ps-error mt-1">{{ error }}</div>

            <div class="arcade-menu arcade-menu--narrow mt-3">
                <button class="arcade-btn arcade-btn--primary"
                        :class="{ selected: btnIdx === 0 }"
                        :disabled="loading"
                        @click="lookup"
                        @mouseenter="btnIdx = 0">
                    <svg class="btn-icon" viewBox="0 0 24 24" fill="none">
                        <circle cx="11" cy="11" r="7" stroke="currentColor" stroke-width="1.8"/>
                        <path d="M16.5 16.5l4 4" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>
                    </svg>
                    {{ loading ? 'ZOEKEN...' : 'ZOEKEN' }}
                </button>
                <button class="arcade-btn arcade-btn--back"
                        :class="{ selected: btnIdx === 1 }"
                        @click="goBack"
                        @mouseenter="btnIdx = 1">
                    &lsaquo; TERUG
                </button>
            </div>
        </template>

        <!-- Results phase -->
        <template v-if="phase === 'results'">
            <div class="ps-player-name mt-1">{{ playerName }}</div>

            <div class="leaderboard-panel ps-panel">
                <div v-if="stats.length === 0" class="leaderboard-empty">
                    Nog geen scores voor {{ playerName }}
                </div>
                <template v-else>
                    <div v-for="s in stats" :key="s.slug" class="ps-stat-row">
                        <span class="ps-stat-label">{{ s.label }}</span>
                        <span class="ps-stat-rank" :class="rankClass(s.rank)">#{{ s.rank }}</span>
                        <span class="ps-stat-of">/ {{ s.totalPlayers }}</span>
                        <span class="ps-stat-time">{{ s.best }}</span>
                    </div>
                </template>
            </div>

            <div class="arcade-menu arcade-menu--narrow mt-2">
                <button class="arcade-btn arcade-btn--back"
                        :class="{ selected: btnIdx === 0 }"
                        @click="reset"
                        @mouseenter="btnIdx = 0">
                    &#8635; ANDERE NAAM
                </button>
                <button class="arcade-btn arcade-btn--back"
                        :class="{ selected: btnIdx === 1 }"
                        @click="goBack"
                        @mouseenter="btnIdx = 1">
                    &lsaquo; TERUG
                </button>
            </div>
        </template>

        <!-- Key hint -->
        <div class="arcade-subtitle" style="font-size:.4rem;">
            &#8593;&#8595; NAVIGEREN &bull; ENTER ZOEKEN &bull; ESC TERUG
        </div>
    </div>
</template>

<script setup>
import { ref, nextTick, onMounted, onUnmounted } from 'vue';

const phase     = ref('input');
const nameInput = ref('');
const loading   = ref(false);
const error     = ref('');
const playerName = ref('');
const stats     = ref([]);
const btnIdx    = ref(0);
const inputRef  = ref(null);

async function lookup() {
    const name = nameInput.value.trim();
    if (!name) { error.value = 'Voer een naam in.'; return; }

    loading.value = true;
    error.value   = '';

    try {
        const r = await fetch(`/api/personal?name=${encodeURIComponent(name)}`);
        if (r.status === 404) {
            error.value = 'Speler niet gevonden. Heb je al een keer gespeeld?';
            return;
        }
        if (!r.ok) {
            error.value = 'Er ging iets mis. Probeer opnieuw.';
            return;
        }
        const d = await r.json();
        playerName.value = d.playerName;
        stats.value      = d.stats ?? [];
        phase.value      = 'results';
        btnIdx.value     = 0;
    } catch {
        error.value = 'Verbindingsfout. Probeer opnieuw.';
    } finally {
        loading.value = false;
    }
}

function reset() {
    phase.value     = 'input';
    nameInput.value = '';
    error.value     = '';
    btnIdx.value    = 0;
    nextTick(() => inputRef.value?.focus());
}

function goBack() { window.location.href = '/'; }

function rankClass(rank) {
    if (rank === 1) return 'ps-rank--gold';
    if (rank === 2) return 'ps-rank--silver';
    if (rank === 3) return 'ps-rank--bronze';
    return '';
}

// Keyboard
function onKey(e) {
    if (e.code === 'Escape') { e.preventDefault(); goBack(); return; }

    if (phase.value === 'input') {
        const total = 2; // search + back
        if (e.code === 'ArrowDown') {
            e.preventDefault(); btnIdx.value = Math.min(btnIdx.value + 1, total - 1);
        } else if (e.code === 'ArrowUp') {
            e.preventDefault(); btnIdx.value = Math.max(btnIdx.value - 1, 0);
        } else if (e.code === 'Space' || e.code === 'Enter') {
            if (btnIdx.value === 1) { e.preventDefault(); goBack(); }
            // Enter on input field is handled by @keydown.enter
        }
    } else {
        // results phase: 2 buttons (reset + back)
        const total = 2;
        if (e.code === 'ArrowDown') {
            e.preventDefault(); btnIdx.value = Math.min(btnIdx.value + 1, total - 1);
        } else if (e.code === 'ArrowUp') {
            e.preventDefault(); btnIdx.value = Math.max(btnIdx.value - 1, 0);
        } else if (e.code === 'Space' || e.code === 'Enter') {
            e.preventDefault();
            if (btnIdx.value === 0) reset();
            else goBack();
        }
    }
}

onMounted(() => {
    document.addEventListener('keydown', onKey);
    nextTick(() => inputRef.value?.focus());
});
onUnmounted(() => document.removeEventListener('keydown', onKey));
</script>

<style scoped>
.ps-player-name {
    font-family: var(--font-arcade);
    font-size: 0.9rem;
    color: var(--accent-yellow);
    text-shadow: var(--glow-yellow);
    letter-spacing: 1px;
    text-align: center;
}

.ps-panel {
    width: 100%;
    max-width: 420px;
    padding: 0.8rem 1rem;
}

.ps-stat-row {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.35rem 0.4rem;
    border-radius: 5px;
    margin-bottom: 0.2rem;
}

.ps-stat-row:nth-child(odd) {
    background: rgba(255,255,255,.03);
}

.ps-stat-label {
    font-family: var(--font-arcade);
    font-size: 0.6rem;
    color: var(--text-primary);
    flex: 1;
    letter-spacing: 0.5px;
}

.ps-stat-rank {
    font-family: var(--font-arcade);
    font-size: 0.75rem;
    color: var(--text-muted);
    min-width: 2.8rem;
    text-align: right;
}

.ps-rank--gold   { color: #ffd700; text-shadow: var(--glow-yellow); }
.ps-rank--silver { color: #c0c0c0; }
.ps-rank--bronze { color: #cd7f32; }

.ps-stat-of {
    font-family: var(--font-arcade);
    font-size: 0.5rem;
    color: var(--text-muted);
    min-width: 2rem;
}

.ps-stat-time {
    font-family: var(--font-arcade);
    font-size: 0.65rem;
    color: var(--accent-green);
    min-width: 5rem;
    text-align: right;
}

.ps-error {
    font-family: var(--font-arcade);
    font-size: 0.6rem;
    color: var(--accent-red);
    text-align: center;
    letter-spacing: 0.5px;
}
</style>
