https://builds.dotnet.microsoft.com/dotnet/Sdk/9.0.303/dotnet-sdk-9.0.303-win-x64.exe

dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef

git clone --quiet https://github.com/cariotan/aspnetcore-htmx.git "$env:USERPROFILE\Documents\aspnetcore-htmx"; dotnet new install "$env:USERPROFILE\Documents\aspnetcore-htmx"