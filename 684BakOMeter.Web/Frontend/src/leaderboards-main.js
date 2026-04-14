import { createApp } from 'vue';
import LeaderboardsPage from './components/LeaderboardsPage.vue';

const el = document.getElementById('leaderboards-app');
if (el) {
    const chugTypes = JSON.parse(el.dataset.chugTypes || '[]');
    const leaderboards = JSON.parse(el.dataset.leaderboards || '{}');

    createApp(LeaderboardsPage, { chugTypes, leaderboards }).mount(el);
}
