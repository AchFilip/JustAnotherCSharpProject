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

        // Check Cache --> Check DB --> Check IP2C
        // Save to Cache <--  Save to DB <--  Get from IP2C
        //GET /ip/1
        group.MapGet("/{ipAddress}", async (string ipAddress, ProjectNoviContext dbContext) =>
        {

            //Check Cache
            //return it.
            IpInfoDto? IpInfo = IpInfos.Find(IpInfo => IpInfo.IpAddress == ipAddress);
            if(IpInfo is not null){
                return Results.Ok(IpInfo);
            }


            // if not in cache Check Db
            //save to cache and retun
            


            //if not in db CheckIp2C
            //Save to DB and Cache and return

            if (IpInfo is null)
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
                            CountryId = 1,
                            Country = country
                        };

                        // Save to the database
                        dbContext.Add(ipEntity);
                        await dbContext.SaveChangesAsync();

                    // Return the content as the response
                    return Results.Ok(result);
                }
                else
                {
                    // Return a failure status if the request failed
                    return Results.StatusCode((int)response.StatusCode);
                }
            }



            return Results.NotFound();
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