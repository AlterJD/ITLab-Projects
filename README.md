# Projects service for RTUITLab

## Prerequriments

* Net Core 2.2.301
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
    "DB_TYPE": "TYPE",
    "ConnectionStrings": {
        "Postgres": "CONNECTION STRING"
    }
}
```

**DB_TYPE** - type of the database which will be used. Can be:

* **IN_MEMORY** - use in memory database, use for debug or first start
* **POSTGRES** - use PostgreSQL database with connection string from ```ConnectionStrings:Postgres```

## Run
```bash
cd ./src/ITLab.Projects
dotnet run
```
API will be available on [localhost:54052](http://localhost:54052)