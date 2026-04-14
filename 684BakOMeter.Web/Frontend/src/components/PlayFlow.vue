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
                <TimerDisplay :elapsed="elapsed1" :state="timerState" />
                <div class="arcade-prompt mt-3" :class="{ 'arcade-blink': timerState !== 'running' }">
                    <span v-if="timerState === 'idle'">DRUK OP SPATIE OM TE STARTEN</span>
                    <span v-if="timerState === 'running'">DRUK OP SPATIE OM TE STOPPEN</span>
                    <span v-if="timerState === 'stopped'">OPSLAAN...</span>
                </div>
            </template>

            <!-- Multiplayer -->
            <template v-if="isMultiplayer">
                <div class="mp-timers">
                    <div class="mp-timer-col">
                        <div class="result-player">{{ playerName1 }}</div>
                        <TimerDisplay :elapsed="elapsed1" :state="mp1State" />
                    </div>
                    <div class="mp-vs">VS</div>
                    <div class="mp-timer-col">
                        <div class="result-player">{{ playerName2 }}</div>
                        <TimerDisplay :elapsed="elapsed2" :state="mp2State" />
                    </div>
                </div>
                <div class="arcade-prompt mt-3" :class="{ 'arcade-blink': !mpRunning }">
                    <span v-if="mpPhase === 'p1ready'">{{ playerName1.toUpperCase() }}: DRUK OP SPATIE</span>
                    <span v-if="mpPhase === 'p1running'">{{ playerName1.toUpperCase() }}: DRUK OP SPATIE OM TE STOPPEN</span>
                    <span v-if="mpPhase === 'p2ready'">{{ playerName2.toUpperCase() }}: DRUK OP SPATIE</span>
                    <span v-if="mpPhase === 'p2running'">{{ playerName2.toUpperCase() }}: DRUK OP SPATIE OM TE STOPPEN</span>
                    <span v-if="mpPhase === 'done'">OPSLAAN...</span>
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
    </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue';
import TimerDisplay from './TimerDisplay.vue';
import NfcScanGate from './NfcScanGate.vue';
import CreateUserFromNfc from './CreateUserFromNfc.vue';
import { useDiaboloMode } from '../composables/useDiaboloMode.js';

const props = defineProps({
    chugTypeSlug: { type: String, default: 'Bak' },
    chugTypeLabel: { type: String, default: 'Bak' },
});

const diabolo = useDiaboloMode();

// --- Flow ---
// Starts at mode selection; NFC scans happen after mode is chosen
const step = ref('mode');
const modeIndex = ref(0);
const isMultiplayer = ref(false);

// Holds the scanned tag UID while waiting for user creation
const pendingTagUid = ref('');

// --- Player data (resolved via NFC) ---
const playerName1 = ref('');
const playerName2 = ref('');
let player1Id = null;
let player2Id = null;

// --- Timer (single) ---
const timerState = ref('idle');
const elapsed1 = ref(0);
let startTime1 = 0;
let timerInterval1 = null;

// --- Timer (multi) ---
const mpPhase = ref('p1ready');
const mp1State = ref('idle');
const mp2State = ref('idle');
const elapsed2 = ref(0);
let startTime2 = 0;
let timerInterval2 = null;

const mpRunning = computed(() => mpPhase.value === 'p1running' || mpPhase.value === 'p2running');

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

function startChug() {
    step.value = 'chug';
    timerState.value = 'idle';
    mpPhase.value = 'p1ready';
    mp1State.value = 'idle';
    mp2State.value = 'idle';
    elapsed1.value = 0;
    elapsed2.value = 0;
}

function goBack() { window.location.href = '/'; }

// --- Chug logic ---
function handleChugSpace() {
    if (!isMultiplayer.value) {
        if (timerState.value === 'idle') {
            timerState.value = 'running';
            startTime1 = performance.now();
            timerInterval1 = setInterval(() => {
                elapsed1.value = Math.round(performance.now() - startTime1);
            }, 31);
        } else if (timerState.value === 'running') {
            timerState.value = 'stopped';
            elapsed1.value = Math.round(performance.now() - startTime1);
            clearInterval(timerInterval1);
            saveAndShowResult();
        }
    } else {
        if (mpPhase.value === 'p1ready') {
            mpPhase.value = 'p1running'; mp1State.value = 'running';
            startTime1 = performance.now();
            timerInterval1 = setInterval(() => { elapsed1.value = Math.round(performance.now() - startTime1); }, 31);
        } else if (mpPhase.value === 'p1running') {
            mpPhase.value = 'p2ready'; mp1State.value = 'stopped';
            elapsed1.value = Math.round(performance.now() - startTime1);
            clearInterval(timerInterval1);
        } else if (mpPhase.value === 'p2ready') {
            mpPhase.value = 'p2running'; mp2State.value = 'running';
            startTime2 = performance.now();
            timerInterval2 = setInterval(() => { elapsed2.value = Math.round(performance.now() - startTime2); }, 31);
        } else if (mpPhase.value === 'p2running') {
            mpPhase.value = 'done'; mp2State.value = 'stopped';
            elapsed2.value = Math.round(performance.now() - startTime2);
            clearInterval(timerInterval2);
            saveAndShowResult();
        }
    }
}

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
    resultCountdown.value = 10;
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
function onKeyDown(e) {
    diabolo.feedKey(e.code);

    if (step.value === 'mode') {
        if (e.code === 'ArrowUp') { e.preventDefault(); modeIndex.value = Math.max(0, modeIndex.value - 1); }
        else if (e.code === 'ArrowDown') { e.preventDefault(); modeIndex.value = Math.min(2, modeIndex.value + 1); }
        else if (e.code === 'Space' || e.code === 'Enter') {
            e.preventDefault();
            if (modeIndex.value === 2) goBack();
            else selectMode(modeIndex.value);
        } else if (e.code === 'Escape') { goBack(); }
    } else if (step.value === 'chug') {
        if (e.code === 'Space') { e.preventDefault(); handleChugSpace(); }
    }
}

onMounted(() => { document.addEventListener('keydown', onKeyDown); });
onUnmounted(() => {
    document.removeEventListener('keydown', onKeyDown);
    if (timerInterval1) clearInterval(timerInterval1);
    if (timerInterval2) clearInterval(timerInterval2);
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
