import { createApp } from 'vue';
import ManagerPage from './components/ManagerPage.vue';

const el = document.getElementById('manager-app');
if (el) {
    createApp(ManagerPage).mount(el);
}
