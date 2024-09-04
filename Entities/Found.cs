namespace ProjectNovi.Api.Entities;


public class Found
{
    public int Id {get; set;}

    public required string CountryName {get; set;}

    public DateTime UpdatedAt {get; set;}

    public int TimesFound {get;set;}
}