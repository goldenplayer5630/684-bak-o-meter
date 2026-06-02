import { ref, watch, onMounted, onUnmounted } from 'vue';
import { useAppMode } from './useAppMode.js';

const STORAGE_KEY = 'bak-o-meter-volume';

/**
 * Centralized global music service that automatically determines which song
 * to play based on the server-authoritative active mode.
 *
 * Music priority (matches ApplicationMode enum):
 *   Diabolo    => hell-yeah.mp3
 *   Lion       => nero.mp3
 *   Klootviool => klootviool.mp3
 *   Dysis      => dysis.mp3
 *   Kwaak      => kwaak.mp3
 *   Kompas     => oers.mp3
 *   Dms        => trekeenbak.mp3  (same as Official — no dedicated track)
 *   Official   => trekeenbak.mp3
 */

let globalAudio  = null;
let activeInstances = 0;

const MUSIC_MAP = {
    Diabolo:    '/music/hell-yeah.mp3',
    Lion:       '/music/nero.mp3',
    Klootviool: '/music/klootviool.mp3',
    Dysis:      '/music/dysis.mp3',
    Kwaak:      '/music/kwaak.mp3',
    Kompas:     '/music/oers.mp3',
};
const DEFAULT_MUSIC = '/music/trekeenbak.mp3';

function trackForMode(modeName) {
    return MUSIC_MAP[modeName] ?? DEFAULT_MUSIC;
}

function getStoredVolume() {
    const v = localStorage.getItem(STORAGE_KEY);
    return v !== null ? parseInt(v, 10) : 50;
}

function initOrUpdateAudio(src) {
    if (!globalAudio) {
        globalAudio = new Audio(src);
        globalAudio.loop = true;
        globalAudio.volume = getStoredVolume() / 100;
    } else if (globalAudio.src !== new URL(src, window.location.origin).href) {
        const wasPlaying    = !globalAudio.paused;
        const currentVolume = globalAudio.volume;
        globalAudio.pause();
        globalAudio.src    = src;
        globalAudio.volume = currentVolume;
        if (wasPlaying) tryPlay(globalAudio);
    }
}

function tryPlay(audio) {
    audio.play().catch(() => {
        const resume = () => {
            audio.play().catch(() => {});
            document.removeEventListener('click',   resume);
            document.removeEventListener('keydown', resume);
        };
        document.addEventListener('click',   resume, { once: true });
        document.addEventListener('keydown', resume, { once: true });
    });
}

export function useGlobalMusic() {
    const { activeMode, loadMode } = useAppMode();
    const currentSrc = ref(trackForMode(activeMode.value));

    activeInstances++;

    function updateMusic() {
        const newSrc = trackForMode(activeMode.value);
        if (currentSrc.value !== newSrc) {
            currentSrc.value = newSrc;
            initOrUpdateAudio(newSrc);
        }
    }

    function onVolumeChange(e) {
        if (globalAudio) globalAudio.volume = (e.detail ?? 50) / 100;
    }

    // Still listen for the legacy mode-change custom event (dispatched by useDifferentBgMode)
    function onModeChange() { updateMusic(); }

    onMounted(async () => {
        await loadMode();
        currentSrc.value = trackForMode(activeMode.value);
        initOrUpdateAudio(currentSrc.value);

        if (globalAudio && activeInstances === 1) tryPlay(globalAudio);

        window.addEventListener('volume-change', onVolumeChange);
        window.addEventListener('mode-change',   onModeChange);
    });

    // Watch reactive mode changes (e.g. from useDifferentBgMode in same session)
    watch(activeMode, () => updateMusic());

    onUnmounted(() => {
        activeInstances--;
        window.removeEventListener('volume-change', onVolumeChange);
        window.removeEventListener('mode-change',   onModeChange);

        if (activeInstances === 0 && globalAudio) {
            globalAudio.pause();
            globalAudio.src = '';
            globalAudio = null;
        }
    });

    return { currentSrc };
}