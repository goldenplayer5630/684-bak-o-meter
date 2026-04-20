<template>
    <div class="arcade-page">
        <h1 class="arcade-title arcade-title--small">PROFIEL</h1>

        <!-- Phase 1: NFC scan to identify player -->
        <template v-if="phase === 'scan'">
            <NfcScanGate @scanned="onScanned" @back="goBack" />
        </template>

        <!-- Phase 1b: Unknown tag — create new user -->
        <template v-if="phase === 'create-user'">
            <CreateUserFromNfc :tagUid="pendingTagUid"
                               @created="onUserCreated"
                               @cancel="phase = 'scan'" />
        </template>

        <!-- Phase 2: Profile view with tabs -->
        <template v-if="phase === 'profile'">
            <div class="ps-player-name mt-1">{{ playerName }}</div>

            <!-- Chug type tabs (same structure as LeaderboardsPage) -->
            <div class="leaderboard-tabs mt-1">
                <button v-for="(ct, i) in chugTypes" :key="ct.slug"
                        class="leaderboard-tab"
                        :class="{ selected: focus === 'tab' && tabIdx === i }"
                        @click="pickTab(i)"
                        @mouseenter="pickTab(i)">
                    {{ ct.label }}
                </button>
            </div>

            <!-- Stats panel -->
            <div class="leaderboard-panel">
                <div class="leaderboard-title">
                    <svg class="btn-icon" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <path d="M3 3v18h18" stroke="#ffd700" stroke-width="1.8" stroke-linecap="round"/>
                        <path d="M7 16l4-5 4 3 4-6" stroke="#ffd700" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/>
                    </svg>
                    {{ currentLabel }}
                </div>

                <!-- Stats rows (leaderboard style) -->
                <div v-if="currentStat" class="leaderboard-scroll">
                    <div class="leaderboard-row" :class="rowClass(currentStat.rank)">
                        <span class="leaderboard-rank" :class="rankClass(currentStat.rank)">#</span>
                        <span class="leaderboard-name">POSITIE</span>
                        <span class="leaderboard-time ps-dim"> >#{{ currentStat.rank }} / {{ currentStat.totalPlayers }}</span>
                    </div>
                    <div class="leaderboard-row">
                        <span class="leaderboard-rank">⏱</span>
                        <span class="leaderboard-name">BESTE TIJD</span>
                        <span class="leaderboard-time">{{ currentStat.best }}</span>
                    </div>
                    <div class="leaderboard-row">
                        <span class="leaderboard-rank">🎯</span>
                        <span class="leaderboard-name">POGINGEN</span>
                        <span class="leaderboard-time">{{ currentStat.attemptCount }}</span>
                    </div>
                </div>
                <div v-else class="leaderboard-empty">
                    Nog geen scores voor {{ currentLabel }}
                </div>

                <!-- Line graph canvas -->
                <div class="ps-graph-container mt-1">
                    <div v-if="graphLoading" class="leaderboard-empty">Laden...</div>
                    <div v-else-if="graphData.length === 0" class="leaderboard-empty">
                        Geen data om te tonen
                    </div>
                    <canvas v-else ref="canvasRef" class="ps-graph-canvas"
                            width="360" height="160"></canvas>
                </div>
            </div>

            <!-- NFC tag management -->
            <div class="ps-nfc-section mt-1" @mouseenter="focus = 'nfc'">
                <ManageNfcTags ref="manageNfcRef"
                               :playerId="playerId"
                               :keyboardSelected="focus === 'nfc'" />
            </div>

            <!-- Back -->
            <button class="arcade-btn arcade-btn--back mt-1"
                    :class="{ selected: focus === 'back' }"
                    @click="goBack()"
                    @mouseenter="focus = 'back'">
                &lsaquo; TERUG (DRUK ESC)
            </button>

            <!-- Key hint -->
            <div class="arcade-subtitle" style="font-size:.4rem;">
                &#8592;&#8594; TYPE WISSELEN &bull; &#8593;&#8595; NAVIGEREN &bull; ENTER SELECTEREN &bull; ESC TERUG
            </div>
        </template>
    </div>
</template>

<script setup>
import { ref, computed, watch, nextTick } from 'vue';
import NfcScanGate from './NfcScanGate.vue';
import CreateUserFromNfc from './CreateUserFromNfc.vue';
import ManageNfcTags from './ManageNfcTags.vue';
import { useKeyController } from '../composables/useKeyController.js';
import { useBgMusic } from '../composables/useBgMusic.js';

useBgMusic('/music/trekeenbak.mp3');

// --- Phase control ---
const phase = ref('scan');
const pendingTagUid = ref('');

// --- Player ---
const playerId = ref(0);
const playerName = ref('');
const stats = ref([]);

// --- Tab state ---
const chugTypes = ref([]);
const tabIdx = ref(0);
const focus = ref('tab');
const graphLoading = ref(false);
const graphData = ref([]);
const canvasRef = ref(null);
const manageNfcRef = ref(null);

const currentLabel = computed(() => chugTypes.value[tabIdx.value]?.label ?? '');
const currentStat = computed(() => {
    const slug = chugTypes.value[tabIdx.value]?.slug;
    return stats.value.find(s => s.slug === slug) || null;
});

// --- NFC handlers ---
async function onScanned(uid) {
    try {
        const res = await fetch('/api/nfc/resolve', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ uid }),
        });
        const data = await res.json();
        if (data.known) {
            await loadProfile(data.playerId, data.playerName);
        } else {
            pendingTagUid.value = uid;
            phase.value = 'create-user';
        }
    } catch {
        // Retry handled by scan gate
    }
}

function onUserCreated({ playerId: pid, playerName: pname }) {
    loadProfile(pid, pname);
}

async function loadProfile(pid, pname) {
    playerId.value = pid;
    playerName.value = pname;

    // Fetch chug types
    try {
        const r = await fetch('/api/leaderboards/types');
        const types = await r.json();
        chugTypes.value = Array.isArray(types) ? types : [];
    } catch {
        chugTypes.value = [];
    }

    // Fetch personal stats
    try {
        const r = await fetch(`/api/personal?playerId=${pid}`);
        if (r.ok) {
            const d = await r.json();
            stats.value = d.stats ?? [];
        }
    } catch {
        stats.value = [];
    }

    phase.value = 'profile';
    focus.value = 'tab';
    tabIdx.value = 0;
    if (chugTypes.value.length > 0) {
        await nextTick();
        loadGraph(0);
    }
}

// --- Tab switching ---
function pickTab(i) {
    tabIdx.value = i;
    focus.value = 'tab';
    loadGraph(i);
}

function moveFocusDown() {
    if (phase.value !== 'profile') return;
    if (focus.value === 'tab') focus.value = 'nfc';
    else if (focus.value === 'nfc') focus.value = 'back';
}

function moveFocusUp() {
    if (phase.value !== 'profile') return;
    if (focus.value === 'back') focus.value = 'nfc';
    else if (focus.value === 'nfc') focus.value = 'tab';
}

function activateFocus() {
    if (phase.value !== 'profile') return;

    if (focus.value === 'back') {
        goBack();
        return;
    }

    if (focus.value === 'nfc') {
        manageNfcRef.value?.activatePrimaryAction?.();
    }
}

async function loadGraph(idx) {
    const slug = chugTypes.value[idx]?.slug;
    if (!slug) { graphData.value = []; return; }

    graphLoading.value = true;
    try {
        const r = await fetch(
            `/api/personal/history?playerId=${playerId.value}&type=${slug}&count=10`
        );
        if (r.ok) {
            graphData.value = await r.json();
        } else {
            graphData.value = [];
        }
    } catch {
        graphData.value = [];
    } finally {
        graphLoading.value = false;
        await nextTick();
        drawGraph();
    }
}

// --- Canvas line graph ---
function drawGraph() {
    const canvas = canvasRef.value;
    if (!canvas || graphData.length === 0) return;

    const data = graphData.value;
    if (data.length === 0) return;

    const ctx = canvas.getContext('2d');
    const W = canvas.width;
    const H = canvas.height;
    const pad = { top: 16, right: 12, bottom: 28, left: 50 };
    const gW = W - pad.left - pad.right;
    const gH = H - pad.top - pad.bottom;

    ctx.clearRect(0, 0, W, H);

    const durations = data.map(d => d.durationMs);
    const minD = Math.min(...durations);
    const maxD = Math.max(...durations);
    const range = maxD - minD || 1000; // avoid division by zero
    const margin = range * 0.1;
    const yMin = Math.max(0, minD - margin);
    const yMax = maxD + margin;

    // Map data to pixel coordinates
    const n = data.length;
    const points = data.map((d, i) => ({
        x: pad.left + (n > 1 ? (i / (n - 1)) * gW : gW / 2),
        y: pad.top + gH - ((d.durationMs - yMin) / (yMax - yMin)) * gH,
        label: d.duration,
        isHighScore: d.isHighScore,
        dateLabel: d.date,
    }));

    // Grid lines
    ctx.strokeStyle = 'rgba(255,255,255,0.08)';
    ctx.lineWidth = 1;
    const gridSteps = 4;
    for (let i = 0; i <= gridSteps; i++) {
        const y = pad.top + (i / gridSteps) * gH;
        ctx.beginPath();
        ctx.moveTo(pad.left, y);
        ctx.lineTo(W - pad.right, y);
        ctx.stroke();

        // Y-axis label (time in seconds)
        const val = yMax - (i / gridSteps) * (yMax - yMin);
        ctx.fillStyle = 'rgba(255,255,255,0.35)';
        ctx.font = '8px monospace';
        ctx.textAlign = 'right';
        ctx.fillText((val / 1000).toFixed(1) + 's', pad.left - 4, y + 3);
    }

    // Line
    ctx.strokeStyle = '#ffd700';
    ctx.lineWidth = 2;
    ctx.lineJoin = 'round';
    ctx.beginPath();
    points.forEach((p, i) => {
        if (i === 0) ctx.moveTo(p.x, p.y);
        else ctx.lineTo(p.x, p.y);
    });
    ctx.stroke();

    // Area fill
    ctx.fillStyle = 'rgba(255,215,0,0.08)';
    ctx.beginPath();
    ctx.moveTo(points[0].x, pad.top + gH);
    points.forEach(p => ctx.lineTo(p.x, p.y));
    ctx.lineTo(points[points.length - 1].x, pad.top + gH);
    ctx.closePath();
    ctx.fill();

    // Data points
    points.forEach(p => {
        ctx.fillStyle = p.isHighScore ? '#00e676' : '#ffd700';
        ctx.beginPath();
        ctx.arc(p.x, p.y, 3, 0, Math.PI * 2);
        ctx.fill();
    });

    // X-axis attempt numbers
    ctx.fillStyle = 'rgba(255,255,255,0.35)';
    ctx.font = '7px monospace';
    ctx.textAlign = 'center';
    points.forEach((p, i) => {
        ctx.fillText(`${i + 1}`, p.x, pad.top + gH + 14);
    });
}

// Redraw when graph data changes
watch(graphData, () => { nextTick(() => drawGraph()); });

function rankClass(rank) {
    if (rank === 1) return 'leaderboard-rank--gold';
    if (rank === 2) return 'leaderboard-rank--silver';
    if (rank === 3) return 'leaderboard-rank--bronze';
    return '';
}

function rowClass(rank) {
    if (rank === 1) return 'leaderboard-row--gold';
    if (rank === 2) return 'leaderboard-row--silver';
    if (rank === 3) return 'leaderboard-row--bronze';
    return '';
}

function goBack() { window.location.href = '/'; }

useKeyController({
    onEscape: () => goBack(),
    onRight: () => {
        if (phase.value !== 'profile' || chugTypes.value.length === 0) return;
        pickTab((tabIdx.value + 1) % chugTypes.value.length);
    },
    onLeft: () => {
        if (phase.value !== 'profile' || chugTypes.value.length === 0) return;
        const n = chugTypes.value.length;
        pickTab((tabIdx.value - 1 + n) % n);
    },
    onDown: () => moveFocusDown(),
    onUp: () => moveFocusUp(),
    onActivate: () => activateFocus(),
});
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

.ps-dim {
    color: var(--text-muted) !important;
    font-size: 0.5rem;
}

.ps-graph-container {
    width: 100%;
    display: flex;
    justify-content: center;
}

.ps-graph-canvas {
    max-width: 100%;
    height: auto;
}

.ps-nfc-section {
    width: 100%;
    max-width: 340px;
    margin-left: auto;
    margin-right: auto;
}
</style>
