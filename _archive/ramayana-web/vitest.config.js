import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    environment: 'jsdom',
    include: [
      'tests/unit/**/*.test.{js,mjs}',
      'tests/smoke/**/*.test.{js,mjs}',
    ],
    // Playwright tests are tagged with .e2e — opt-in via separate run.
    // We exclude them from the default `vitest run` since they need a server.
    exclude: ['tests/e2e/**'],
    testTimeout: 30_000,
    hookTimeout: 30_000,
    globals: false,
    reporters: ['default'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'html'],
      include: ['js/**/*.js'],
      exclude: ['js/main.js', 'js/__mocks__/**', 'js/world/scene.js', 'js/core/audio.js'],
    },
  },
  resolve: {
    alias: {
      three: new URL('./js/__mocks__/three.js', import.meta.url).pathname,
    },
  },
});
