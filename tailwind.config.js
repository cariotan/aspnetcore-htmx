module.exports = {
	content: [
		'Controllers/*.cs',
		'Views/**/*.{cshtml,html,cs}',
		'wwwroot/**/*.{html,js}',
		'Static Class/*.cs',
		'Areas/**/*.{cshtml,html}',
		'!wwwroot/js/alpine.js',
		'!wwwroot/js/htmx.js',
		'!wwwroot/js/signalr/dist/browser/signalr.js',
		'!./node_modules',
	],
	theme: {
		extend: {
			colors: {
			},
			screens: {
				ml: '896px',
				lr: '1152px'
			},
			fontFamily: {
				sans: ['Poppins']
			}
		},
		fontFamily: {
		}
	},
	darkMode: 'selector',
	plugins: [
		require('@tailwindcss/forms')({
			strategy: 'class',
		}),
	],
}