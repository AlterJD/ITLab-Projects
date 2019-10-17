FROM mcr.microsoft.com/dotnet/core-nightly/sdk:3.1.100-preview2-alpine3.10 as build

WORKDIR /app
COPY ./ITLab.Projects.sln ./ITLab.Projects.sln
COPY ./src/ITLab.Projects/ITLab.Projects.fsproj ./src/ITLab.Projects/
COPY ./src/ITLab.Projects.Database/ITLab.Projects.Database.csproj ./src/ITLab.Projects.Database/
COPY ./src/ITLab.Projects.Models/ITLab.Projects.Models.fsproj ./src/ITLab.Projects.Models/
COPY ./src/ITLab.Projects.PublicAPI/ITLab.Projects.PublicAPI.fsproj ./src/ITLab.Projects.PublicAPI/

COPY ./tests/ITLab.Projects.Tests/ITLab.Projects.Tests.fsproj ./tests/ITLab.Projects.Tests/

RUN dotnet restore

COPY . ./

RUN cp ./src/ITLab.Projects/appsettings.TestMace.json ./src/ITLab.Projects/appsettings.Secret.json
RUN echo "{}" > ./src/ITLab.Projects.Database/appsettings.Secret.json

RUN dotnet publish ./src/ITLab.Projects/ITLab.Projects.fsproj -o out

FROM mcr.microsoft.com/dotnet/core-nightly/aspnet:3.1.0-preview1-alpine3.10
WORKDIR /app
COPY --from=build /app/out .
CMD ["dotnet", "ITLab.Projects.dll"]