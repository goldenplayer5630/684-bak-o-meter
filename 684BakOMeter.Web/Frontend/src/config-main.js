import { createApp } from 'vue';
import CalibrationWizard from './components/CalibrationWizard.vue';

const el = document.getElementById('config-app');
if (el) {
    createApp(CalibrationWizard).mount(el);
}
