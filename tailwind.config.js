module.exports = {
	content: ['3Controllers/*.cs', '2Views/**/*.{cshtml,html}', 'wwwroot/**/*.{css,html,js}', 'Static Classes/*.cs'],
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