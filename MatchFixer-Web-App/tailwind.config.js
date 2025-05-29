// tailwind.config.js
/** @type {import('tailwindcss').Config} */
module.exports = {
    prefix: 'tw-',
    content: [
        './Views/**/*.cshtml', // This is crucial for .NET MVC Razor views
        './Pages/**/*.cshtml', // If you're using Razor Pages
        './wwwroot/**/*.html', // If you have any raw HTML files in wwwroot
        // Add any other paths where you use Tailwind classes
        // For example, if you have any JS files in a 'scripts' folder that dynamically add classes:
        // './Scripts/**/*.js',
        // './src/**/*.{js,ts,jsx,tsx}', // Keep this if you actually have client-side JS/TS in src/
    ],
    theme: {
        extend: {},
    },
    plugins: [],
    corePlugins: {
        preflight: false,
    },
}