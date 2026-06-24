module.exports = {
  darkMode: "class",
  content: [
    "./Components/**/*.razor",
    "./Components/**/*.razor.js",
    "./wwwroot/js/**/*.js"
  ],
  theme: {
    extend: {
      colors: {
        primary: "#f2df0d",
        "primary-hover": "#f7e85f",
        "background-light": "#f8f8f5",
        "background-dark": "#0F2E2E",
        "teal-deep": "#081a1a"
      },
      fontFamily: {
        display: ["Inter", "sans-serif"]
      },
      borderRadius: {
        DEFAULT: "0.5rem",
        lg: "1rem",
        xl: "1.5rem",
        full: "9999px"
      }
    }
  },
  plugins: [
    require("@tailwindcss/forms"),
    require("@tailwindcss/container-queries"),
    require("@tailwindcss/typography")
  ]
};
