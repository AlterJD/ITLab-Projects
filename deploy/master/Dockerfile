FROM mcr.microsoft.com/dotnet/core-nightly/aspnet:3.0.0-preview8-alpine3.9

WORKDIR /app
COPY itlab-projects-master/itlab-projects-master .
ENTRYPOINT ["dotnet", "ITLab.Projects.dll"]