<template>
    <div class="arcade-page">
        <!-- PHASE 1: Attract Mode -->
        <template v-if="phase === 'attract'">
            <h1 class="arcade-title bg-primary">DEMOS =</h1>
            <h2 class="arcade-title">BAK-O-METER</h2>

            <div class="leaderboard-panel mt-2">
                <div class="leaderboard-title">
                    <svg class="btn-icon" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <path d="M8 21h8M12 17v4M5 3h14l-1 8a6 6 0 01-12 0L5 3z" stroke="#ffd700" stroke-width="1.8" stroke-linecap="round" />
                        <path d="M5 7H2l1 5a3 3 0 003 2M19 7h3l-1 5a3 3 0 01-3 2" stroke="#ffd700" stroke-width="1.8" stroke-linecap="round" />
                    </svg>
                    {{ currentTypeLabel }}
                </div>
                <div v-if="currentLeaderboard.length === 0" class="leaderboard-empty">
                    Nog geen scores
                </div>
                <div v-else>
                    <div v-for="entry in currentLeaderboard"
                         :key="entry.rank"
                         class="leaderboard-row"
                         :class="rankRowClass(entry.rank)">
                        <span class="leaderboard-rank" :class="rankClass(entry.rank)">
                            #{{ entry.rank }}
                        </span>
                        <span class="leaderboard-name">{{ entry.playerName }}</span>
                        <span class="leaderboard-time">{{ entry.duration }}</span>
                    </div>
                </div>
            </div>

            <div class="arcade-prompt arcade-blink mt-3">DRUK OP SPATIE OM TE STARTEN</div>
        </template>

        <!-- PHASE 2: Main Menu -->
        <template v-if="phase === 'menu'">
            <h1 class="arcade-title arcade-title--small">KIES JE UITDAGING</h1>

            <div class="menu-list mt-2">
                <div v-for="(item, i) in menuItems" :key="item.id">
                    <button class="arcade-btn mt-1"
                            :class="[item.btnClass, { selected: selectedIndex === i }]"
                            @click="activateItem(i)"
                            @mouseenter="onHover(i)">
                        <svg class="btn-icon" viewBox="0 0 24 24" fill="none" v-html="item.iconSvg"></svg>
                        {{ item.label }}
                    </button>
                </div>
            </div>

            <div class="arcade-subtitle mt-2" style="font-size:.4rem;">
                &#8593;&#8595; NAVIGEREN &bull; SPATIE/ENTER SELECTEREN
            </div>
        </template>
    </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue';
import { useIdleTimeout } from '../composables/useIdleTimeout.js';
import { useKeyController } from '../composables/useKeyController.js';

// --- SVG icon paths (inline, no emoji dependency) ---
const ICONS = {
    beer:  '<path d="M5 3h11l-1 13a4 4 0 01-4 4H9a4 4 0 01-4-4L5 3z" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/><path d="M5 7h13v3a4 4 0 01-3 3.87" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>',
    trophy:'<path d="M8 21h8M12 17v4M5 3h14l-1 8a6 6 0 01-12 0L5 3z" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/><path d="M5 7H2l1 5a3 3 0 003 2M19 7h3l-1 5a3 3 0 01-3 2" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>',
    chart: '<path d="M3 3v18h18" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/><path d="M7 16l4-5 4 3 4-6" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/>',
    gear:  '<circle cx="12" cy="12" r="3" stroke="currentColor" stroke-width="1.8"/><path d="M12 1v3M12 20v3M4.22 4.22l2.12 2.12M17.66 17.66l2.12 2.12M1 12h3M20 12h3M4.22 19.78l2.12-2.12M17.66 6.34l2.12-2.12" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>',
};

const props = defineProps({
    chugTypes: { type: Array, default: () => [] },
    leaderboards: { type: Object, default: () => ({}) },
});

// --- Secret codes ---
const SECRET_SEQ684 = ['Digit6', 'Digit8', 'Digit4'];
const secretBuffer684 = [];

const SECRET_CONFIG = ['KeyC', 'KeyO', 'KeyN', 'KeyF', 'KeyI', 'KeyG'];
const secretBufferConfig = [];
let configTimeout = null;

// --- State ---
const phase = ref('attract');
const currentTypeIndex = ref(0);
const currentLeaderboard = ref([]);
const selectedIndex = ref(0);

// --- Attract mode ---
let attractInterval = null;

const currentTypeLabel = computed(() => {
    if (props.chugTypes.length === 0) return '';
    return props.chugTypes[currentTypeIndex.value]?.label || '';
});

function rankRowClass(rank) {
    if (rank === 1) return 'leaderboard-row--gold';
    if (rank === 2) return 'leaderboard-row--silver';
    if (rank === 3) return 'leaderboard-row--bronze';
    return '';
}

function rankClass(rank) {
    if (rank === 1) return 'leaderboard-rank--gold';
    if (rank === 2) return 'leaderboard-rank--silver';
    if (rank === 3) return 'leaderboard-rank--bronze';
    return '';
}

function loadLeaderboard(index) {
    const type = props.chugTypes[index];
    if (!type) return;
    const data = props.leaderboards[type.slug];
    currentLeaderboard.value = Array.isArray(data) ? data : [];
}

function startAttract() {
    phase.value = 'attract';
    idleTimeout.stop();
    currentTypeIndex.value = 0;
    loadLeaderboard(0);
    attractInterval = setInterval(() => {
        if (props.chugTypes.length === 0) return;
        currentTypeIndex.value = (currentTypeIndex.value + 1) % props.chugTypes.length;
        loadLeaderboard(currentTypeIndex.value);
    }, 10000);
}

function stopAttract() {
    if (attractInterval) {
        clearInterval(attractInterval);
        attractInterval = null;
    }
}

// --- Menu ---
const menuItems = computed(() => {
    const items = props.chugTypes.map(ct => ({
        id: ct.slug,
        label: ct.label,
        iconSvg: ICONS.beer,
        btnClass: 'arcade-btn--primary',
        action: () => { window.location.href = `/play?type=${ct.slug}`; },
    }));
    items.push({
        id: 'leaderboards',
        label: 'Leaderboards',
        iconSvg: ICONS.trophy,
        btnClass: '',
        action: () => { window.location.href = '/leaderboards'; },
    });
    items.push({
        id: 'personal',
        label: 'Profiel',
        iconSvg: ICONS.chart,
        btnClass: '',
        action: () => { window.location.href = '/personal'; },
    });
    items.push({
        id: 'settings',
        label: 'Instellingen',
        iconSvg: ICONS.gear,
        btnClass: '',
        action: () => { window.location.href = '/settings'; },
    });
    return items;
});

function showMenu() {
    stopAttract();
    phase.value = 'menu';
    selectedIndex.value = 0;
    idleTimeout.start();
}

function activateItem(i) {
    selectedIndex.value = i;
    const item = menuItems.value[i];
    if (item?.action) item.action();
}

function onHover(i) {
    selectedIndex.value = i;
    idleTimeout.reset();
}

// --- Always single column ---
function getCols() { return 1; }

// --- Idle timeout (60s) ---
const idleTimeout = useIdleTimeout(60000, () => { startAttract(); });

function checkSecrets(code) {
    // 684 → hidden menu
    secretBuffer684.push(code);
    if (secretBuffer684.length > SECRET_SEQ684.length) secretBuffer684.shift();
    if (secretBuffer684.length === SECRET_SEQ684.length && secretBuffer684.every((k, i) => k === SECRET_SEQ684[i])) {
        window.location.href = '/hidden';
        return;
    }

    // config → calibration wizard
    clearTimeout(configTimeout);
    secretBufferConfig.push(code);
    if (secretBufferConfig.length > SECRET_CONFIG.length) secretBufferConfig.shift();
    if (secretBufferConfig.length === SECRET_CONFIG.length && secretBufferConfig.every((k, i) => k === SECRET_CONFIG[i])) {
        window.location.href = '/config';
        return;
    }
    configTimeout = setTimeout(() => { secretBufferConfig.length = 0; }, 10000);
}

// --- Keyboard ---
useKeyController({
    feedSecrets: (code) => checkSecrets(code),
    onActivate: () => {
        if (phase.value === 'attract') { showMenu(); return; }
        if (phase.value === 'menu') { activateItem(selectedIndex.value); idleTimeout.reset(); }
    },
    onEscape: () => {
        if (phase.value === 'menu') startAttract();
    },
    onDown: () => {
        if (phase.value !== 'menu') return;
        selectedIndex.value = (selectedIndex.value + 1) % menuItems.value.length;
        idleTimeout.reset();
    },
    onUp: () => {
        if (phase.value !== 'menu') return;
        selectedIndex.value = (selectedIndex.value - 1 + menuItems.value.length) % menuItems.value.length;
        idleTimeout.reset();
    },
    onRight: () => {
        if (phase.value !== 'menu') return;
        selectedIndex.value = (selectedIndex.value + 1) % menuItems.value.length;
        idleTimeout.reset();
    },
    onLeft: () => {
        if (phase.value !== 'menu') return;
        selectedIndex.value = (selectedIndex.value - 1 + menuItems.value.length) % menuItems.value.length;
        idleTimeout.reset();
    },
});

onMounted(() => { startAttract(); });
onUnmounted(() => { stopAttract(); idleTimeout.stop(); });
</script>

<style scoped>
.menu-list {
    list-style: none;
    padding: 0;
    margin: 0;
    display: flex;
    flex-direction: column;
    gap: 0.35rem;
    width: 100%;
    max-width: 340px;
}

.menu-list .arcade-btn {
    width: 100%;
}
</style>
