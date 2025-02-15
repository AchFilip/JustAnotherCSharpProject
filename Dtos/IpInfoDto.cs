namespace ProjectNovi.Api.Dtos;


//In .NETC#, a record class is a special kind of class that provides a concise way 
//to define immutable data models with built-in functionality for value-based equality,
// meaning two record instances with the same data are considered equal, 
//even if they are different objects in memory.
public record class IpInfoDto(
    int Id,
    string IpAddress,
    string CountryName,
    string TwoLetterCode,
    string ThreeLetterCode,
    DateTime UpdatedAt
);