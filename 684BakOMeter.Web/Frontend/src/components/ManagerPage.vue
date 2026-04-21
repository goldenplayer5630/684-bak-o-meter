<template>
    <div class="arcade-page">
        <h1 class="arcade-title arcade-title--small">MANAGER</h1>

        <div class="leaderboard-panel manager-panel mt-1">
            <div class="leaderboard-title">SCORE BEHEER</div>

            <button class="arcade-btn mt-1"
                    :class="{ selected: !confirmState && focus === 'clear-all' }"
                    @click="startConfirmAll"
                    @mouseenter="focus = 'clear-all'">
                VERWIJDER ALLE SCORES
            </button>

            <div class="manager-users mt-1"
                 :class="{ selected: !confirmState && focus === 'users' }"
                 @mouseenter="focus = 'users'">
                <div class="manager-users-title">GEBRUIKERS MET SCORES</div>

                <!-- Search bar — visible when focused on users and there are results -->
                <div v-if="!loadingUsers && users.length > 0" class="manager-search">
                    <span class="manager-search-prefix">ZOEK&rsaquo;</span>
                    <span class="manager-search-query">{{ searchQuery }}</span><span class="manager-search-cursor" :class="{ blink: focus === 'users' }">&#9646;</span>
                </div>

                <div v-if="loadingUsers" class="leaderboard-empty">Laden...</div>
                <div v-else-if="users.length === 0" class="leaderboard-empty">
                    Geen gebruikers met scores gevonden.
                </div>
                <div v-else-if="filteredUsers.length === 0" class="leaderboard-empty">
                    Geen resultaten voor "{{ searchQuery }}".
                </div>
                <div v-else class="leaderboard-scroll">
                    <div v-for="(u, i) in visibleUsers"
                         :key="u.playerId"
                         class="leaderboard-row"
                         :class="{ selected: !confirmState && focus === 'users' && selectedFilteredIdx === i }"
                         @click="pickUser(i)">
                        <span class="leaderboard-rank">#{{ i + 1 }}</span>
                        <span class="leaderboard-name">{{ u.playerName }}</span>
                        <span class="leaderboard-time">{{ u.attemptCount }}</span>
                    </div>
                    <div v-if="filteredUsers.length > 10" class="manager-more-results">
                        +{{ filteredUsers.length - 10 }} meer — typ verder om te filteren
                    </div>
                </div>
            </div>

            <button class="arcade-btn mt-1"
                    :class="{ selected: !confirmState && focus === 'clear-user' }"
                    @click="startConfirmUser"
                    @mouseenter="focus = 'clear-user'">
                VERWIJDER SCORES VAN GEKOZEN GEBRUIKER
            </button>

            <div class="arcade-subtitle mt-1 manager-message">{{ statusMessage }}</div>

            <div v-if="confirmState" class="manager-confirm mt-1">
                <div class="arcade-subtitle">{{ confirmMessage }}</div>
                <div class="manager-confirm-actions mt-1">
                    <button class="arcade-btn"
                            :class="{ selected: confirmChoice === 0 }"
                            @click="confirmDelete">
                        JA, VERWIJDER
                    </button>
                    <button class="arcade-btn arcade-btn--back"
                            :class="{ selected: confirmChoice === 1 }"
                            @click="cancelConfirm">
                        ANNULEREN
                    </button>
                </div>
            </div>
        </div>

        <button class="arcade-btn arcade-btn--back mt-1"
                :class="{ selected: !confirmState && focus === 'back' }"
                @click="goBack"
                @mouseenter="focus = 'back'">
            &lsaquo; TERUG (DRUK ESC)
        </button>

        <div class="arcade-subtitle" style="font-size:.4rem;">
            &#8592;&#8594;&#8593;&#8595; NAVIGEREN &bull; SPATIE/ENTER SELECTEREN &bull; ESC TERUG
        </div>
    </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue';
import { useKeyController } from '../composables/useKeyController.js';
import { useBgMusic } from '../composables/useBgMusic.js';

useBgMusic('/music/trekeenbak.mp3');

const users = ref([]);
const loadingUsers = ref(false);
const selectedFilteredIdx = ref(0);
const focus = ref('clear-all'); // clear-all | users | clear-user | back
const statusMessage = ref('');
const searchQuery = ref('');

const confirmState = ref(null); // 'all' | 'user' | null
const confirmChoice = ref(0);   // 0 = confirm, 1 = cancel

const controls = ['clear-all', 'users', 'clear-user', 'back'];

const filteredUsers = computed(() => {
    const q = searchQuery.value.trim().toLowerCase();
    if (!q) return users.value;
    return users.value.filter(u => u.playerName.toLowerCase().includes(q));
});

const visibleUsers = computed(() => {
    return filteredUsers.value.slice(0, 10);
});

// Reset selection when search results change
watch(filteredUsers, () => { selectedFilteredIdx.value = 0; });

// Clear search when focus leaves the users panel
watch(focus, (next, prev) => {
    if (prev === 'users' && next !== 'users') searchQuery.value = '';
});

const selectedUser = computed(() => visibleUsers.value[selectedFilteredIdx.value] ?? null);

const confirmMessage = computed(() => {
    if (confirmState.value === 'all') {
        return 'Weet je zeker dat je ALLE scores wilt verwijderen?';
    }
    if (confirmState.value === 'user') {
        if (!selectedUser.value) return 'Geen gebruiker geselecteerd.';
        return `Weet je zeker dat je alle scores van ${selectedUser.value.playerName} wilt verwijderen?`;
    }
    return '';
});

async function loadUsers() {
    loadingUsers.value = true;
    try {
        const r = await fetch('/api/manager/users-with-scores');
        const d = await r.json();
        users.value = Array.isArray(d) ? d : [];
    } catch {
        users.value = [];
        statusMessage.value = 'Kon gebruikerslijst niet laden.';
    } finally {
        loadingUsers.value = false;
    }
}

function moveFocus(step) {
    const idx = controls.indexOf(focus.value);
    focus.value = controls[(idx + step + controls.length) % controls.length];
}

function moveUser(step) {
    const len = visibleUsers.value.length;
    if (len === 0) return;
    selectedFilteredIdx.value = (selectedFilteredIdx.value + step + len) % len;
}

function pickUser(i) {
    selectedFilteredIdx.value = i;
    focus.value = 'users';
}

function startConfirmAll() {
    if (users.value.length === 0) {
        statusMessage.value = 'Er zijn geen scores om te verwijderen.';
        return;
    }
    confirmState.value = 'all';
    confirmChoice.value = 1;
}

function startConfirmUser() {
    if (!selectedUser.value) {
        statusMessage.value = 'Selecteer eerst een gebruiker met scores.';
        return;
    }
    confirmState.value = 'user';
    confirmChoice.value = 1;
}

function cancelConfirm() {
    confirmState.value = null;
}

async function confirmDelete() {
    if (confirmState.value === 'all') await clearAllScores();
    else if (confirmState.value === 'user') await clearSelectedUserScores();
    confirmState.value = null;
}

async function clearAllScores() {
    try {
        const r = await fetch('/api/manager/scores/all', { method: 'DELETE' });
        const d = await r.json();
        statusMessage.value = d.message ?? 'Actie voltooid.';
    } catch {
        statusMessage.value = 'Kon scores niet verwijderen.';
    }
    await loadUsers();
}

async function clearSelectedUserScores() {
    if (!selectedUser.value) {
        statusMessage.value = 'Selecteer eerst een gebruiker met scores.';
        return;
    }
    try {
        const r = await fetch(`/api/manager/scores/player/${selectedUser.value.playerId}`, { method: 'DELETE' });
        const d = await r.json();
        statusMessage.value = d.message ?? d.error ?? 'Actie voltooid.';
    } catch {
        statusMessage.value = 'Kon scores van gebruiker niet verwijderen.';
    }
    await loadUsers();
}

function activateFocused() {
    if (confirmState.value) {
        if (confirmChoice.value === 0) confirmDelete();
        else cancelConfirm();
        return;
    }
    if (focus.value === 'clear-all') startConfirmAll();
    else if (focus.value === 'clear-user') startConfirmUser();
    else if (focus.value === 'back') goBack();
    else if (focus.value === 'users') startConfirmUser();
}

function goBack() {
    window.location.href = '/';
}

useKeyController({
    // Intercept printable keys + Backspace when the user panel is focused
    onKey: (e) => {
        if (focus.value !== 'users' || confirmState.value) return false;
        if (e.key === 'Backspace') {
            searchQuery.value = searchQuery.value.slice(0, -1);
            e.preventDefault();
            return true;
        }
        if (e.key.length === 1 && !e.ctrlKey && !e.altKey && !e.metaKey) {
            searchQuery.value += e.key;
            e.preventDefault();
            return true;
        }
        return false;
    },
    onEscape: () => {
        if (confirmState.value) { cancelConfirm(); return; }
        if (focus.value === 'users' && searchQuery.value) { searchQuery.value = ''; return; }
        goBack();
    },
    onLeft:  () => {
        if (confirmState.value) { confirmChoice.value = 0; return; }
        moveFocus(-1);
    },
    onRight: () => {
        if (confirmState.value) { confirmChoice.value = 1; return; }
        moveFocus(1);
    },
    onUp: () => {
        if (confirmState.value) { confirmChoice.value = 0; return; }
        if (focus.value === 'users') { moveUser(-1); return; }
        moveFocus(-1);
    },
    onDown: () => {
        if (confirmState.value) { confirmChoice.value = 1; return; }
        if (focus.value === 'users') { moveUser(1); return; }
        moveFocus(1);
    },
    onActivate: () => activateFocused(),
});

// Use onMounted pattern without the import since we call loadUsers directly
loadUsers().then(() => {
    if (users.value.length === 0) statusMessage.value = 'Geen scores gevonden om te beheren.';
});
</script>

<style scoped>
.manager-panel {
    width: 100%;
    max-width: 380px;
}

.manager-users {
    border: 1px solid rgba(255, 255, 255, 0.15);
    border-radius: 8px;
    padding: 0.45rem;
}

.manager-users .leaderboard-row.selected {
    border-color: var(--accent-yellow);
    background: rgba(255, 215, 0, 0.07);
    box-shadow: inset 0 0 0 1px rgba(255, 215, 0, 0.35);
}

.manager-users.selected {
    border-color: var(--accent-yellow);
    box-shadow: var(--glow-yellow);
}

.manager-users-title {
    font-family: var(--font-arcade);
    font-size: 0.5rem;
    color: var(--text-muted);
    margin-bottom: 0.3rem;
    letter-spacing: 1px;
}

.manager-search {
    display: flex;
    align-items: center;
    gap: 0.3rem;
    font-family: var(--font-arcade);
    font-size: 0.6rem;
    color: var(--accent-yellow);
    background: rgba(255, 215, 0, 0.05);
    border: 1px solid rgba(255, 215, 0, 0.2);
    border-radius: 4px;
    padding: 0.25rem 0.45rem;
    margin-bottom: 0.35rem;
    min-height: 1.4rem;
}

.manager-search-prefix {
    color: var(--text-muted);
    flex-shrink: 0;
}

.manager-search-query {
    flex: 1;
    color: var(--text-primary);
    letter-spacing: 1px;
}

.manager-search-cursor {
    color: var(--accent-yellow);
    opacity: 0;
}

.manager-search-cursor.blink {
    animation: cursor-blink 1s step-end infinite;
}

@keyframes cursor-blink {
    0%, 100% { opacity: 1; }
    50%       { opacity: 0; }
}

.manager-message {
    min-height: 1.2rem;
}

.manager-more-results {
    font-family: var(--font-arcade);
    font-size: 0.45rem;
    color: var(--text-muted);
    text-align: center;
    padding: 0.5rem;
    margin-top: 0.3rem;
    border-top: 1px solid rgba(255, 255, 255, 0.08);
}

.manager-confirm {
    border: 1px solid rgba(255, 215, 0, 0.35);
    border-radius: 8px;
    padding: 0.5rem;
}

.manager-confirm-actions {
    display: flex;
    gap: 0.45rem;
}

.manager-confirm-actions .arcade-btn {
    flex: 1;
}
</style>
