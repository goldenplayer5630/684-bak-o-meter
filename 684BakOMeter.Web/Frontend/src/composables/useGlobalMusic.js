import { ref, watch, onMounted, onUnmounted } from 'vue';

const STORAGE_KEY = 'bak-o-meter-volume';

/**
 * Centralized global music service that automatically determines which song
 * to play based on the active mode (diabolo, KLC/lion, klootviool, dysis, kwaak, kompas, or normal).
 * 
 * Music priority:
 * 1. Diabolo mode (666) → hell-yeah.mp3
 * 2. KLC/Lion mode (klc) → nero.mp3
 * 3. Klootviool mode (klootviool) → klootviool.mp3
 * 4. Dysis mode (427) → dysis.mp3
 * 5. Kwaak mode (kwaak) → kwaak.mp3
 * 6. Kompas mode (kompas) → oers.mp3
 * 7. Normal mode → trekeenbak.mp3
 * 
 * This composable maintains a single audio instance and automatically
 * switches tracks when modes change via localStorage and custom events.
 */

// Singleton audio instance shared across all component instances
let globalAudio = null;
let activeInstances = 0;

const STORAGE_DIABOLO    = 'diabolo';
const STORAGE_LION       = 'lionmode';
const STORAGE_KLOOTVIOOL = 'klootvioolmode';
const STORAGE_DYSIS      = 'dysismode';
const STORAGE_KWAAK      = 'kwaakmode';
const STORAGE_KOMPAS     = 'kompasmode';

/**
 * Determines which music file should be playing based on current modes.
 * @returns {string} Path to the music file
 */
function determineCurrentMusic() {
    // Check localStorage for active modes
    const diaboloActive    = localStorage.getItem(STORAGE_DIABOLO) === '1';
    const lionActive       = localStorage.getItem(STORAGE_LION) === '1';
    const klootvioolActive = localStorage.getItem(STORAGE_KLOOTVIOOL) === '1';
    const dysisActive      = localStorage.getItem(STORAGE_DYSIS) === '1';
    const kwaakActive      = localStorage.getItem(STORAGE_KWAAK) === '1';
    const kompasActive     = localStorage.getItem(STORAGE_KOMPAS) === '1';

    // Priority order: diabolo > lion > klootviool > dysis > kwaak > kompas > normal
    if (diaboloActive) {
        return '/music/hell-yeah.mp3';
    }
    if (lionActive) {
        return '/music/nero.mp3';
    }
    if (klootvioolActive) {
        return '/music/klootviool.mp3';
    }
    if (dysisActive) {
        return '/music/dysis.mp3';
    }
    if (kwaakActive) {
        return '/music/kwaak.mp3';
    }
    if (kompasActive) {
        return '/music/oers.mp3';
    }
    return '/music/trekeenbak.mp3';
}

/**
 * Gets the stored volume from localStorage.
 * @returns {number} Volume value (0-100)
 */
function getStoredVolume() {
    const v = localStorage.getItem(STORAGE_KEY);
    return v !== null ? parseInt(v, 10) : 50;
}

/**
 * Initializes or updates the global audio instance.
 * @param {string} src - Path to the music file
 */
function initOrUpdateAudio(src) {
    if (!globalAudio) {
        globalAudio = new Audio(src);
        globalAudio.loop = true;
        globalAudio.volume = getStoredVolume() / 100;
    } else if (globalAudio.src !== new URL(src, window.location.origin).href) {
        // Song needs to change
        const wasPlaying = !globalAudio.paused;
        const currentVolume = globalAudio.volume;

        globalAudio.pause();
        globalAudio.src = src;
        globalAudio.volume = currentVolume;

        if (wasPlaying) {
            tryPlay(globalAudio);
        }
    }
}

/**
 * Attempts to play audio, handling browser autoplay restrictions.
 * @param {HTMLAudioElement} audio - The audio element to play
 */
function tryPlay(audio) {
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

/**
 * Main composable for global music management.
 * Should be called once per page/component that needs music.
 */
export function useGlobalMusic() {
    const currentSrc = ref(determineCurrentMusic());

    activeInstances++;

    /**
     * Updates the current music based on mode changes.
     */
    function updateMusic() {
        const newSrc = determineCurrentMusic();
        if (currentSrc.value !== newSrc) {
            currentSrc.value = newSrc;
            initOrUpdateAudio(newSrc);
        }
    }

    /**
     * Handles volume changes from the settings.
     * @param {CustomEvent} e - Volume change event
     */
    function onVolumeChange(e) {
        if (globalAudio) {
            globalAudio.volume = (e.detail ?? 50) / 100;
        }
    }

    /**
     * Handles storage changes from other tabs/windows or mode toggles.
     * @param {StorageEvent} e - Storage event
     */
    function onStorageChange(e) {
        if (e.key === STORAGE_DIABOLO || e.key === STORAGE_LION || e.key === STORAGE_KLOOTVIOOL || e.key === STORAGE_DYSIS || e.key === STORAGE_KWAAK || e.key === STORAGE_KOMPAS) {
            updateMusic();
        }
    }

    /**
     * Handles custom mode-change events dispatched when modes toggle.
     * @param {CustomEvent} e - Mode change event
     */
    function onModeChange() {
        updateMusic();
    }

    onMounted(() => {
        // Initialize audio with current mode's music
        initOrUpdateAudio(currentSrc.value);

        // Start playing if this is the first instance
        if (globalAudio && activeInstances === 1) {
            tryPlay(globalAudio);
        }

        // Listen for volume changes
        window.addEventListener('volume-change', onVolumeChange);

        // Listen for storage changes (cross-tab sync and mode toggles)
        window.addEventListener('storage', onStorageChange);

        // Listen for custom mode-change events (same-page mode toggles)
        window.addEventListener('mode-change', onModeChange);
    });

    onUnmounted(() => {
        activeInstances--;

        // Clean up listeners
        window.removeEventListener('volume-change', onVolumeChange);
        window.removeEventListener('storage', onStorageChange);
        window.removeEventListener('mode-change', onModeChange);

        // Only stop and cleanup audio when no instances remain
        if (activeInstances === 0 && globalAudio) {
            globalAudio.pause();
            globalAudio.src = '';
            globalAudio = null;
        }
    });

    return {
        currentSrc,
        audio: globalAudio,
    };
}
