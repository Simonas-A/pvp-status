﻿// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");
using Azure.Identity;
using Microsoft.Graph;
using static Microsoft.Graph.Constants;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.Identity.Client;
using System.Security.Claims;
using Microsoft.Identity.Client.Extensions.Msal;
using Azure.Core;
using System.Web;
using System.Net;
using pvp_status;
using Microsoft.Graph.ExternalConnectors;
using Microsoft.Extensions.Configuration;

internal class Program
{
    private static async Task Main(string[] args)
    {
        GraphServiceClient GetServiceClient()
        {
            var scopes = new[] { "User.Read" };

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

            return graphClient;
        }

        //var graphClient = GetServiceClient();

        //GraphServiceClient graphClient = new GraphServiceClient(authProvider);

        //var presence = await graphClient.Me.Presence
        //    .Request()
        //    .GetAsync();



        async Task<string> GetStatus()
        {
            string[] scopes = new string[] { "user.read" };
            //string accessToken = "eyJ0eXAiOiJKV1QiLCJub25jZSI6Imp3UjRHRnhzTnlCbUpCUHkwNXhMcHJsQmNQSzhWUmlFOFBUUk5nSnZpNnMiLCJhbGciOiJSUzI1NiIsIng1dCI6Ii1LSTNROW5OUjdiUm9meG1lWm9YcWJIWkdldyIsImtpZCI6Ii1LSTNROW5OUjdiUm9meG1lWm9YcWJIWkdldyJ9.eyJhdWQiOiIwMDAwMDAwMy0wMDAwLTAwMDAtYzAwMC0wMDAwMDAwMDAwMDAiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC8zNDE1ZjJmNy1mNWE4LTQwOTItYjUyYS0wMDNhYWY4NDQ4NTMvIiwiaWF0IjoxNjc2Mzk1NDMzLCJuYmYiOjE2NzYzOTU0MzMsImV4cCI6MTY3NjM5OTU3MiwiYWNjdCI6MCwiYWNyIjoiMSIsImFpbyI6IkFUUUF5LzhUQUFBQURuM2tKa09YUG5BTm41V3c5cDJqRWhRL1dNVGRKYnRJdTN4SHlFaVJ5NVF5NWJGSnhrOS9rWDBHMTYvT0lscjAiLCJhbXIiOlsicHdkIl0sImFwcF9kaXNwbGF5bmFtZSI6IkdyYXBoIEV4cGxvcmVyIiwiYXBwaWQiOiJkZThiYzhiNS1kOWY5LTQ4YjEtYThhZC1iNzQ4ZGE3MjUwNjQiLCJhcHBpZGFjciI6IjAiLCJkZXZpY2VpZCI6IjUzMDA2NDllLWJiNDYtNDE0NC04YTZmLWI3ZWJkNWQ0ZWMzMSIsImZhbWlseV9uYW1lIjoiQWxicmVjaHRhcyIsImdpdmVuX25hbWUiOiJTaW1vbmFzIiwiaWR0eXAiOiJ1c2VyIiwiaXBhZGRyIjoiNzguNjAuMTQ3LjE0OSIsIm5hbWUiOiJBbGJyZWNodGFzIFNpbW9uYXMiLCJvaWQiOiJjYTU1YThjYi0xOTM1LTQ5YzMtOGI1Mi1jN2QxMjYzYjZlZDUiLCJvbnByZW1fc2lkIjoiUy0xLTUtMjEtMTAwMzYxOTAyMS0yMjQ3MzY2MDY0LTE0MjE2MjQ3ODEtMjE4MDQ3IiwicGxhdGYiOiIzIiwicHVpZCI6IjEwMDMyMDAwREJGM0M5OEUiLCJyaCI6IjAuQVRzQTlfSVZOS2oxa2tDMUtnQTZyNFJJVXdNQUFBQUFBQUFBd0FBQUFBQUFBQUE3QU1ZLiIsInNjcCI6IkNhbGVuZGFycy5SZWFkIENhbGVuZGFycy5SZWFkV3JpdGUgb3BlbmlkIHByb2ZpbGUgVXNlci5SZWFkIGVtYWlsIFByZXNlbmNlLlJlYWQuQWxsIiwic3ViIjoiR3Z2d1FwbEFrSHRNMWZiRHMyeWYwOVZiaEJVdlhFNnItMHRSUHdZTHc5USIsInRlbmFudF9yZWdpb25fc2NvcGUiOiJFVSIsInRpZCI6IjM0MTVmMmY3LWY1YTgtNDA5Mi1iNTJhLTAwM2FhZjg0NDg1MyIsInVuaXF1ZV9uYW1lIjoic2ltYWxiQGt0dS5sdCIsInVwbiI6InNpbWFsYkBrdHUubHQiLCJ1dGkiOiJHRzlkMk9HYzRFQ0xzVFJfTVZZVkFBIiwidmVyIjoiMS4wIiwid2lkcyI6WyJiNzlmYmY0ZC0zZWY5LTQ2ODktODE0My03NmIxOTRlODU1MDkiXSwieG1zX3N0Ijp7InN1YiI6IkdQeEZhTk8tT2Y0NTcwWUhOazMxSVBUaUhFcHZUOHNwcFZPN3BqRFUwVDQifSwieG1zX3RjZHQiOjEzNjc1NDQwMTEsInhtc190ZGJyIjoiRVUifQ.EfIsgTvym2-KpVxabJ2n-dkChZYhDrLw1XxOga9R-9Bsp8um7KB5K2tN3Ryqb3CJCP4dRPpwEpiam-iiF8iCIXjOn5-x8J9E0lylc-E2nlv_IfQwNp0L_3JeMLtsFBfcAEDaj8YmFttt65Gugbor6k_kiSnUBwyhc6ZFxHjvFNzvN-VOTOSwhU2FjjqjDsyDT03tYFuWY2fjoVxLqYpp5tpB0kLHPjBka4hojw0locnrkLRK1RXEZBOlJOdXjr347q2QR6uO3UeOREdDHIW5o0uXUhY4Gm78t822n14ITWpvSrYMBg6pEo8MQX1T9Iec-KpNjEtX1QVpIokJaQCwOg";
            //string accessToken = "eyJ0eXAiOiJKV1QiLCJub25jZSI6Imp3UjRHRnhzTnlCbUpCUHkwNXhMcHJsQmNQSzhWUmlFOFBUUk5nSnZpNnMiLCJhbGciOiJSUzI1NiIsIng1dCI6Ii1LSTNROW5OUjdiUm9meG1lWm9YcWJIWkdldyIsImtpZCI6Ii1LSTNROW5OUjdiUm9meG1lWm9YcWJIWkdldyJ9.eyJhdWQiOiIwMDAwMDAwMy0wMDAwLTAwMDAtYzAwMC0wMDAwMDAwMDAwMDAiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC8zNDE1ZjJmNy1mNWE4LTQwOTItYjUyYS0wMDNhYWY4NDQ4NTMvIiwiaWF0IjoxNjc2Mzk1NDMzLCJuYmYiOjE2NzYzOTU0MzMsImV4cCI6MTY3NjM5OTU3MiwiYWNjdCI6MCwiYWNyIjoiMSIsImFpbyI6IkFUUUF5LzhUQUFBQURuM2tKa09YUG5BTm41V3c5cDJqRWhRL1dNVGRKYnRJdTN4SHlFaVJ5NVF5NWJGSnhrOS9rWDBHMTYvT0lscjAiLCJhbXIiOlsicHdkIl0sImFwcF9kaXNwbGF5bmFtZSI6IkdyYXBoIEV4cGxvcmVyIiwiYXBwaWQiOiJkZThiYzhiNS1kOWY5LTQ4YjEtYThhZC1iNzQ4ZGE3MjUwNjQiLCJhcHBpZGFjciI6IjAiLCJkZXZpY2VpZCI6IjUzMDA2NDllLWJiNDYtNDE0NC04YTZmLWI3ZWJkNWQ0ZWMzMSIsImZhbWlseV9uYW1lIjoiQWxicmVjaHRhcyIsImdpdmVuX25hbWUiOiJTaW1vbmFzIiwiaWR0eXAiOiJ1c2VyIiwiaXBhZGRyIjoiNzguNjAuMTQ3LjE0OSIsIm5hbWUiOiJBbGJyZWNodGFzIFNpbW9uYXMiLCJvaWQiOiJjYTU1YThjYi0xOTM1LTQ5YzMtOGI1Mi1jN2QxMjYzYjZlZDUiLCJvbnByZW1fc2lkIjoiUy0xLTUtMjEtMTAwMzYxOTAyMS0yMjQ3MzY2MDY0LTE0MjE2MjQ3ODEtMjE4MDQ3IiwicGxhdGYiOiIzIiwicHVpZCI6IjEwMDMyMDAwREJGM0M5OEUiLCJyaCI6IjAuQVRzQTlfSVZOS2oxa2tDMUtnQTZyNFJJVXdNQUFBQUFBQUFBd0FBQUFBQUFBQUE3QU1ZLiIsInNjcCI6IkNhbGVuZGFycy5SZWFkIENhbGVuZGFycy5SZWFkV3JpdGUgb3BlbmlkIHByb2ZpbGUgVXNlci5SZWFkIGVtYWlsIFByZXNlbmNlLlJlYWQuQWxsIiwic3ViIjoiR3Z2d1FwbEFrSHRNMWZiRHMyeWYwOVZiaEJVdlhFNnItMHRSUHdZTHc5USIsInRlbmFudF9yZWdpb25fc2NvcGUiOiJFVSIsInRpZCI6IjM0MTVmMmY3LWY1YTgtNDA5Mi1iNTJhLTAwM2FhZjg0NDg1MyIsInVuaXF1ZV9uYW1lIjoic2ltYWxiQGt0dS5sdCIsInVwbiI6InNpbWFsYkBrdHUubHQiLCJ1dGkiOiJHRzlkMk9HYzRFQ0xzVFJfTVZZVkFBIiwidmVyIjoiMS4wIiwid2lkcyI6WyJiNzlmYmY0ZC0zZWY5LTQ2ODktODE0My03NmIxOTRlODU1MDkiXSwieG1zX3N0Ijp7InN1YiI6IkdQeEZhTk8tT2Y0NTcwWUhOazMxSVBUaUhFcHZUOHNwcFZPN3BqRFUwVDQifSwieG1zX3RjZHQiOjEzNjc1NDQwMTEsInhtc190ZGJyIjoiRVUifQ.EfIsgTvym2-KpVxabJ2n-dkChZYhDrLw1XxOga9R-9Bsp8um7KB5K2tN3Ryqb3CJCP4dRPpwEpiam-iiF8iCIXjOn5-x8J9E0lylc-E2nlv_IfQwNp0L_3JeMLtsFBfcAEDaj8YmFttt65Gugbor6k_kiSnUBwyhc6ZFxHjvFNzvN-VOTOSwhU2FjjqjDsyDT03tYFuWY2fjoVxLqYpp5tpB0kLHPjBka4hojw0locnrkLRK1RXEZBOlJOdXjr347q2QR6uO3UeOREdDHIW5o0uXUhY4Gm78t822n14ITWpvSrYMBg6pEo8MQX1T9Iec-KpNjEtX1QVpIokJaQCwOg";
            //string accessToken = "eyJ0eXAiOiJKV1QiLCJub25jZSI6IlhrSEdka3E4RnZ6YmNRYzQ3dmNQUkNxY19Dck9oellaNi1HYW1fRGRSOWMiLCJhbGciOiJSUzI1NiIsIng1dCI6Ii1LSTNROW5OUjdiUm9meG1lWm9YcWJIWkdldyIsImtpZCI6Ii1LSTNROW5OUjdiUm9meG1lWm9YcWJIWkdldyJ9.eyJhdWQiOiIwMDAwMDAwMy0wMDAwLTAwMDAtYzAwMC0wMDAwMDAwMDAwMDAiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC8zNDE1ZjJmNy1mNWE4LTQwOTItYjUyYS0wMDNhYWY4NDQ4NTMvIiwiaWF0IjoxNjc2NDExNTQ1LCJuYmYiOjE2NzY0MTE1NDUsImV4cCI6MTY3NjQxNzEwMCwiYWNjdCI6MCwiYWNyIjoiMSIsImFpbyI6IkFUUUF5LzhUQUFBQURuM2tKa09YUG5BTm41V3c5cDJqRWhRL1dNVGRKYnRJdTN4SHlFaVJ5NVF5NWJGSnhrOS9rWDBHMTYvT0lscjAiLCJhbXIiOlsicHdkIl0sImFwcF9kaXNwbGF5bmFtZSI6IkdyYXBoIEV4cGxvcmVyIiwiYXBwaWQiOiJkZThiYzhiNS1kOWY5LTQ4YjEtYThhZC1iNzQ4ZGE3MjUwNjQiLCJhcHBpZGFjciI6IjAiLCJkZXZpY2VpZCI6IjUzMDA2NDllLWJiNDYtNDE0NC04YTZmLWI3ZWJkNWQ0ZWMzMSIsImZhbWlseV9uYW1lIjoiQWxicmVjaHRhcyIsImdpdmVuX25hbWUiOiJTaW1vbmFzIiwiaWR0eXAiOiJ1c2VyIiwiaXBhZGRyIjoiNzguNjAuMTQ3LjE0OSIsIm5hbWUiOiJBbGJyZWNodGFzIFNpbW9uYXMiLCJvaWQiOiJjYTU1YThjYi0xOTM1LTQ5YzMtOGI1Mi1jN2QxMjYzYjZlZDUiLCJvbnByZW1fc2lkIjoiUy0xLTUtMjEtMTAwMzYxOTAyMS0yMjQ3MzY2MDY0LTE0MjE2MjQ3ODEtMjE4MDQ3IiwicGxhdGYiOiIzIiwicHVpZCI6IjEwMDMyMDAwREJGM0M5OEUiLCJyaCI6IjAuQVRzQTlfSVZOS2oxa2tDMUtnQTZyNFJJVXdNQUFBQUFBQUFBd0FBQUFBQUFBQUE3QU1ZLiIsInNjcCI6IkNhbGVuZGFycy5SZWFkIENhbGVuZGFycy5SZWFkV3JpdGUgb3BlbmlkIFByZXNlbmNlLlJlYWQgUHJlc2VuY2UuUmVhZC5BbGwgcHJvZmlsZSBVc2VyLlJlYWQgZW1haWwiLCJzdWIiOiJHdnZ3UXBsQWtIdE0xZmJEczJ5ZjA5VmJoQlV2WEU2ci0wdFJQd1lMdzlRIiwidGVuYW50X3JlZ2lvbl9zY29wZSI6IkVVIiwidGlkIjoiMzQxNWYyZjctZjVhOC00MDkyLWI1MmEtMDAzYWFmODQ0ODUzIiwidW5pcXVlX25hbWUiOiJzaW1hbGJAa3R1Lmx0IiwidXBuIjoic2ltYWxiQGt0dS5sdCIsInV0aSI6IjRJZ0F4SUt6N1VLd1VvbHpMQ01kQUEiLCJ2ZXIiOiIxLjAiLCJ3aWRzIjpbImI3OWZiZjRkLTNlZjktNDY4OS04MTQzLTc2YjE5NGU4NTUwOSJdLCJ4bXNfc3QiOnsic3ViIjoiR1B4RmFOTy1PZjQ1NzBZSE5rMzFJUFRpSEVwdlQ4c3BwVk83cGpEVTBUNCJ9LCJ4bXNfdGNkdCI6MTM2NzU0NDAxMSwieG1zX3RkYnIiOiJFVSJ9.L4eDFIfUFpHeLrOaUVafZBqNZw2VyLCOSeFXKyDJlu398fn-tdaCVH74fJMFgKjwzbOlGHhPNi0Wbr9UCdWKKfYM7aanF9B9j_JiG5mSdHF-4xaoByjG_WkN5oB-PyIDcsDDnN-FoMfJ3fLrY5fGAoqqwCw-OIUJsisFaoHpmfk35PksTpiPpebmNTQr6mwawhqon5LqYsgFuMOiVZJzr8_oLGIEUiZ_R7ftQiymlCR1LIs9Kdr24oQKfUe6SwM-eWLqFdzzWt891Du_U0rfKGYxnz96SUb8n9nIIUEdMZPM9VJdFgBRBGar_GqBYeg8vhU5cHpDrj7SbNlZhBsDoQ";
            // Use the access token to call a protected web API.

            string uri = "https://login.microsoftonline.com/930cb5c9-d7f2-4958-9166-cc23ab9952c6/oauth2/v2.0/authorize?client_id=62e38b84-e68d-48b7-be12-998f1dd8c19b&response_type=code&redirect_uri=http%3A%2F%2Flocalhost:553&response_mode=query&scope=offline_access%20user.read%20mail.read&state=12345";

            var authenticator = new Authenticator();
            string accessToken = await authenticator.GetAccessToken(uri);
            //string[] scopes = new string[] { "user.read" };

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            //client.DefaultRequestHeaders.
            string url = "https://graph.microsoft.com/v1.0/me/presence";
            return await client.GetStringAsync(url);
        }

        char a = ' ';

        while (a == ' ')
        {
            string status = await GetStatus();
            // Newtonsoft json deserialize
            var userPresence = JsonConvert.DeserializeObject<UserPresence>(status);

            Console.WriteLine(userPresence.activity);
            a = Console.ReadLine()[0];
        }
    }
}

class UserPresence
{
    public UserPresence(string odatacontext, string id, string availability, string activity)
    {
        this.odatacontext = odatacontext;
        this.id = id;
        this.availability = availability;
        this.activity = activity;
    }

    public string odatacontext { get; set; }
    public string id { get; set; }
    public string availability { get; set; }
    public string activity { get; set; }
}
