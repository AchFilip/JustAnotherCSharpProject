using System.Data.Entity;
using System.Xml.Linq;
using ProjectNovi.Api.Data;
using ProjectNovi.Api.Dtos;
using ProjectNovi.Api.Entities;

namespace ProjectNovi.Api.Endpoints;


public static class IpInfoEndpoints
{
    private static readonly List<IpInfoDto> IpInfos = [

    ];

    public static RouteGroupBuilder MapIpInfoEndpoints(this WebApplication app)
    {


        var group = app.MapGroup("ip");

        //GET /ip
        group.MapGet("/", () => IpInfos);

        //GET /ip/dbs 
        //Receives data from db.
        group.MapGet("/dbs", (ProjectNoviContext dbContext) => 
        {
            var ips = dbContext.Ips.ToList();

            List<IpInfoDto> results = [];

            for(int i = 0 ; i < ips.Count; i++){
                results.Add(new IpInfoDto
                (
                    ips[i].Id,
                    ips[i].IpAddress,
                    ips[i].Country.CountryName,
                    ips[i].Country.TwoLetterCode,
                    ips[i].Country.ThreeLetterCode
                ));
            }
            
            return Results.Ok(ips);
        });

        //Task 1
        group.MapGet("/{ipAddress}", async (string ipAddress, ProjectNoviContext dbContext) =>
        {

            //Check Cache


            //Check DB
            //Get the ips from the db.
            var ip = dbContext.Ips
                    .Where(ip => ip.IpAddress == ipAddress);

            //Check if is empty
            if(ip.Any())
                return Results.Ok(ip);


            //if not in db CheckIp2C
            //Save to DB and Cache and return

            
                using var httpClient = new HttpClient();

                // Build the URL with the provided IP address
                string url = $"http://ip2c.org/{ipAddress}";

                // Send a GET request to the external service
                HttpResponseMessage response = await httpClient.GetAsync(url);

                // Ensure the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a string
                    string content = await response.Content.ReadAsStringAsync();

                    // Split the string by semicolon
                    string[] result = content.Split(';');
    
                    IpInfos.Add(new IpInfoDto(
                        IpInfos.Count + 1,
                        ipAddress,
                        result[3],
                        result[1],
                        result[2]
                    ));


                    // Create a new Country and IP entity
                    var country = new Country
                    {
                        Id = IpInfos.Count,
                        CountryName = result[3],
                        TwoLetterCode = result[1],
                        ThreeLetterCode = result[2]
                    };

                    var ipEntity = new IP
                    {
                        Id = IpInfos.Count,
                        IpAddress = ipAddress,
                        CountryId = dbContext.Countries.Count() + 1,
                        Country = country
                    };

                    // Save to the database
                    dbContext.Add(ipEntity);
                    await dbContext.SaveChangesAsync();


                    // Return the content as the response
                    return Results.Ok(IpInfos[IpInfos.Count-1]);
                }
                else
                {
                    // Return a failure status if the request failed
                    return Results.StatusCode((int)response.StatusCode);
                }
        });

        //Task 3
        group.MapGet("/sql/{twolettercode?}", (string[]? twolettercode, ProjectNoviContext dbContext) => 
        {
                if(twolettercode == null || twolettercode.Length == 0){
                    return Results.Ok($"No Countries");
                }

                //Get the ips from the db.
                var ips = dbContext.Ips.ToList();

                for(int i = 0 ; i<twolettercode.Length; i++){
                    var code = twolettercode[i];
                    var country = dbContext.Countries
                                .Where(c => c.TwoLetterCode == code);

                    if(country is not null)
                        return Results.Ok(country);
                }

                return Results.Ok(twolettercode);
        });

        group.MapGet("/ip2c/{ipAddress}", async (string ipAddress) =>
        {
            using var httpClient = new HttpClient();

            // Build the URL with the provided IP address
            string url = $"http://ip2c.org/{ipAddress}";

            // Send a GET request to the external service
            HttpResponseMessage response = await httpClient.GetAsync(url);

            // Ensure the request was successful
            if (response.IsSuccessStatusCode)
            {
                // Read the response content as a string
                string content = await response.Content.ReadAsStringAsync();

                // Split the string by semicolon
                string[] result = content.Split(';');

                IpInfos.Add(new IpInfoDto(
                    IpInfos.Count + 1,
                    ipAddress,
                    result[3],
                    result[1],
                    result[2]
                ));

                // Return the content as the response
                return Results.Ok(result);
            }
            else
            {
                // Return a failure status if the request failed
                return Results.StatusCode((int)response.StatusCode);
            }
        });



        return group;
    }
}