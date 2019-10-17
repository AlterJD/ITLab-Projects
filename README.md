# Projects service for RTUITLab

master | develop | tests
--- | --- | ---
[![Build Status][build-master-image]][build-master-link] | [![Build Status][build-dev-image]][build-dev-link] | ![Azure DevOps tests](https://img.shields.io/azure-devops/tests/RTUITLab/RTU%20IT%20Lab/66?label=%20&style=plastic)

[build-master-image]: https://dev.azure.com/rtuitlab/RTU%20IT%20Lab/_apis/build/status/ITLab-Projects?branchName=master
[build-master-link]: https://dev.azure.com/rtuitlab/RTU%20IT%20Lab/_build/latest?definitionId=66&branchName=master
[build-dev-image]: https://dev.azure.com/rtuitlab/RTU%20IT%20Lab/_apis/build/status/ITLab-Projects?branchName=develop
[build-dev-link]: https://dev.azure.com/rtuitlab/RTU%20IT%20Lab/_build/latest?definitionId=66&branchName=develop

## Prerequriments

* Net Core 3
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
		"Authority": "URL of identity server"
	}
}
```

**DB_TYPE** - type of the database which will be used. Possible types:

* **IN_MEMORY** - to use in memory database for debug or first start
* **POSTGRES** - to use PostgreSQL database with connection string from ```ConnectionStrings:Postgres```

**FILL_DEBUG_DB** ( true | false ) - fill database with default values

**TESTS** ( true | false ) - if true: acess token not required

**AUTHORIZATION**

Section JWT store all data for authorize users. Application require scope ```itlab.projects```.

## Run
```bash
cd ./src/ITLab.Projects
dotnet run
```
API will be available on [localhost:54052](http://localhost:54052)

<!-- TODO: run tests -->