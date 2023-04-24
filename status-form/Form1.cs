using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Threading;
using Microsoft.Graph;
using Azure.Core;
using Microsoft.Graph.Models;
using System.IdentityModel;
using Azure.Identity;

namespace status_form
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string Token;
        private string Status;

        private void Form1_Load(object sender, EventArgs e)
        {
            Token = System.IO.File.ReadAllText("../../token.txt");
        }

        class UserPresence
        {
            public UserPresence(string odatacontext, string id, string availability, string activity)
            {
                this.Odatacontext = odatacontext;
                this.Id = id;
                this.Availability = availability;
                this.Activity = activity;
            }

            public string Odatacontext { get; set; }
            public string Id { get; set; }
            public string Availability { get; set; }
            public string Activity { get; set; }
        }

        public class MinimalCalendar
        {
            public MinimalCalendar(string id, string subject, CalendarTime start, CalendarTime end)
            {
                this.Id = id;
                this.Subject = subject;
                this.Start = start;
                this.End = end;
            }

            public string Id { get; set; }
            public string Subject { get; set; }
            public CalendarTime Start { get; set; }
            public CalendarTime End { get; set; }
        }

        public class CalendarResponse
        {
            public CalendarResponse(MinimalCalendar[] value)
            {
                Value = value;
            }

            
            public MinimalCalendar[] Value { get; set; }
        }

        public class CalendarTime
        {
            public CalendarTime(string dateTime, string timeZone)
            {
                DateTime = dateTime;
                TimeZone = timeZone;
            }

            public string DateTime { get; set; }
            public string TimeZone { get; set; }
            
            
        }

        private async void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private async Task<string> GetUserPresenceAsync()
        {
            string url = "https://graph.microsoft.com/v1.0/me/presence";
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + Token);
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<Presence>(await response.Content.ReadAsStringAsync()).Availability;
            else
                return response.StatusCode.ToString();
        }

        private async Task<CalendarResponse> GetCalendarAsyncMock()
        {
            string resp = System.IO.File.ReadAllText("../../Response.json");
            return JsonConvert.DeserializeObject<CalendarResponse>(resp);
        }

        private async Task<CalendarResponse> GetCalendarAsync()
        {
            string url = "https://graph.microsoft.com/v1.0/me/calendarview?startdatetime=2023-04-24T20:48:00.115Z&enddatetime=2023-05-06T20:48:00.116Z";
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + Token);
            var response = await httpClient.GetAsync(url);
            string resp = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CalendarResponse>(resp);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string presence = await GetUserPresenceAsync();
            label1.Text = presence;
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            CalendarResponse calendar = await GetCalendarAsyncMock();
            label1.Text = "";

            foreach (var item in calendar.Value)
            {
                label1.Text += $"{item.Subject} {item.Start.DateTime}\n";
            }

        }
    }
}
