import { ref, onMounted, onUnmounted } from 'vue';

const STORAGE_KEY = 'bak-o-meter-volume';

/**
 * Plays looping background music and syncs volume with localStorage + the
 * 'volume-change' custom event dispatched by VolumeSettings.
 *
 * @param {string} src - path to the audio file (e.g. '/music/trekeenbak.mp3')
 */
export function useBgMusic(src) {
    const audio = new Audio(src);
    audio.loop = true;
    audio.volume = getStoredVolume() / 100;

    function getStoredVolume() {
        const v = localStorage.getItem(STORAGE_KEY);
        return v !== null ? parseInt(v, 10) : 50;
    }

    function onVolumeChange(e) {
        audio.volume = (e.detail ?? 50) / 100;
    }

    function tryPlay() {
        audio.play().catch(() => {
            // Browser blocked autoplay — retry on first user interaction
            const resume = () => {
                audio.play().catch(() => {});
                document.removeEventListener('click', resume);
                document.removeEventListener('keydown', resume);
            };
            document.addEventListener('click', resume, { once: true });
            document.addEventListener('keydown', resume, { once: true });
        });
    }

    onMounted(() => {
        window.addEventListener('volume-change', onVolumeChange);
        tryPlay();
    });

    onUnmounted(() => {
        window.removeEventListener('volume-change', onVolumeChange);
        audio.pause();
        audio.src = '';
    });

    return { audio };
}
