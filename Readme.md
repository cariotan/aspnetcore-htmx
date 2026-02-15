https://builds.dotnet.microsoft.com/dotnet/Sdk/9.0.308/dotnet-sdk-9.0.308-win-x64.exe

dotnet tool install --global dotnet-ef
dotnet tool install --global dotnet-outdated-tool

dotnet tool update --global dotnet-ef
dotnet tool update --global dotnet-outdated-tool

git clone --quiet https://github.com/cariotan/aspnetcore-htmx.git "$env:USERPROFILE\Documents\aspnetcore-htmx"; dotnet new install "$env:USERPROFILE\Documents\aspnetcore-htmx"

dotnet outdated -u --version-lock Minor

New-Item -Type File -Path $profile -Force
notepad $profile
. $profile
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser

function db {
	dotnet build @args
}

function dr {
	dotnet run @args
}

function d {
	dotnet watch @args
}

function dbr {
	dotnet build -c Release
}

function drr {
	dotnet run -c Release
}

function dwr {
	dotnet watch -c Release
}