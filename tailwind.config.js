import { theme } from './wwwroot/js/theme.js'

module.exports = {
	content: ['Controllers/*.cs', 'Views/**/*.{cshtml,html,cs}', 'wwwroot/**/*.{css,html,js}', 'Static Classes/*.cs', 'Areas/**/*.{cshtml,html}'],
	theme: theme,
	plugins: [
		require('@tailwindcss/forms')({
			strategy: 'class',
		}),
	],
}