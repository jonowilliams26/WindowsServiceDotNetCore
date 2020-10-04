using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Monitoring_Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private readonly IHttpClientFactory httpClientFactory;
        private List<string> Urls = new List<string> { "https://www.google.com" };

        public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory)
        {
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PollUrls();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while polling urls");
                }
                finally
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }

        private async Task PollUrls()
        {
            var tasks = new List<Task>();
            foreach(var url in Urls)
            {
                tasks.Add(PollUrl(url));
            }

            await Task.WhenAll(tasks);
        }

        private async Task PollUrl(string url)
        {
            try
            {
                var client = httpClientFactory.CreateClient();
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                    logger.LogInformation("{Url} is online.", url);
                else
                    logger.LogWarning("{Url} is offline.", url);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "{Url} is offline.", url);
            }
        }
    }
}
