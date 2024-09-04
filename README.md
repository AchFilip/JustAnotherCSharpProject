# Project Novi

This project provides a REST API built with .NET Core to retrieve and manage information about IP addresses. The service uses a SQL database and integrates with the IP2C web service to fetch and update IP address information.


## Tasks
* An **endpoint** that receives an IP address. The backend first checks if the IP exists in the cache. If not found, it checks the database. If still not found, it fetches the information from the IP2C service. If the data is retrieved from the database, it is then saved to the cache. If the data is fetched from IP2C, it is saved in the database and subsequently stored in the cache.
* A **Background Service** that will check every hour in batches of 100, if the information we have for every IP stored in our database have changed or not.
* An **endpoint** that reiceves either null or a query of TwoLetterCode. If null print a country name, the times it appears in our DB and the last updated time. If a query, do the same but only for the countries with the TwoLetterCode.


## Run the project

Simply run: 

```
dotnet run
```

If you want to delete the Database & the Migrations you need to re-create them by executing the following commands:

```
dotnet ef migrations add InitialCreate --output-dir Data/Migrations
dotnet ef database update
```
