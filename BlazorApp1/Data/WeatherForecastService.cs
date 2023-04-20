using Azure.Identity;
using Microsoft.Graph;
using System.Diagnostics;

namespace BlazorApp1.Data
{
    public class WeatherForecastService
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        public Task<WeatherForecast[]> GetForecastAsync(DateOnly startDate)
        {
            return Task.FromResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }).ToArray());
        }

        public Task<bool> OpenAuth()
        {
            string url = "https://login.microsoftonline.com/930cb5c9-d7f2-4958-9166-cc23ab9952c6/oauth2/v2.0/authorize?client_id=62e38b84-e68d-48b7-be12-998f1dd8c19b&response_type=code&redirect_uri=http://localhost:5190/code&response_mode=query&scope=Presence.Read";

            try
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return Task.FromResult(true);
            //return true;
        }

        public Task<GraphServiceClient> GetCode(string code)
        {
            var scopes = new[] { "Presence.Read" };

            // Multi-tenant apps can use "common",
            // single-tenant apps must use the tenant ID from the Azure portal
            var tenantId = "common";

            // Values from app registration
            var clientId = "YOUR_CLIENT_ID";
            var clientSecret = "YOUR_CLIENT_SECRET";

            // For authorization code flow, the user signs into the Microsoft
            // identity platform, and the browser is redirected back to your app
            // with an authorization code in the query parameters
            var authorizationCode = "AUTH_CODE_FROM_REDIRECT";

            // using Azure.Identity;
            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            // https://learn.microsoft.com/dotnet/api/azure.identity.authorizationcodecredential
            var authCodeCredential = new AuthorizationCodeCredential(
                tenantId, clientId, clientSecret, authorizationCode, options);

            var graphClient = new GraphServiceClient(authCodeCredential, scopes);
            
            return Task.FromResult(graphClient);

            //return Task.FromResult(true);
        }
    }
}