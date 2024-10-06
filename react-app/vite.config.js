import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [react()],
    build: {
        manifest: true,  // Enable the generation of manifest.json
        outDir: '../wwwroot/dir',  // Optional: Define your output directory
        rollupOptions: {
            input: {
                main: path.resolve("src", 'main.jsx'),  // Use main.jsx as the entry
            },
        },
    },
    base: '/dir/'
})
