using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ProjectNovi.Api.Data;
using ProjectNovi.Api.Dtos;
using ProjectNovi.Api.Entities;

using System.IO;
using System.Text;
using System.Data.SqlClient;

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
            var ips = dbContext.Ips
                    .Include(i => i.Country).ToList();

            return Results.Ok(ips);
        });


        //Task 1
        group.MapGet("/{ipAddress}", async (string ipAddress, ProjectNoviContext dbContext, IMemoryCache cache) =>
        {
            // Check the cache first
            var cacheKey = $"IpInfo_{ipAddress}";
            if (cache.TryGetValue(cacheKey, out IpInfoDto cachedIpInfo))
            {
                return Results.Ok(cachedIpInfo);
            }

            //Check Database
            var ip = await dbContext.Ips
                    .Include(ip => ip.Country)        //Include is not that optimized.
                    .Where(ip => ip.IpAddress == ipAddress)
                    .FirstOrDefaultAsync();

            if (ip != null)
            {
                IpInfoDto IpInfos = new IpInfoDto(
                    ip.Id,
                    ipAddress,
                    ip.Country.CountryName,
                    ip.Country.TwoLetterCode,
                    ip.Country.ThreeLetterCode,
                    ip.Country.UpdatedAt
                );
                cache.Set(cacheKey, IpInfos, TimeSpan.FromHours(1)); // Cache for 1 hour
                return Results.Ok(IpInfos);
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
                    result[2],
                    DateTime.Now
                );

                var country = new Country
                {
                    Id = IpInfos.Id,
                    CountryName = result[3],
                    TwoLetterCode = result[1],
                    ThreeLetterCode = result[2],
                    UpdatedAt = IpInfos.UpdatedAt
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
                return Results.Ok(IpInfos);
            }
            else
            {
                // Return a failure status if the request failed
                return Results.StatusCode((int)response.StatusCode);
            }
        });

        //Task 3
        group.MapGet("/sql/{twolettercode}", (string[]? twolettercode, ProjectNoviContext dbContext) =>
        {
            //If null return everything
            if (twolettercode[0] == "null")
            {
                var countries = dbContext.Set<Found>()
                    .FromSqlRaw(@"
                        SELECT 
                            *,
                            COUNT(*) AS TimesFound,
                            MAX(UpdatedAt) AS LastUpdated
                        FROM 
                            Countries
                        GROUP BY 
                            CountryName")
                    .ToList();

                countries = countries.OrderByDescending(c => c.TimesFound).ToList();
                
                string result = "";

                foreach(var t in countries){
                    result +=  "{CountryName: " + t.CountryName + 
                                " AddressesCount: " + t.TimesFound + 
                                " LastAddressUpdated: " + t.UpdatedAt.ToString()
                                 + "}\n";
                }
                
                Console.WriteLine(result);
                return Results.Ok( result + " From Null");
            }

            if (twolettercode.Length == 0)
            {
                return Results.NotFound($"You did not gave any countries");
            }


            //TODO: EntityFramework -> Raw SQL for specific
            var parameterizedCodes = string.
            Join(",", twolettercode.Select((code, index) => $"@p{index}"));

            var sqlQuery = $@"
                        SELECT 
                            CountryName,
                            COUNT(*) AS TimesFound,
                            MAX(UpdatedAt) AS LastUpdated
                        FROM 
                            Countries
                        WHERE 
                            TwoLetterCode IN ({parameterizedCodes})
                        GROUP BY 
                            CountryName";

            // Create the parameters array
            // var parameters = twolettercode.Select((code, index) => new SqlParameter($"@p{index}", code)).ToArray();

            // // Execute the query with the parameters
            // var countryReports = dbContext.Set<Country>()
            //     .FromSqlRaw(sqlQuery, parameters)
            //     .ToList();
                
            return Results.Ok(twolettercode);
        });

        return group;
    }
}