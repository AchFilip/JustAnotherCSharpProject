using Microsoft.EntityFrameworkCore;
using ProjectNovi.Api.Entities;


namespace ProjectNovi.Api.Data;


public class ProjectNoviContext(DbContextOptions<ProjectNoviContext> options) 
    : DbContext(options)
{
    public DbSet<IP> Ips => Set<IP>();

    public DbSet<Country> Countries => Set<Country>();

    // protected override void OnModelCreating(ModelBuilder modelBuilder)
    // {
    //     //Create Migrations
    //     //dotnet ef migrations add InitialCreate --output-dir Data/Migrations

    //     //Generate DB
    //     //dotnet ef database update

    //     //Seed Data.
    //     //dotnet ef migrations add SeedGenres --output-dir Data/Migrations
    //     modelBuilder.Entity<Country>().HasData(
    //         new {Id = 1, CountryName= "Greece", TwoLetterCode = "GR", ThreeLetterCode = "GRC"}
    //     );

    // }
}