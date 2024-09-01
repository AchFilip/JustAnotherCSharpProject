using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ProjectNovi.Api.Data;
using ProjectNovi.Api.Dtos;
using ProjectNovi.Api.Entities;

namespace ProjectNovi.Api.Endpoints;


public static class IpInfoEndpoints
{
    public static RouteGroupBuilder MapIpInfoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("ip");

        //GET /ip/dbs 
        //Receives data from db.
        group.MapGet("/dbs", (ProjectNoviContext dbContext) =>
        {
            var ips = dbContext.Ips.ToList();

            List<IpInfoDto> results = [];

            for (int i = 0; i < ips.Count; i++)
            {
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
        group.MapGet("/{ipAddress}", async (string ipAddress, ProjectNoviContext dbContext, IMemoryCache cache) =>
        {
            // Check the cache first
            var cacheKey = $"IpInfo_{ipAddress}";
            if (cache.TryGetValue(cacheKey, out IpInfoDto cachedIpInfo))
            {
                return Results.Ok(cachedIpInfo + " in Cache");
            }

            //Check Database
            var ip = await dbContext.Ips
                    .Include(ip => ip.Country)        //Include is not that optimized.
                    .Where(ip => ip.IpAddress == ipAddress)
                    .FirstOrDefaultAsync();

            if (ip != null)
            {
                cache.Set(cacheKey, ip, TimeSpan.FromHours(1)); // Cache for 1 hour
                return Results.Ok(ip + " in DB");
            }

            //If not in Database, fetch from IP2C
            using var httpClient = new HttpClient();

            string url = $"http://ip2c.org/{ipAddress}";
            HttpResponseMessage response = await httpClient.GetAsync(url);

            // Ensure the request was successful
            if (response.IsSuccessStatusCode)
            {
                // Read the response content as a string
                string content = await response.Content.ReadAsStringAsync();

                // Split the string by semicolon
                string[] result = content.Split(';');

                int dbLength = await dbContext.Ips.CountAsync(); 
                IpInfoDto IpInfos = new IpInfoDto(
                    dbLength + 1,
                    ipAddress,
                    result[3],
                    result[1],
                    result[2]
                );

                var country = new Country
                {
                    Id = IpInfos.Id,
                    CountryName = result[3],
                    TwoLetterCode = result[1],
                    ThreeLetterCode = result[2]
                };

                var ipEntity = new IP
                {
                    Id = IpInfos.Id,
                    IpAddress = ipAddress,
                    CountryId = dbContext.Countries.Count() + 1,
                    Country = country
                };

                // Save to the database
                dbContext.Add(ipEntity);
                await dbContext.SaveChangesAsync();

                cache.Set(cacheKey, IpInfos, TimeSpan.FromHours(1)); // Cache for 1 hour

                // Return the content as the response
                return Results.Ok(IpInfos + " Found in IP2C");
            }
            else
            {
                // Return a failure status if the request failed
                return Results.StatusCode((int)response.StatusCode);
            }
        });

        //Task 3
        //TODO: EntityFramework -> Raw SQL
        //TODO: Create a .csv report
        //TODO: Add last edited time
        group.MapGet("/sql/{twolettercode}", (string[]? twolettercode, ProjectNoviContext dbContext) =>
        {
            //If null return everything
            if (twolettercode[0] == "null")
            {
                var allCountriesQuery = @"
                        SELECT c.CountryName, 
                            COUNT(ip.Id) AS AddressesCount
                        FROM Countries c
                        LEFT JOIN IPs ip ON ip.CountryId = c.Id
                        GROUP BY c.CountryName";

                var allCountries = dbContext.Countries
                                .FromSqlRaw(allCountriesQuery)
                                .ToList();

                return Results.Ok(allCountries + " From Null");
            }

            if (twolettercode.Length == 0)
            {
                return Results.NotFound($"You did not gave any countries");
            }

            //TODO: Change entityframework to raw sql
            //Get the ips from the db.
            var ips = dbContext.Ips.ToList();

            for (int i = 0; i < twolettercode.Length; i++)
            {
                var code = twolettercode[i];
                var country = dbContext.Countries
                            .Where(c => c.TwoLetterCode == code);

                if (country is not null)
                    return Results.Ok(country);
            }

            return Results.Ok(twolettercode);
        });

        return group;
    }
}