<template>
    <div class="arcade-page">
        <h1 class="arcade-title arcade-title--small" style="color: var(--accent-purple);">
            ??? GEHEIM ???
        </h1>

        <!-- Chug type tabs -->
        <div class="leaderboard-tabs mt-1">
            <button v-for="(ct, i) in chugTypes" :key="ct.slug"
                    class="leaderboard-tab"
                    :class="{ selected: focus === 'tab' && tabIdx === i }"
                    @click="pickTab(i)"
                    @mouseenter="pickTab(i)">
                {{ ct.label }}
            </button>
        </div>

        <!-- Leaderboard panel -->
        <div class="leaderboard-panel">
            <div class="leaderboard-title">
                <svg class="btn-icon" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <path d="M8 21h8M12 17v4M5 3h14l-1 8a6 6 0 01-12 0L5 3z" stroke="#b388ff" stroke-width="1.8" stroke-linecap="round"/>
                    <path d="M5 7H2l1 5a3 3 0 003 2M19 7h3l-1 5a3 3 0 01-3 2" stroke="#b388ff" stroke-width="1.8" stroke-linecap="round"/>
                </svg>
                {{ currentLabel }}
            </div>

            <div class="leaderboard-scroll">
                <div v-if="loading" class="leaderboard-empty">Laden...</div>
                <template v-else-if="entries.length === 0">
                    <div class="leaderboard-empty">Nog geen scores voor {{ currentLabel }}</div>
                </template>
                <template v-else>
                    <div v-for="e in entries"
                         :key="e.rank"
                         class="leaderboard-row"
                         :class="rowClass(e.rank)">
                        <span class="leaderboard-rank" :class="rankClass(e.rank)">{{ e.rank }}</span>
                        <span class="leaderboard-name">{{ e.playerName }}</span>
                        <span class="leaderboard-time">{{ e.duration }}</span>
                    </div>
                </template>
            </div>

            <!-- Pagination -->
            <div v-if="pages > 1" class="lb-pager">
                <button class="arcade-btn arcade-btn--back arcade-btn--compact mt-2"
                        :class="{ selected: focus === 'pager' }"
                        @click="go((page % pages) + 1)"
                        @mouseenter="focus = 'pager'">
                    {{ page }}/{{ pages }} &rsaquo;
                </button>
            </div>
        </div>

        <!-- Back -->
        <button class="arcade-btn arcade-btn--back"
                :class="{ selected: focus === 'back' }"
                @click="goBack()"
                @mouseenter="focus = 'back'">
            &lsaquo; TERUG (DRUK ESC)
        </button>

        <!-- Key hint -->
        <div class="arcade-subtitle" style="font-size:.4rem;">
            &#8592;&#8594; NAVIGEREN &bull; ESC TERUG
        </div>
    </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue';
import { useDiaboloMode } from '../composables/useDiaboloMode.js';

const PER_PAGE = 8;

const props = defineProps({
    chugTypes:    { type: Array,  default: () => [] },
    leaderboards: { type: Object, default: () => ({}) },
});

const tabIdx  = ref(0);
const page    = ref(1);
const pages   = ref(1);
const entries = ref([]);
const loading = ref(false);
const focus   = ref('tab');
const diabolo = useDiaboloMode();

const currentLabel = computed(() => props.chugTypes[tabIdx.value]?.label ?? '');

async function fetchPage(slug, p) {
    loading.value = true;
    try {
        const r = await fetch(`/api/leaderboards/paged?type=${slug}&page=${p}&pageSize=${PER_PAGE}`);
        const d = await r.json();
        entries.value = d.entries    ?? [];
        pages.value   = d.totalPages ?? 1;
        page.value    = d.page       ?? 1;
    } catch {
        entries.value = [];
    } finally {
        loading.value = false;
    }
}

function load(tab, p = 1) {
    const s = props.chugTypes[tab]?.slug;
    if (s) fetchPage(s, p);
}

function go(p) {
    if (p >= 1 && p <= pages.value) load(tabIdx.value, p);
}

function pickTab(i) {
    tabIdx.value = i;
    focus.value  = 'tab';
    load(i, 1);
}

function goBack() { window.location.href = '/'; }

function rowClass(r) {
    return r === 1 ? 'leaderboard-row--gold'
         : r === 2 ? 'leaderboard-row--silver'
         : r === 3 ? 'leaderboard-row--bronze' : '';
}

function rankClass(r) {
    return r === 1 ? 'leaderboard-rank--gold'
         : r === 2 ? 'leaderboard-rank--silver'
         : r === 3 ? 'leaderboard-rank--bronze' : '';
}

function onKey(e) {
    diabolo.feedKey(e.code);

    const n      = props.chugTypes.length;
    const paging = pages.value > 1;
    const f      = focus.value;

    if (e.code === 'Escape') { e.preventDefault(); goBack(); return; }

    if (f === 'back') {
        if (e.code === 'ArrowUp') {
            e.preventDefault();
            focus.value = paging ? 'pager' : 'tab';
        } else if (e.code === 'Space' || e.code === 'Enter') {
            e.preventDefault(); goBack();
        }
        return;
    }

    if (f === 'pager') {
        if (e.code === 'ArrowDown')  { e.preventDefault(); focus.value = 'back'; }
        else if (e.code === 'ArrowUp') { e.preventDefault(); focus.value = 'tab'; }
        else if (e.code === 'Space' || e.code === 'Enter') {
            e.preventDefault(); go((page.value % pages.value) + 1);
        }
        return;
    }

    if (n === 0) return;
    if (e.code === 'ArrowRight') {
        e.preventDefault();
        tabIdx.value = (tabIdx.value + 1) % n;
        load(tabIdx.value, 1);
    } else if (e.code === 'ArrowLeft') {
        e.preventDefault();
        tabIdx.value = (tabIdx.value - 1 + n) % n;
        load(tabIdx.value, 1);
    } else if (e.code === 'ArrowDown') {
        e.preventDefault();
        focus.value = paging ? 'pager' : 'back';
    }
}

onMounted(() => {
    document.addEventListener('keydown', onKey);
    if (props.chugTypes.length > 0) {
        const slug = props.chugTypes[0].slug;
        const seed = props.leaderboards[slug];
        if (Array.isArray(seed) && seed.length) entries.value = seed;
        fetchPage(slug, 1);
    }
});
onUnmounted(() => document.removeEventListener('keydown', onKey));
</script>

<style scoped>
.lb-pager {
    display: flex;
    margin-top: 0.6rem;
    padding-top: 0.5rem;
    border-top: 1px solid rgba(255,255,255,.06);
}
</style>
