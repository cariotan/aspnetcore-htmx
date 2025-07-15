module.exports = {
    content: ['3 C/*.cs', '2 V/**/*.{cshtml,html}', 'wwwroot/**/*.{css,html,js}', 'Static Classes/*.cs'],
    theme: {
        extend: {
            colors: {
            },
            screens: {
            },
        },
        fontFamily: {
        },
    },
    plugins: [
        require('@tailwindcss/forms')({
            strategy: 'class',
        }),
    ],
}