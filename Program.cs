using ProjectNovi.Api.Data;
using ProjectNovi.Api.Endpoints;


var builder = WebApplication.CreateBuilder(args);

//Connection string to db. Getting the info from appsettings.json
var connString = builder.Configuration.GetConnectionString("ProjectNovi");
builder.Services.AddSqlite<ProjectNoviContext>(connString);

var app = builder.Build();


//Maps the endpoints
app.MapIpInfoEndpoints();

//Migrate DB
app.MigrateDb();


app.Run();