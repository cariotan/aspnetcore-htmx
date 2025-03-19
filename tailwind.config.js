module.exports = {
    content: ['controllers/*.cs', 'views/**/*.{cshtml,html}', 'wwwroot/**/*.{css,html,js}', 'Static Classes/*.cs'],
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