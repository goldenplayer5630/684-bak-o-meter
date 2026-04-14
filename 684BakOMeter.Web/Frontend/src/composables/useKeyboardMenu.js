import { ref, onMounted, onUnmounted } from 'vue';

/**
 * Reusable keyboard-navigated menu composable.
 * Supports arrow keys (Up/Down/Left/Right), Enter, Space, Escape.
 *
 * @param {Object} options
 * @param {import('vue').Ref<number>} options.itemCount - total number of items
 * @param {number} [options.columns=1] - columns in grid layout for Left/Right nav
 * @param {Function} options.onActivate - called with the selected index on Enter/Space
 * @param {Function} [options.onEscape] - called on Escape
 * @param {Function} [options.onNavigate] - called on any navigation (for idle reset)
 */
export function useKeyboardMenu({ itemCount, columns = 1, onActivate, onEscape, onNavigate }) {
    const selectedIndex = ref(0);

    function moveNext() {
        const total = typeof itemCount === 'function' ? itemCount() : itemCount.value ?? itemCount;
        selectedIndex.value = (selectedIndex.value + 1) % total;
        onNavigate?.();
    }

    function movePrevious() {
        const total = typeof itemCount === 'function' ? itemCount() : itemCount.value ?? itemCount;
        selectedIndex.value = (selectedIndex.value - 1 + total) % total;
        onNavigate?.();
    }

    function moveDown() {
        const total = typeof itemCount === 'function' ? itemCount() : itemCount.value ?? itemCount;
        const cols = typeof columns === 'function' ? columns() : columns;
        selectedIndex.value = Math.min(selectedIndex.value + cols, total - 1);
        onNavigate?.();
    }

    function moveUp() {
        const cols = typeof columns === 'function' ? columns() : columns;
        selectedIndex.value = Math.max(selectedIndex.value - cols, 0);
        onNavigate?.();
    }

    function setSelected(i) {
        selectedIndex.value = i;
        onNavigate?.();
    }

    function activateSelected() {
        onActivate?.(selectedIndex.value);
    }

    function handleKeyDown(e) {
        const code = e.code;
        if (code === 'ArrowRight') { e.preventDefault(); moveNext(); }
        else if (code === 'ArrowLeft') { e.preventDefault(); movePrevious(); }
        else if (code === 'ArrowDown') { e.preventDefault(); moveDown(); }
        else if (code === 'ArrowUp') { e.preventDefault(); moveUp(); }
        else if (code === 'Enter' || code === 'Space') { e.preventDefault(); activateSelected(); }
        else if (code === 'Escape') { e.preventDefault(); onEscape?.(); }
    }

    onMounted(() => document.addEventListener('keydown', handleKeyDown));
    onUnmounted(() => document.removeEventListener('keydown', handleKeyDown));

    return {
        selectedIndex,
        setSelected,
        moveNext,
        movePrevious,
        activateSelected,
    };
}
