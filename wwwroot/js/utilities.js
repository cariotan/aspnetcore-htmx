function utilities_saveUrlParameter(key, value)
{
	const url = new URL(window.location.href)
	url.searchParams.set(key, value)
	window.history.replaceState({}, '', url.toString())
}

function utilities_getUrlParameter(key)
{
	const url = new URL(window.location.href)
	return url.searchParams.get(key)
}