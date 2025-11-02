import type { Config } from 'tailwindcss';
import forms from '@tailwindcss/forms';
import typography from '@tailwindcss/typography';
import daisyui from 'daisyui';

const config: Config = {
  content: ['./src/**/*.{html,ts}'],
  theme: {
    extend: {},
  },
  plugins: [forms, typography, daisyui],
  daisyui: {
    themes: ['dim', 'light'],
    darkTheme: 'dim',
  },
};

export default config;
