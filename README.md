# Projects service for RTUITLab

master | develop | tests
--- | --- | ---
[![Build Status][build-master-image]][build-master-link] | [![Build Status][build-dev-image]][build-dev-link] | ![Azure DevOps tests](https://img.shields.io/azure-devops/tests/RTUITLab/RTU%20IT%20Lab/66?label=%20&style=plastic)

[build-master-image]: https://dev.azure.com/rtuitlab/RTU%20IT%20Lab/_apis/build/status/ITLab-Projects?branchName=master
[build-master-link]: https://dev.azure.com/rtuitlab/RTU%20IT%20Lab/_build/latest?definitionId=66&branchName=master
[build-dev-image]: https://dev.azure.com/rtuitlab/RTU%20IT%20Lab/_apis/build/status/ITLab-Projects?branchName=develop
[build-dev-link]: https://dev.azure.com/rtuitlab/RTU%20IT%20Lab/_build/latest?definitionId=66&branchName=develop

## Prerequriments

* Net Core 3.1
* PostgreSQL

## Configuration

### Database project

```appsettings.Secret.json``` must be placed to the ```src/ITLab.Projects.Database/``` folder

```json
{
    "ConnectionStrings": {
        "Postgres": "CONNECTION STRING"
    }
}
```

### Main project

```appsettings.Secret.json``` must be placed to the ```src/ITLab.Projects/``` folder


```json
{
    "DB_TYPE": "IN_MEMORY",
    "FILL_DEBUG_DB": true,
    "TESTS": true,
    "ConnectionStrings": {
        "Postgres": "CONNECTION STRING"
    },
    "JWT" : {
        "DebugKey": "123456789010111213",
        "Authority": "URL of identity server"
    }
}
```

**DB_TYPE** - type of the database which will be used. Possible types:

* **IN_MEMORY** - to use in memory database for debug or first start
* **POSTGRES** - to use PostgreSQL database with connection string from ```ConnectionStrings:Postgres```

**FILL_DEBUG_DB** ( true | false ) - fill database with default values

**TESTS** ( true | false ) - if true: acess token writes in first line of console output, use it for authorize

**AUTHORIZATION**

Section **JWT** store all data for authorize users. Application require scope ```itlab.projects```.
* **DebugKey** - uses when TESTS in true state, key for signing access token
* **Authority** - uses when TESTS in false state, authority for validating production jwt token

## Run
```bash
cd ./src/ITLab.Projects
dotnet run
```
API will be available on [localhost:54052](http://localhost:54052)

## Tests

### Unit tests (not yet available)

To run unit tests in root folder invoke next command:

```shell
dotnet test tests/ITLab.Projects.Tests/ITLab.projects.Tests.fsproj
```

### Integration tests

#### Prerequriments

* Docker compose
To run integration tests run follow commands in folder ```tests/docker```

```shell
docker-compose build
docker-compose up -d web-app
docker-compose up testmace
docker-compose rm -s -f
```

Test results will be displayed in console output, and saved to xml file in JUnit format

Integration tests uses [Testmace](https://testmace.com/) for testing API.
