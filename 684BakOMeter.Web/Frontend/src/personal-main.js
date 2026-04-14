import { createApp } from 'vue';
import PersonalPage from './components/PersonalPage.vue';

const el = document.getElementById('personal-app');
if (el) {
    createApp(PersonalPage).mount(el);
}
