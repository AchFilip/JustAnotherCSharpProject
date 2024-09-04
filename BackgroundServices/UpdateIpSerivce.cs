using System.Data.Entity;
using Microsoft.Extensions.Caching.Memory;
using ProjectNovi.Api.Data;
using ProjectNovi.Api.Dtos;
using ProjectNovi.Api.Entities;

namespace ProjectNovi.Api.BackgroundServices;


public class UpdateIpService : BackgroundService
{

    readonly ILogger<UpdateIpService> _logger;
    readonly IServiceProvider _serviceProvider;
    readonly IMemoryCache _cache;

    public UpdateIpService(ILogger<UpdateIpService> logger, IServiceProvider serviceProvider, IMemoryCache cache)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _cache = cache;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UpdateIpService started.");

        while (!stoppingToken.IsCancellationRequested){
            try
            {
                _logger.LogInformation("Starting IP update process...");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ProjectNoviContext>();

                    int batchSize = 100; 
                    int totalIps = dbContext.Ips.Count();

                    //Getting the max time we need to iterate the list of our dbs in order to
                    //Check for udpates on every single ip.
                    int batches = (int)Math.Ceiling((double)totalIps / batchSize);

                    for (int i = 0; i < batches; i++){
                        
                        var ips = dbContext.Ips
                            .OrderBy(ip => ip.Id)
                            .Skip(i * batchSize)
                            .Take(batchSize)
                            .ToList();

                        //I know this is really bad technique
                        //But I could not include the Countries table w/ the ips ones.
                        //Due to time limitation, I did this simple thing :')  
                        var countries = dbContext.Countries
                            .OrderBy(c => c.Id)
                            .Skip(i * batchSize)
                            .Take(batchSize)
                            .ToList();
                        
                        for(int j = 0; j < ips.Count; j++){
                            using var httpClient = new HttpClient();
                            string url = $"http://ip2c.org/{ips[j].IpAddress}";
                            HttpResponseMessage response = await httpClient.GetAsync(url);
                            if(response.IsSuccessStatusCode){
                                    var ip = ips[j];
                                    var country = countries[j];
                                    string content = await response.Content.ReadAsStringAsync();
                                    string[] result = content.Split(';');
                                    IpInfoDto newIP = new IpInfoDto
                                    (
                                        ip.Id,
                                        ip.IpAddress,
                                        result[3],
                                        result[1],
                                        result[2],
                                        DateTime.Now
                                    );
                                    
                                    if(newIP.CountryName != country.CountryName ||
                                        newIP.TwoLetterCode != country.TwoLetterCode ||
                                        newIP.ThreeLetterCode != country.ThreeLetterCode){

                                        var cacheKey = $"IpInfo_{ip.IpAddress}";
                                        if (_cache.TryGetValue(cacheKey, out IpInfoDto cachedIpInfo))
                                        {
                                            _cache.Remove(cacheKey);
                                        }

                                        ip.Country = country;
                                        country.CountryName = newIP.CountryName;
                                        country.TwoLetterCode = newIP.TwoLetterCode;
                                        country.ThreeLetterCode = newIP.ThreeLetterCode;

                                        dbContext.Ips.Update(ip);
                                        dbContext.Countries.Update(country);
                                        await dbContext.SaveChangesAsync();

                                        _logger.LogInformation($"Updated the ip: {ip.IpAddress}");
                                    }
                            }
                        }
                    }
                }

                _logger.LogInformation("IP update process completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating IP information.");
            }

            // Wait for an hour before the next update
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }

        _logger.LogInformation("UpdateIpService stopped.");
    }

    private bool HasIpInfoChanged(IP ip, IpInfoDto newInfo)
    {
        return ip.Country.TwoLetterCode != newInfo.TwoLetterCode ||
               ip.Country.ThreeLetterCode != newInfo.ThreeLetterCode ||
               ip.Country.CountryName != newInfo.CountryName;
    }
}