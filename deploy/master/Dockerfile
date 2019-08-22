FROM mcr.microsoft.com/dotnet/core-nightly/aspnet:3.0.0-preview8-alpine3.9

WORKDIR /app
COPY itlab-projects/itlab-projects-build .
ENTRYPOINT ["dotnet", "ITLab.Projects.dll"]