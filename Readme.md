https://builds.dotnet.microsoft.com/dotnet/Sdk/9.0.308/dotnet-sdk-9.0.308-win-x64.exe

dotnet tool install --global dotnet-ef
dotnet tool install --global dotnet-outdated-tool

dotnet tool update --global dotnet-ef
dotnet tool update --global dotnet-outdated-tool

git clone --quiet https://github.com/cariotan/aspnetcore-htmx.git "$env:USERPROFILE\Documents\aspnetcore-htmx"; dotnet new install "$env:USERPROFILE\Documents\aspnetcore-htmx"

dotnet list package --outdated
dotnet outdated -u --version-lock Major

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

cd ""

mklink /D "1 Models" "Models"
mklink /D "2 Views" "Areas\V1\Views"
mklink /D "2.5 Views (Root)" "Views"
mklink /D "3 Controllers" "Areas\V1\Controllers"
mklink /D "4 Static Class" "Static Class"

<ItemGroup>
	<Compile Remove="1*\**;2*\**;3*\**;4*\**" />
	<Content Remove="1*\**;2*\**;3*\**;4*\**" />
	<None Remove="1*\**;2*\**;3*\**;4*\**" />
</ItemGroup>