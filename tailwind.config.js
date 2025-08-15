module.exports = {
	content: ['Controllers/*.cs', 'Views/**/*.{cshtml,html,cs}', 'wwwroot/**/*.{css,html,js}', 'Static Classes/*.cs'],
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