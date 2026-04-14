import { createApp } from 'vue';
import HiddenLeaderboardsPage from './components/HiddenLeaderboardsPage.vue';

const el = document.getElementById('hidden-leaderboards-app');
if (el) {
    const chugTypes = JSON.parse(el.dataset.chugTypes || '[]');
    const leaderboards = JSON.parse(el.dataset.leaderboards || '{}');

    createApp(HiddenLeaderboardsPage, { chugTypes, leaderboards }).mount(el);
}
