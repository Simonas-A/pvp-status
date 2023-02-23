using Azure.Core;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Graph.Constants;

namespace pvp_status
{
    internal class Authenticator
    {
        public Authenticator() { }

        public async Task<string> GetAccessToken(string url)
        {
            /*
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string error = string.Format("Status {0}: {1}", response.StatusCode.ToString(), response.ReasonPhrase.ToString());
                Console.WriteLine(error);
                return null;
            }

            var data = await response.Content.ReadAsStringAsync();
            return data;
            */
            OpenUrl(url);
            return CreateListener();
            //var client = new HttpClient();
            //return await client.GetStringAsync(url);
            //Authenticate(url);
            //return "";

        }

        private string CreateListener()
        {
            using var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8088/");
            listener.AuthenticationSchemes = AuthenticationSchemes.Negotiate;

            listener.Start();

            var context = listener.GetContext();
            HttpListenerRequest request = context.Request;
            return request.RawUrl.Substring(7);
        }

        private async Task<string> CreateTokenListener()
        {
            var client = new HttpClient();
            //var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            var response = await client.GetAsync("http://localhost:8088");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode.ToString());
                return "";
            }

            return await response.Content.ReadAsStringAsync();
        }

        private void OpenUrl(string url)
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            //Process.Start(url.ToString());
        }

        private void Authenticate(string url)
        {
            //var client = new HttpClient();
            //return await client.GetStringAsync(url);
        }
    }
}
