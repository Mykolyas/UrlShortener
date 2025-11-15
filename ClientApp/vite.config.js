import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));

export default defineConfig({
  plugins: [react()],
  build: {
    outDir: '../wwwroot/js/dist',
    emptyOutDir: true,
    rollupOptions: {
      input: {
        main: path.resolve(__dirname, 'src/main.jsx')
      },
      output: {
        entryFileNames: 'url-shortener.js',
        format: 'iife',
        name: 'UrlShortenerApp',
        inlineDynamicImports: true
      }
    }
  },
  server: {
    port: 3000,
    proxy: {
      '/ShortUrl': {
        target: 'http://localhost:5000',
        changeOrigin: true
      }
    }
  }
});

