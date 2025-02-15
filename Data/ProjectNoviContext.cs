using Microsoft.EntityFrameworkCore;
using ProjectNovi.Api.Entities;


namespace ProjectNovi.Api.Data;


public class ProjectNoviContext(DbContextOptions<ProjectNoviContext> options) 
    : DbContext(options)
{
    public DbSet<IP> Ips => Set<IP>();

    public DbSet<Country> Countries => Set<Country>();


    public DbSet<CountriesTask3> T3_Countries => Set<CountriesTask3>();
}