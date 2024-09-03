namespace ProjectNovi.Api.Entities;


public class Country
{
    public int Id {get; set;}

    public required string CountryName {get; set;}

    public required string TwoLetterCode {get; set;}

    public required string ThreeLetterCode {get; set;}

    public DateTime UpdatedAt {get; set;}
}