namespace ProjectNovi.Api.Entities;


public class IP
{
    public int Id {get; set;}

    public required string IpAddress {get;set;}
    public int CountryId {get; set;}
    public required Country Country {get; set;}
}