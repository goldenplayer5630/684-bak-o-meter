<template>
    <div class="arcade-page">

        <!-- STEP 1: Mode Selection -->
        <template v-if="step === 'mode'">
            <h1 class="arcade-title arcade-title--small">KIES MODUS</h1>
            <div class="arcade-subtitle mt-2">{{ chugTypeLabel }}</div>

            <div class="arcade-menu arcade-menu--narrow mt-3">
                <button class="arcade-btn arcade-btn--primary"
                        :class="{ selected: modeIndex === 0 }"
                        @click="selectMode(0)"
                        @mouseenter="modeIndex = 0">
                    <!-- person icon -->
                    <svg class="btn-icon" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <circle cx="12" cy="7" r="4" stroke="currentColor" stroke-width="1.8"/>
                        <path d="M4 21c0-4 3.6-7 8-7s8 3 8 7" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>
                    </svg>
                    SINGLEPLAYER
                </button>
                <button class="arcade-btn arcade-btn--primary"
                        :class="{ selected: modeIndex === 1 }"
                        @click="selectMode(1)"
                        @mouseenter="modeIndex = 1">
                    <!-- two people icon -->
                    <svg class="btn-icon" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <circle cx="9" cy="7" r="3.5" stroke="currentColor" stroke-width="1.8"/>
                        <path d="M2 21c0-3.5 3.1-6 7-6s7 2.5 7 6" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>
                        <circle cx="17" cy="7" r="3" stroke="currentColor" stroke-width="1.6"/>
                        <path d="M20 21c0-2.5-1.3-4.6-3.5-5.5" stroke="currentColor" stroke-width="1.6" stroke-linecap="round"/>
                    </svg>
                    MULTIPLAYER
                </button>
                <button class="arcade-btn arcade-btn--back"
                        :class="{ selected: modeIndex === 2 }"
                        @click="goBack()"
                        @mouseenter="modeIndex = 2">
                    &lsaquo; TERUG (DRUK ESC)
                </button>
            </div>
        </template>

        <!-- STEP 2: NFC scan for player 1 -->
        <template v-if="step === 'nfc-scan'">
            <h1 class="arcade-title arcade-title--small">SPELER 1</h1>
            <NfcScanGate @scanned="onPlayer1Scanned" @back="step = 'mode'" />
        </template>

        <!-- STEP 2b: Unknown tag for player 1 — create new user -->
        <template v-if="step === 'create-user'">
            <CreateUserFromNfc :tagUid="pendingTagUid"
                               @created="onUserCreated"
                               @cancel="step = 'nfc-scan'" />
        </template>

        <!-- STEP 3: NFC scan for player 2 (multiplayer only) -->
        <template v-if="step === 'nfc-scan-p2'">
            <h1 class="arcade-title arcade-title--small">SPELER 2</h1>
            <NfcScanGate @scanned="onPlayer2Scanned" @back="step = 'nfc-scan'" />
        </template>

        <!-- STEP 3b: Unknown tag for player 2 — create new user -->
        <template v-if="step === 'create-user-p2'">
            <CreateUserFromNfc :tagUid="pendingTagUid"
                               @created="onPlayer2Created"
                               @cancel="step = 'nfc-scan-p2'" />
        </template>

        <!-- STEP 3: Chug Timer -->
        <template v-if="step === 'chug'">
            <h1 class="arcade-title arcade-title--small">{{ chugTypeLabel }}</h1>

            <!-- Singleplayer -->
            <template v-if="!isMultiplayer">
                <div class="result-player mt-2">{{ playerName1 }}</div>
                <TimerDisplay :elapsed="chugElapsed1" :state="timer1State" />
                <div v-if="scale1State.state !== 'Completed'" class="arcade-prompt mt-3" :class="{ 'arcade-blink': scale1State.state !== 'Running' && scale1State.state !== 'Validating' }">
                    <span v-if="scale1State.state === 'WaitingForFull'">PLAATS EEN VOLLE BAK</span>
                    <span v-if="scale1State.state === 'Ready'">TREK EEN BAK!</span>
                    <span v-if="scale1State.state === 'Running'">ZUIPEN MET JE DONDER!</span>
                    <span v-if="scale1State.state === 'Validating'">VALIDEREN... (LAAT GLAS LOS!!!)</span>
                </div>
            </template>

            <!-- Multiplayer -->
            <template v-if="isMultiplayer">
                <div class="mp-timers">
                    <div class="mp-timer-col">
                        <div class="result-player">{{ playerName1 }}</div>
                        <TimerDisplay :elapsed="chugElapsed1" :state="timer1State" />
                    </div>
                    <div class="mp-vs">VS</div>
                    <div class="mp-timer-col">
                        <div class="result-player">{{ playerName2 }}</div>
                        <TimerDisplay :elapsed="chugElapsed2" :state="timer2State" />
                    </div>
                </div>
                <div v-if="!bothCompleted" class="arcade-prompt mt-3" :class="{ 'arcade-blink': !anyRunning }">
                    <span v-if="bothWaiting">PLAATS JULLIE VOLLE BAKKEN</span>
                    <span v-if="bothReady">TREK EEN BAK!</span>
                    <span v-if="anyRunning">ZUIPEN MET JE DONDER!</span>
                    <span v-if="anyValidating">VALIDEREN...</span>
                </div>
            </template>
        </template>

        <!-- STEP 4: Result -->
        <template v-if="step === 'result'">
            <div class="result-screen">

                <!-- Singleplayer result -->
                <template v-if="!isMultiplayer">
                    <div class="result-title">RESULTAAT</div>
                    <div class="result-player">{{ playerName1 }}</div>
                    <div v-if="isNewBest1" class="result-new-best arcade-blink">★ NIEUW RECORD!</div>
                    <div v-else class="result-not-best">NIET VERBETERD</div>
                    <div v-if="rank1" class="result-rank">#{{ rank1 }}</div>
                    <div class="result-time">{{ formatTime(elapsed1) }}</div>
                </template>

                <!-- Multiplayer result -->
                <template v-if="isMultiplayer">
                    <div class="result-title">WINNAAR</div>
                    <div class="result-winner">{{ winnerName }}</div>
                    <div v-if="winnerNewBest" class="result-new-best arcade-blink">★ NIEUW RECORD!</div>
                    <div v-if="winnerRank" class="result-rank">#{{ winnerRank }}</div>
                    <div class="result-time">{{ formatTime(winnerTime) }}</div>
                    <div class="result-vs">VS</div>
                    <div class="result-loser">
                        {{ loserName }}
                        <span v-if="!loserNewBest" class="result-not-best-inline"> — niet verbeterd</span>
                        <span v-if="loserRank"> — #{{ loserRank }}</span>
                        — {{ formatTime(loserTime) }}
                    </div>
                </template>

                <div class="result-countdown">TERUG NAAR MENU IN {{ resultCountdown }}...</div>
            </div>
        </template>

        <!-- STEP: Invalid (full glass placed back) -->
        <template v-if="step === 'invalid'">
            <div class="result-screen">
                <div class="result-title arcade-blink" style="color: #ff4081;">ONGELDIG!</div>
                <div class="arcade-prompt mt-3">LAFFE BORRELAAR!<br>JE MAG GEEN VOL GLAS TERUG ZETTEN</div>
                <div class="result-countdown mt-3">TERUG NAAR MENU IN {{ resultCountdown }}...</div>
            </div>
        </template>
    </div>
</template>

<script setup>
import { ref, computed, onUnmounted } from 'vue';
import TimerDisplay from './TimerDisplay.vue';
import NfcScanGate from './NfcScanGate.vue';
import CreateUserFromNfc from './CreateUserFromNfc.vue';
import { useKeyController } from '../composables/useKeyController.js';
import { useChugHub } from '../composables/useChugHub.js';
import { useBgMusic } from '../composables/useBgMusic.js';

useBgMusic('/music/battle.mp3');

const props = defineProps({
    chugTypeSlug: { type: String, default: 'Bak' },
    chugTypeLabel: { type: String, default: 'Bak' },
});

const chugHub = useChugHub();

// --- Flow ---
const step = ref('mode');
const modeIndex = ref(0);
const isMultiplayer = ref(false);

const pendingTagUid = ref('');

// --- Player data (resolved via NFC) ---
const playerName1 = ref('');
const playerName2 = ref('');
let player1Id = null;
let player2Id = null;

// --- Scale state (driven by SignalR) ---
const scale1State = computed(() => chugHub.scale1.value);
const scale2State = computed(() => chugHub.scale2.value);
const chugElapsed1 = computed(() => chugHub.elapsed1.value);
const chugElapsed2 = computed(() => chugHub.elapsed2.value);

// Map scale states to TimerDisplay state prop
const timer1State = computed(() => {
    const s = scale1State.value.state;
    if (s === 'Running') return 'running';
    if (s === 'Completed' || s === 'Validating') return 'stopped';
    return 'idle';
});
const timer2State = computed(() => {
    const s = scale2State.value.state;
    if (s === 'Running') return 'running';
    if (s === 'Completed' || s === 'Validating') return 'stopped';
    return 'idle';
});

// Multiplayer prompt helpers
const bothWaiting = computed(() => scale1State.value.state === 'WaitingForFull' || scale2State.value.state === 'WaitingForFull');
const bothReady = computed(() => scale1State.value.state === 'Ready' && scale2State.value.state === 'Ready');
const anyRunning = computed(() => scale1State.value.state === 'Running' || scale2State.value.state === 'Running');
const anyValidating = computed(() => (scale1State.value.state === 'Validating' || scale2State.value.state === 'Validating') && !anyRunning.value);
const bothCompleted = computed(() => scale1State.value.state === 'Completed' && scale2State.value.state === 'Completed');

// Result-screen elapsed values (frozen on completion)
const elapsed1 = ref(0);
const elapsed2 = ref(0);

// --- Result ---
const resultCountdown = ref(10);
let resultInterval = null;

const rank1      = ref(null);
const rank2      = ref(null);
const isNewBest1 = ref(false);
const isNewBest2 = ref(false);

const winnerName    = computed(() => elapsed1.value <= elapsed2.value ? playerName1.value : playerName2.value);
const loserName     = computed(() => elapsed1.value <= elapsed2.value ? playerName2.value : playerName1.value);
const winnerTime    = computed(() => Math.min(elapsed1.value, elapsed2.value));
const loserTime     = computed(() => Math.max(elapsed1.value, elapsed2.value));
const winnerRank    = computed(() => elapsed1.value <= elapsed2.value ? rank1.value : rank2.value);
const loserRank     = computed(() => elapsed1.value <= elapsed2.value ? rank2.value : rank1.value);
const winnerNewBest = computed(() => elapsed1.value <= elapsed2.value ? isNewBest1.value : isNewBest2.value);
const loserNewBest  = computed(() => elapsed1.value <= elapsed2.value ? isNewBest2.value : isNewBest1.value);

function formatTime(ms) {
    const sec  = Math.floor(ms / 1000);
    const frac = String(ms % 1000).padStart(3, '0');
    return `${sec}.${frac}`;
}

// --- NFC scan handlers ---

// Player 1 scanned their tag
async function onPlayer1Scanned(uid) {
    const result = await resolveNfcTag(uid);
    if (result.known) {
        player1Id = result.playerId;
        playerName1.value = result.playerName;
        if (isMultiplayer.value) {
            step.value = 'nfc-scan-p2';
        } else {
            startChug();
        }
    } else {
        // Unknown tag — redirect to user creation flow
        pendingTagUid.value = uid;
        step.value = 'create-user';
    }
}

// New user created from unknown tag (player 1)
function onUserCreated({ playerId, playerName }) {
    player1Id = playerId;
    playerName1.value = playerName;
    if (isMultiplayer.value) {
        step.value = 'nfc-scan-p2';
    } else {
        startChug();
    }
}

// Player 2 scanned their tag (multiplayer)
async function onPlayer2Scanned(uid) {
    const result = await resolveNfcTag(uid);
    if (result.known) {
        if (result.playerId === player1Id) {
            // Same player — don't allow playing against yourself
            return;
        }
        player2Id = result.playerId;
        playerName2.value = result.playerName;
        startChug();
    } else {
        pendingTagUid.value = uid;
        step.value = 'create-user-p2';
    }
}

// New user created from unknown tag (player 2)
function onPlayer2Created({ playerId, playerName }) {
    player2Id = playerId;
    playerName2.value = playerName;
    startChug();
}

async function resolveNfcTag(uid) {
    try {
        const res = await fetch('/api/nfc/resolve', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ uid }),
        });
        return await res.json();
    } catch {
        return { known: false };
    }
}

// --- Mode selection ---
function selectMode(i) {
    modeIndex.value = i;
    if (i === 0) {
        isMultiplayer.value = false;
        step.value = 'nfc-scan';
    } else if (i === 1) {
        isMultiplayer.value = true;
        step.value = 'nfc-scan';
    }
}

let completedScales = 0;

async function startChug() {
    chugHub.reset();
    elapsed1.value = 0;
    elapsed2.value = 0;
    completedScales = 0;
    step.value = 'chug';

    await chugHub.connect(onChugCompleted, onChugInvalid);

    // Start session(s) on the backend
    await fetch('/api/chug/start-session', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ playerId: player1Id, chugType: props.chugTypeSlug, scaleNumber: 1 }),
    });

    if (isMultiplayer.value) {
        await fetch('/api/chug/start-session', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ playerId: player2Id, chugType: props.chugTypeSlug, scaleNumber: 2 }),
        });
    }
}

function onChugCompleted(data) {
    if (data.scaleNumber === 1) elapsed1.value = data.durationMs;
    if (data.scaleNumber === 2) elapsed2.value = data.durationMs;

    if (!isMultiplayer.value) {
        saveAndShowResult();
    } else {
        completedScales++;
        if (completedScales >= 2) saveAndShowResult();
    }
}

function onChugInvalid(data) {
    // Don't save — just show the shame message and go back to menu
    step.value = 'invalid';
    resultCountdown.value = 5;
    resultInterval = setInterval(() => {
        resultCountdown.value--;
        if (resultCountdown.value <= 0) { clearInterval(resultInterval); window.location.href = '/'; }
    }, 1000);
}

function goBack() { window.location.href = '/'; }

async function saveAndShowResult() {
    try {
        const r1 = await fetch('/api/play/save-attempt', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ playerId: player1Id, chugType: props.chugTypeSlug, durationMs: elapsed1.value }),
        });
        const d1 = await r1.json();
        rank1.value      = d1.rank ?? null;
        isNewBest1.value = d1.isNewBest ?? false;

        if (isMultiplayer.value && player2Id) {
            const r2 = await fetch('/api/play/save-attempt', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ playerId: player2Id, chugType: props.chugTypeSlug, durationMs: elapsed2.value }),
            });
            const d2 = await r2.json();
            rank2.value      = d2.rank ?? null;
            isNewBest2.value = d2.isNewBest ?? false;
        }
    } catch (err) { console.error('Save failed:', err); }

    spawnConfetti();
    step.value = 'result';
    resultCountdown.value = 5;
    resultInterval = setInterval(() => {
        resultCountdown.value--;
        if (resultCountdown.value <= 0) { clearInterval(resultInterval); window.location.href = '/'; }
    }, 1000);
}

function spawnConfetti() {
    const colors = ['#ffd700', '#00e676', '#00e5ff', '#ff4081', '#ff9100', '#b388ff'];
    for (let i = 0; i < 60; i++) {
        const el = document.createElement('div');
        el.className = 'confetti';
        el.style.left = Math.random() * 100 + 'vw';
        el.style.background = colors[Math.floor(Math.random() * colors.length)];
        el.style.width = (6 + Math.random() * 8) + 'px';
        el.style.height = (6 + Math.random() * 8) + 'px';
        el.style.borderRadius = Math.random() > 0.5 ? '50%' : '2px';
        el.style.animationDuration = (2 + Math.random() * 3) + 's';
        el.style.animationDelay = (Math.random() * 2) + 's';
        document.body.appendChild(el);
        setTimeout(() => el.remove(), 6000);
    }
}

// --- Keyboard ---
useKeyController({
    onEscape: () => { if (step.value === 'mode') goBack(); },
    onUp: () => { if (step.value === 'mode') modeIndex.value = Math.max(0, modeIndex.value - 1); },
    onDown: () => { if (step.value === 'mode') modeIndex.value = Math.min(2, modeIndex.value + 1); },
    onActivate: () => {
        if (step.value === 'mode') {
            if (modeIndex.value === 2) goBack();
            else selectMode(modeIndex.value);
        }
    },
});

onUnmounted(() => {
    chugHub.disconnect();
    if (resultInterval) clearInterval(resultInterval);
});
</script>

<style scoped>
.mp-timers {
    display: flex;
    gap: 1.2rem;
    align-items: center;
    margin-top: 0.6rem;
    width: 100%;
    max-width: 420px;
}
.mp-timer-col {
    text-align: center;
    flex: 1;
    min-width: 0;
}
.mp-vs {
    font-family: var(--font-arcade);
    font-size: 0.9rem;
    color: var(--text-muted);
    flex-shrink: 0;
}
</style>
