<template>
    <div class="arcade-page">
        <h1 class="arcade-title arcade-title--small" style="color: var(--accent-purple);">
            ??? GEHEIM MENU ???
        </h1>

        <div class="arcade-subtitle mt-2" style="font-size:.45rem;color:var(--accent-purple);">
            JE HEBT HET GEVONDEN!
        </div>

        <div class="menu-list mt-2">
            <div v-for="(item, i) in menuItems" :key="item.id">
                <button class="arcade-btn mt-1"
                        :class="[item.btnClass, { selected: selectedIndex === i }]"
                        @click="activateItem(i)"
                        @mouseenter="selectedIndex = i">
                    <svg class="btn-icon" viewBox="0 0 24 24" fill="none" v-html="item.iconSvg"></svg>
                    {{ item.label }}
                </button>
            </div>
        </div>

        <div class="arcade-subtitle mt-2" style="font-size:.4rem;">
            &#8593;&#8595; NAVIGEREN &bull; SPATIE/ENTER SELECTEREN &bull; ESC TERUG
        </div>
    </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import { useKeyController } from '../composables/useKeyController.js';
import { useGlobalMusic } from '../composables/useGlobalMusic.js';

// Use the centralized global music service
useGlobalMusic();

const ICONS = {
    beer:  '<path d="M5 3h11l-1 13a4 4 0 01-4 4H9a4 4 0 01-4-4L5 3z" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/><path d="M5 7h13v3a4 4 0 01-3 3.87" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>',
    trophy:'<path d="M8 21h8M12 17v4M5 3h14l-1 8a6 6 0 01-12 0L5 3z" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/><path d="M5 7H2l1 5a3 3 0 003 2M19 7h3l-1 5a3 3 0 01-3 2" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>',
};

const props = defineProps({
    chugTypes: { type: Array, default: () => [] },
});

const selectedIndex = ref(0);

const menuItems = computed(() => {
    const items = props.chugTypes.map(ct => ({
        id: ct.slug,
        label: ct.label,
        iconSvg: ICONS.beer,
        btnClass: 'arcade-btn--primary',
        action: () => { window.location.href = `/play?type=${ct.slug}`; },
    }));
    items.push({
        id: 'hidden-leaderboards',
        label: 'Geheime Leaderboards',
        iconSvg: ICONS.trophy,
        btnClass: '',
        action: () => { window.location.href = '/hiddenleaderboards'; },
    });
    items.push({
        id: 'back',
        label: '‹ TERUG (DRUK ESC)',
        iconSvg: '',
        btnClass: 'arcade-btn--back',
        action: () => { window.location.href = '/'; },
    });
    return items;
});

function activateItem(i) {
    selectedIndex.value = i;
    const item = menuItems.value[i];
    if (item?.action) item.action();
}

useKeyController({
    onEscape: () => { window.location.href = '/'; },
    onDown: () => { selectedIndex.value = (selectedIndex.value + 1) % menuItems.value.length; },
    onUp: () => { selectedIndex.value = (selectedIndex.value - 1 + menuItems.value.length) % menuItems.value.length; },
    onActivate: () => { activateItem(selectedIndex.value); },
});
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
