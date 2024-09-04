namespace ProjectNovi.Api.Entities;


public class CountriesTask3
{
    public int Id {get; set;}

    public required string CountryName {get; set;}

    public DateTime UpdatedAt {get; set;}

    public int TimesFound {get;set;}
}