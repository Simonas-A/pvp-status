﻿// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
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
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

internal class Program
{
    private static async Task Main(string[] args)
    {
        //string authToken = GetAuthorizationToken();
        //Console.WriteLine(authToken);

        var tenantId = "930cb5c9-d7f2-4958-9166-cc23ab9952c6";
        var clientId = "62e38b84-e68d-48b7-be12-998f1dd8c19b";
        var clientSecret = "Bxr8Q~hQJrHsDovZp4AYrzPnN4lgDR1SkTZfGa4G";
        var userId = "ca55a8cb-1935-49c3-8b52-c7d1263b6ed5";

        var token = await getToken();

        Console.WriteLine(token.Availability);

        

        //var url = $"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/authorize?client_id={clientId}&response_type=code&redirect_uri=http%3A%2F%2Flocalhost%2Fmyapp%2F&response_mode=query&scope=https%3A%2F%2Fgraph.microsoft.com%2Fmail.read&state=12345&code_challenge=YTFjNjI1OWYzMzA3MTI4ZDY2Njg5M2RkNmVjNDE5YmEyZGRhOGYyM2IzNjdmZWFhMTQ1ODg3NDcxY2Nl&code_challenge_method=S256";

        GraphServiceClient GetServiceClient(string code)
        {
            var scopes = new[] { "Presence.Read" };

            // Multi-tenant apps can use "common",
            // single-tenant apps must use the tenant ID from the Azure portal
            var tenantId = "930cb5c9-d7f2-4958-9166-cc23ab9952c6";

            // Values from app registration
            var clientId = "62e38b84-e68d-48b7-be12-998f1dd8c19b";
            var clientSecret = "Bxr8Q~hQJrHsDovZp4AYrzPnN4lgDR1SkTZfGa4G";

            // For authorization code flow, the user signs into the Microsoft
            // identity platform, and the browser is redirected back to your app
            // with an authorization code in the query parameters
            var authorizationCode = code;

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

        //var serviceClient = GetServiceClient();

        //var graphClient = new GraphServiceClient(serviceClient);

        //var result = await graphClient.Me.Presence.ToString();

        //var graphClient = GetServiceClient();

        //GraphServiceClient graphClient = new GraphServiceClient(authProvider);

        //var presence = await graphClient.Me.Presence
        //    .Request()
        //    .GetAsync();

        string GetAuthorizationToken()
        {
            var tenantId = "930cb5c9-d7f2-4958-9166-cc23ab9952c6";
            var clientId = "62e38b84-e68d-48b7-be12-998f1dd8c19b";
            var clientSecret = "Bxr8Q~hQJrHsDovZp4AYrzPnN4lgDR1SkTZfGa4G";
            //var scopes = new[] { "User.Read" };

            var cc = new Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential(clientId, clientSecret);
            var context = new AuthenticationContext("https://login.microsoftonline.com/" + tenantId);
            var result = context.AcquireTokenAsync("https://management.azure.com/", cc);
            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the Access token");
            }
            var AccessToken = result.Result.AccessToken;
            return AccessToken;
        }

        async Task<Presence> getToken()
        {
            var scopes = new[] { "Presence.Read" };

            // Multi-tenant apps can use "common",
            // single-tenant apps must use the tenant ID from the Azure portal
            //var tenantId = "common";

            // Values from app registration
            //var clientId = "YOUR_CLIENT_ID";
            //var clientSecret = "YOUR_CLIENT_SECRET";

            // For authorization code flow, the user signs into the Microsoft
            // identity platform, and the browser is redirected back to your app
            // with an authorization code in the query parameters
            var authorizationCode = "0.AUsAybUMk_LXWEmRZswjq5lSxoSL42KN5rdIvhKZjx3YwZsaAAA.AgABAAIAAAD--DLA3VO7QrddgJg7WevrAgDs_wUA9P_m6hZCZk59BBtcTQ_tZPyOBv72SgeDWmuPHrY_qtDA3C4u6CrcBvuhsn6rfmdCyOAqI6UIpget6rbnrTNvdnpW6tV70br56Pt9SdKrZSLAgOpHbN_b3Wsb-kSsYEWTCuDfkgJwYIRSnbuPNjCbYrWS_lwj_Xaye-qcL6U3jSQoTtuMGWYiYodNphcoDGBcQvkS1k3pgW0zgSQHw-n4DOxYCmLOWV6alO6GRK48p2Q2pblxtG6R8ZOp9CmDbBhXz3fyAPBAIa3Jt_QOx8cXW7rIT2lyRCGqVlqHw9lKCFp_E_LlSMFBOlkQRZxi2jWW2vxUp4pYM-j2seAwEq_DlpAU8T1O1IpP7P5xVLeBZsKu3G8Dei_4J6xj6dLDoW80EwhPAquEe2vX12wBU7PgnW4cjnm78_hGGPjCHxmnzZYK7KztiyNBR2JN8Z_TkudNLzNr-W_4ECXL4sfoTmwg4wPLwSKYsnYFnN3fwyfCR7y1h3pFZDKqIJzLzhyEqq6aixLxVCEquaBzXx4HbSl8_fmiyeij_l61Qzpd5U5fg2NspPormsWgpfolk8LprisH7BZSXx_JY0YpeyNb9pYQ8KnfC0hZ-1g1iRcR-LzfsXOryHZ6EqD3WhdVFLBiLKV25cQy-wKuKUUfa4nPrIjg74qzsdjTYQoLKs-UWUSX-YfKh1zNEMxQOow-d3hVrqh9S0Nllrdt5xbFlE-XlZ2AcPSLsuh7k4VFal2vZiQ4ZP8B46M59FcUpoYdFr9K0tGjE3f9-a8ap9CbSOxGYHnCpmjeswvYkdKYCRtaw5gISVo9WUQEBXaq6oGI4nvtgJ2-gCqDIKT1YUc6_t8s6NP03dtYUoWG0KP4aF7fLnBaG7TYYGI0X4xr4idrONy7rnEyF6LHK2PfuAn72NXLHy_z00nM3dTKcnGC9Q";

            // using Azure.Identity;
            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            // https://learn.microsoft.com/dotnet/api/azure.identity.authorizationcodecredential
            var authCodeCredential = new AuthorizationCodeCredential(
                tenantId, clientId, clientSecret, authorizationCode, options);

            var graphClient = new GraphServiceClient(authCodeCredential, scopes);
            //var presence = graphClient.Me.Presence;

            //var token = authCodeCredential.GetToken(new TokenRequestContext(scopes));
            var req = graphClient.Me.Presence.GetAsync();
            var pr = req.Result;
            return pr;

            //return await graphClient.Me.Presence.Request().GetAsync();
        }

        async Task<string> GetStatus(string authToken)
        {
            //string authToken = GetAuthorizationToken();
            //Console.WriteLine(authToken);
            //return "";
            /*
            string[] scopes = new string[] { "user.read" };
            //string accessToken = "eyJ0eXAiOiJKV1QiLCJub25jZSI6Imp3UjRHRnhzTnlCbUpCUHkwNXhMcHJsQmNQSzhWUmlFOFBUUk5nSnZpNnMiLCJhbGciOiJSUzI1NiIsIng1dCI6Ii1LSTNROW5OUjdiUm9meG1lWm9YcWJIWkdldyIsImtpZCI6Ii1LSTNROW5OUjdiUm9meG1lWm9YcWJIWkdldyJ9.eyJhdWQiOiIwMDAwMDAwMy0wMDAwLTAwMDAtYzAwMC0wMDAwMDAwMDAwMDAiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC8zNDE1ZjJmNy1mNWE4LTQwOTItYjUyYS0wMDNhYWY4NDQ4NTMvIiwiaWF0IjoxNjc2Mzk1NDMzLCJuYmYiOjE2NzYzOTU0MzMsImV4cCI6MTY3NjM5OTU3MiwiYWNjdCI6MCwiYWNyIjoiMSIsImFpbyI6IkFUUUF5LzhUQUFBQURuM2tKa09YUG5BTm41V3c5cDJqRWhRL1dNVGRKYnRJdTN4SHlFaVJ5NVF5NWJGSnhrOS9rWDBHMTYvT0lscjAiLCJhbXIiOlsicHdkIl0sImFwcF9kaXNwbGF5bmFtZSI6IkdyYXBoIEV4cGxvcmVyIiwiYXBwaWQiOiJkZThiYzhiNS1kOWY5LTQ4YjEtYThhZC1iNzQ4ZGE3MjUwNjQiLCJhcHBpZGFjciI6IjAiLCJkZXZpY2VpZCI6IjUzMDA2NDllLWJiNDYtNDE0NC04YTZmLWI3ZWJkNWQ0ZWMzMSIsImZhbWlseV9uYW1lIjoiQWxicmVjaHRhcyIsImdpdmVuX25hbWUiOiJTaW1vbmFzIiwiaWR0eXAiOiJ1c2VyIiwiaXBhZGRyIjoiNzguNjAuMTQ3LjE0OSIsIm5hbWUiOiJBbGJyZWNodGFzIFNpbW9uYXMiLCJvaWQiOiJjYTU1YThjYi0xOTM1LTQ5YzMtOGI1Mi1jN2QxMjYzYjZlZDUiLCJvbnByZW1fc2lkIjoiUy0xLTUtMjEtMTAwMzYxOTAyMS0yMjQ3MzY2MDY0LTE0MjE2MjQ3ODEtMjE4MDQ3IiwicGxhdGYiOiIzIiwicHVpZCI6IjEwMDMyMDAwREJGM0M5OEUiLCJyaCI6IjAuQVRzQTlfSVZOS2oxa2tDMUtnQTZyNFJJVXdNQUFBQUFBQUFBd0FBQUFBQUFBQUE3QU1ZLiIsInNjcCI6IkNhbGVuZGFycy5SZWFkIENhbGVuZGFycy5SZWFkV3JpdGUgb3BlbmlkIHByb2ZpbGUgVXNlci5SZWFkIGVtYWlsIFByZXNlbmNlLlJlYWQuQWxsIiwic3ViIjoiR3Z2d1FwbEFrSHRNMWZiRHMyeWYwOVZiaEJVdlhFNnItMHRSUHdZTHc5USIsInRlbmFudF9yZWdpb25fc2NvcGUiOiJFVSIsInRpZCI6IjM0MTVmMmY3LWY1YTgtNDA5Mi1iNTJhLTAwM2FhZjg0NDg1MyIsInVuaXF1ZV9uYW1lIjoic2ltYWxiQGt0dS5sdCIsInVwbiI6InNpbWFsYkBrdHUubHQiLCJ1dGkiOiJHRzlkMk9HYzRFQ0xzVFJfTVZZVkFBIiwidmVyIjoiMS4wIiwid2lkcyI6WyJiNzlmYmY0ZC0zZWY5LTQ2ODktODE0My03NmIxOTRlODU1MDkiXSwieG1zX3N0Ijp7InN1YiI6IkdQeEZhTk8tT2Y0NTcwWUhOazMxSVBUaUhFcHZUOHNwcFZPN3BqRFUwVDQifSwieG1zX3RjZHQiOjEzNjc1NDQwMTEsInhtc190ZGJyIjoiRVUifQ.EfIsgTvym2-KpVxabJ2n-dkChZYhDrLw1XxOga9R-9Bsp8um7KB5K2tN3Ryqb3CJCP4dRPpwEpiam-iiF8iCIXjOn5-x8J9E0lylc-E2nlv_IfQwNp0L_3JeMLtsFBfcAEDaj8YmFttt65Gugbor6k_kiSnUBwyhc6ZFxHjvFNzvN-VOTOSwhU2FjjqjDsyDT03tYFuWY2fjoVxLqYpp5tpB0kLHPjBka4hojw0locnrkLRK1RXEZBOlJOdXjr347q2QR6uO3UeOREdDHIW5o0uXUhY4Gm78t822n14ITWpvSrYMBg6pEo8MQX1T9Iec-KpNjEtX1QVpIokJaQCwOg";
            //string accessToken = "eyJ0eXAiOiJKV1QiLCJub25jZSI6Imp3UjRHRnhzTnlCbUpCUHkwNXhMcHJsQmNQSzhWUmlFOFBUUk5nSnZpNnMiLCJhbGciOiJSUzI1NiIsIng1dCI6Ii1LSTNROW5OUjdiUm9meG1lWm9YcWJIWkdldyIsImtpZCI6Ii1LSTNROW5OUjdiUm9meG1lWm9YcWJIWkdldyJ9.eyJhdWQiOiIwMDAwMDAwMy0wMDAwLTAwMDAtYzAwMC0wMDAwMDAwMDAwMDAiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC8zNDE1ZjJmNy1mNWE4LTQwOTItYjUyYS0wMDNhYWY4NDQ4NTMvIiwiaWF0IjoxNjc2Mzk1NDMzLCJuYmYiOjE2NzYzOTU0MzMsImV4cCI6MTY3NjM5OTU3MiwiYWNjdCI6MCwiYWNyIjoiMSIsImFpbyI6IkFUUUF5LzhUQUFBQURuM2tKa09YUG5BTm41V3c5cDJqRWhRL1dNVGRKYnRJdTN4SHlFaVJ5NVF5NWJGSnhrOS9rWDBHMTYvT0lscjAiLCJhbXIiOlsicHdkIl0sImFwcF9kaXNwbGF5bmFtZSI6IkdyYXBoIEV4cGxvcmVyIiwiYXBwaWQiOiJkZThiYzhiNS1kOWY5LTQ4YjEtYThhZC1iNzQ4ZGE3MjUwNjQiLCJhcHBpZGFjciI6IjAiLCJkZXZpY2VpZCI6IjUzMDA2NDllLWJiNDYtNDE0NC04YTZmLWI3ZWJkNWQ0ZWMzMSIsImZhbWlseV9uYW1lIjoiQWxicmVjaHRhcyIsImdpdmVuX25hbWUiOiJTaW1vbmFzIiwiaWR0eXAiOiJ1c2VyIiwiaXBhZGRyIjoiNzguNjAuMTQ3LjE0OSIsIm5hbWUiOiJBbGJyZWNodGFzIFNpbW9uYXMiLCJvaWQiOiJjYTU1YThjYi0xOTM1LTQ5YzMtOGI1Mi1jN2QxMjYzYjZlZDUiLCJvbnByZW1fc2lkIjoiUy0xLTUtMjEtMTAwMzYxOTAyMS0yMjQ3MzY2MDY0LTE0MjE2MjQ3ODEtMjE4MDQ3IiwicGxhdGYiOiIzIiwicHVpZCI6IjEwMDMyMDAwREJGM0M5OEUiLCJyaCI6IjAuQVRzQTlfSVZOS2oxa2tDMUtnQTZyNFJJVXdNQUFBQUFBQUFBd0FBQUFBQUFBQUE3QU1ZLiIsInNjcCI6IkNhbGVuZGFycy5SZWFkIENhbGVuZGFycy5SZWFkV3JpdGUgb3BlbmlkIHByb2ZpbGUgVXNlci5SZWFkIGVtYWlsIFByZXNlbmNlLlJlYWQuQWxsIiwic3ViIjoiR3Z2d1FwbEFrSHRNMWZiRHMyeWYwOVZiaEJVdlhFNnItMHRSUHdZTHc5USIsInRlbmFudF9yZWdpb25fc2NvcGUiOiJFVSIsInRpZCI6IjM0MTVmMmY3LWY1YTgtNDA5Mi1iNTJhLTAwM2FhZjg0NDg1MyIsInVuaXF1ZV9uYW1lIjoic2ltYWxiQGt0dS5sdCIsInVwbiI6InNpbWFsYkBrdHUubHQiLCJ1dGkiOiJHRzlkMk9HYzRFQ0xzVFJfTVZZVkFBIiwidmVyIjoiMS4wIiwid2lkcyI6WyJiNzlmYmY0ZC0zZWY5LTQ2ODktODE0My03NmIxOTRlODU1MDkiXSwieG1zX3N0Ijp7InN1YiI6IkdQeEZhTk8tT2Y0NTcwWUhOazMxSVBUaUhFcHZUOHNwcFZPN3BqRFUwVDQifSwieG1zX3RjZHQiOjEzNjc1NDQwMTEsInhtc190ZGJyIjoiRVUifQ.EfIsgTvym2-KpVxabJ2n-dkChZYhDrLw1XxOga9R-9Bsp8um7KB5K2tN3Ryqb3CJCP4dRPpwEpiam-iiF8iCIXjOn5-x8J9E0lylc-E2nlv_IfQwNp0L_3JeMLtsFBfcAEDaj8YmFttt65Gugbor6k_kiSnUBwyhc6ZFxHjvFNzvN-VOTOSwhU2FjjqjDsyDT03tYFuWY2fjoVxLqYpp5tpB0kLHPjBka4hojw0locnrkLRK1RXEZBOlJOdXjr347q2QR6uO3UeOREdDHIW5o0uXUhY4Gm78t822n14ITWpvSrYMBg6pEo8MQX1T9Iec-KpNjEtX1QVpIokJaQCwOg";
            //string accessToken = "eyJ0eXAiOiJKV1QiLCJub25jZSI6IlhrSEdka3E4RnZ6YmNRYzQ3dmNQUkNxY19Dck9oellaNi1HYW1fRGRSOWMiLCJhbGciOiJSUzI1NiIsIng1dCI6Ii1LSTNROW5OUjdiUm9meG1lWm9YcWJIWkdldyIsImtpZCI6Ii1LSTNROW5OUjdiUm9meG1lWm9YcWJIWkdldyJ9.eyJhdWQiOiIwMDAwMDAwMy0wMDAwLTAwMDAtYzAwMC0wMDAwMDAwMDAwMDAiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC8zNDE1ZjJmNy1mNWE4LTQwOTItYjUyYS0wMDNhYWY4NDQ4NTMvIiwiaWF0IjoxNjc2NDExNTQ1LCJuYmYiOjE2NzY0MTE1NDUsImV4cCI6MTY3NjQxNzEwMCwiYWNjdCI6MCwiYWNyIjoiMSIsImFpbyI6IkFUUUF5LzhUQUFBQURuM2tKa09YUG5BTm41V3c5cDJqRWhRL1dNVGRKYnRJdTN4SHlFaVJ5NVF5NWJGSnhrOS9rWDBHMTYvT0lscjAiLCJhbXIiOlsicHdkIl0sImFwcF9kaXNwbGF5bmFtZSI6IkdyYXBoIEV4cGxvcmVyIiwiYXBwaWQiOiJkZThiYzhiNS1kOWY5LTQ4YjEtYThhZC1iNzQ4ZGE3MjUwNjQiLCJhcHBpZGFjciI6IjAiLCJkZXZpY2VpZCI6IjUzMDA2NDllLWJiNDYtNDE0NC04YTZmLWI3ZWJkNWQ0ZWMzMSIsImZhbWlseV9uYW1lIjoiQWxicmVjaHRhcyIsImdpdmVuX25hbWUiOiJTaW1vbmFzIiwiaWR0eXAiOiJ1c2VyIiwiaXBhZGRyIjoiNzguNjAuMTQ3LjE0OSIsIm5hbWUiOiJBbGJyZWNodGFzIFNpbW9uYXMiLCJvaWQiOiJjYTU1YThjYi0xOTM1LTQ5YzMtOGI1Mi1jN2QxMjYzYjZlZDUiLCJvbnByZW1fc2lkIjoiUy0xLTUtMjEtMTAwMzYxOTAyMS0yMjQ3MzY2MDY0LTE0MjE2MjQ3ODEtMjE4MDQ3IiwicGxhdGYiOiIzIiwicHVpZCI6IjEwMDMyMDAwREJGM0M5OEUiLCJyaCI6IjAuQVRzQTlfSVZOS2oxa2tDMUtnQTZyNFJJVXdNQUFBQUFBQUFBd0FBQUFBQUFBQUE3QU1ZLiIsInNjcCI6IkNhbGVuZGFycy5SZWFkIENhbGVuZGFycy5SZWFkV3JpdGUgb3BlbmlkIFByZXNlbmNlLlJlYWQgUHJlc2VuY2UuUmVhZC5BbGwgcHJvZmlsZSBVc2VyLlJlYWQgZW1haWwiLCJzdWIiOiJHdnZ3UXBsQWtIdE0xZmJEczJ5ZjA5VmJoQlV2WEU2ci0wdFJQd1lMdzlRIiwidGVuYW50X3JlZ2lvbl9zY29wZSI6IkVVIiwidGlkIjoiMzQxNWYyZjctZjVhOC00MDkyLWI1MmEtMDAzYWFmODQ0ODUzIiwidW5pcXVlX25hbWUiOiJzaW1hbGJAa3R1Lmx0IiwidXBuIjoic2ltYWxiQGt0dS5sdCIsInV0aSI6IjRJZ0F4SUt6N1VLd1VvbHpMQ01kQUEiLCJ2ZXIiOiIxLjAiLCJ3aWRzIjpbImI3OWZiZjRkLTNlZjktNDY4OS04MTQzLTc2YjE5NGU4NTUwOSJdLCJ4bXNfc3QiOnsic3ViIjoiR1B4RmFOTy1PZjQ1NzBZSE5rMzFJUFRpSEVwdlQ4c3BwVk83cGpEVTBUNCJ9LCJ4bXNfdGNkdCI6MTM2NzU0NDAxMSwieG1zX3RkYnIiOiJFVSJ9.L4eDFIfUFpHeLrOaUVafZBqNZw2VyLCOSeFXKyDJlu398fn-tdaCVH74fJMFgKjwzbOlGHhPNi0Wbr9UCdWKKfYM7aanF9B9j_JiG5mSdHF-4xaoByjG_WkN5oB-PyIDcsDDnN-FoMfJ3fLrY5fGAoqqwCw-OIUJsisFaoHpmfk35PksTpiPpebmNTQr6mwawhqon5LqYsgFuMOiVZJzr8_oLGIEUiZ_R7ftQiymlCR1LIs9Kdr24oQKfUe6SwM-eWLqFdzzWt891Du_U0rfKGYxnz96SUb8n9nIIUEdMZPM9VJdFgBRBGar_GqBYeg8vhU5cHpDrj7SbNlZhBsDoQ";
            // Use the access token to call a protected web API.

            string uri = "https://login.microsoftonline.com/930cb5c9-d7f2-4958-9166-cc23ab9952c6/oauth2/v2.0/authorize?client_id=62e38b84-e68d-48b7-be12-998f1dd8c19b&response_type=code&redirect_uri=http%3A%2F%2Flocalhost:8088&response_mode=query&scope=offline_access%20user.read%20mail.read&state=12345";

            var authenticator = new Authenticator();
            string code = await authenticator.GetAccessToken(uri);
            //string[] scopes = new string[] { "user.read" };
            */
            HttpClient client = new HttpClient();
            //var serviceClient = GetServiceClient(code);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            //client.DefaultRequestHeaders.
            string url = "https://graph.microsoft.com/v1.0/me/presence";
            return await client.GetStringAsync(url);
        }

        //char a = ' ';

        //while (a == ' ')
        //{
        //    string status = await GetStatus(token);
        //    // Newtonsoft json deserialize
        //    var userPresence = JsonConvert.DeserializeObject<UserPresence>(status);

        //    Console.WriteLine(userPresence.activity);
        //    a = Console.ReadLine()[0];
        //}
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
