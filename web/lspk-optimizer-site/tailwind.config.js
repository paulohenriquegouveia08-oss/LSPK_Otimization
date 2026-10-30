/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        lspk: {
          void: "#0B0F17",
          surface: "#131B2E",
          surfaceLight: "#1B253D",
          border: "#1E293B",
          borderGlow: "#334155",
          cyan: "#00F2FE",
          blue: "#4FACFE",
          green: "#10B981",
          amber: "#F59E0B",
          coral: "#EF4444",
        }
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
      }
    },
  },
  plugins: [],
}
