import { createApp } from 'vue';
import HiddenMenu from './components/HiddenMenu.vue';

const el = document.getElementById('hidden-menu-app');
if (el) {
    const chugTypes = JSON.parse(el.dataset.chugTypes || '[]');
    createApp(HiddenMenu, { chugTypes }).mount(el);
}
