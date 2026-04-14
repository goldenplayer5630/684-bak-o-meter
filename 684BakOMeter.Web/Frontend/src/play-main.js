import { createApp } from 'vue';
import PlayFlow from './components/PlayFlow.vue';

const el = document.getElementById('play-app');
if (el) {
    const data = JSON.parse(el.dataset.play || '{}');

    createApp(PlayFlow, {
        chugTypeSlug: data.chugTypeSlug || 'Bak',
        chugTypeLabel: data.chugTypeLabel || 'Bak',
    }).mount(el);
}
