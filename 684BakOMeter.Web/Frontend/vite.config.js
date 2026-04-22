import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import { resolve } from 'path';

export default defineConfig({
    plugins: [vue()],
    appType: 'custom',
    root: resolve(__dirname),
    build: {
        outDir: resolve(__dirname, '../wwwroot/dist'),
        emptyOutDir: true,
        rollupOptions: {
            input: {
                'index-main':              resolve(__dirname, 'src/index-main.js'),
                'play-main':               resolve(__dirname, 'src/play-main.js'),
                'leaderboards-main':       resolve(__dirname, 'src/leaderboards-main.js'),
                'personal-main':           resolve(__dirname, 'src/personal-main.js'),
                'settings-main':           resolve(__dirname, 'src/settings-main.js'),
                'manager-main':            resolve(__dirname, 'src/manager-main.js'),
                'hidden-menu-main':        resolve(__dirname, 'src/hidden-menu-main.js'),
                'hidden-leaderboards-main': resolve(__dirname, 'src/hidden-leaderboards-main.js'),
            },
            output: {
                entryFileNames: '[name].js',
                chunkFileNames: 'chunks/[name]-[hash].js',
                assetFileNames: 'assets/[name]-[hash][extname]',
            },
        },
    },
});
