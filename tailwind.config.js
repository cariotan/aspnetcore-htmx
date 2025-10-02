import { theme } from './wwwroot/js/theme.js'

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
	],
	theme: theme,
	plugins: [
		require('@tailwindcss/forms')({
			strategy: 'class',
		}),
	],
}
