import { createApp } from 'vue';
import UserSettings from './components/UserSettings.vue';

const el = document.getElementById('settings-app');
if (el) {
    createApp(UserSettings).mount(el);
}
