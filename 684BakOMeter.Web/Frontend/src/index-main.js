import { createApp } from 'vue';
import ArcadeMenu from './components/ArcadeMenu.vue';

const el = document.getElementById('arcade-menu-app');
if (el) {
    const chugTypes = JSON.parse(el.dataset.chugTypes || '[]');
    const leaderboards = JSON.parse(el.dataset.leaderboards || '{}');

    createApp(ArcadeMenu, { chugTypes, leaderboards }).mount(el);
}
