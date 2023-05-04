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
using System.Net;
using System.IO.Ports;

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
        private CalendarResponse Calendar = null;
        private MinimalCalendar CurrentEvent = null;
        private bool eventStarted = false;

        SerialPort port;


        private void Form1_Load(object sender, EventArgs e)
        {
            Token = System.IO.File.ReadAllText("../../token.txt");

            if (port == null)
            {
                //Change the portname according to your computer
                port = new SerialPort("COM6", 9600);
                port.Open();
            }
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
            public CalendarTime(DateTime dateTime, string timeZone)
            {
                DateTime = dateTime;
                TimeZone = timeZone;
            }

            public DateTime DateTime { get; set; }
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

        private async Task<string> GetUserPresenceAsyncMock(string resp)
        {
            //string url = "https://graph.microsoft.com/v1.0/me/presence";
            //HttpClient httpClient = new HttpClient();
            //httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + Token);
            string response = System.IO.File.ReadAllText($"../../ResponseSamples/{resp}");
            return JsonConvert.DeserializeObject<Presence>(response).Availability;
        }

        private async Task<CalendarResponse> GetCalendarAsyncMock()
        {
            string resp = System.IO.File.ReadAllText("../../Response.json");
            return JsonConvert.DeserializeObject<CalendarResponse>(resp);
        }

        private async Task<CalendarResponse> GetCalendarAsyncMockDateNow()
        {
            string resp = System.IO.File.ReadAllText("../../Response2.json");
            CalendarResponse response = JsonConvert.DeserializeObject<CalendarResponse>(resp);
            response.Value[0].Start.DateTime = DateTime.Now.AddSeconds(10);
            response.Value[0].End.DateTime = DateTime.Now.AddSeconds(30);
            return response;
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

        private async Task<HttpStatusCode> SetPresence()
        {
            string url = "https://graph.microsoft.com/v1.0/me/presence/setPresence";
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + Token);
            HttpContent content = new StringContent(JsonConvert.SerializeObject(new UserPresence("https://graph.microsoft.com/v1.0/$metadata#users('a%40b.com')/presence", "1", "Busy", "Busy")));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await httpClient.PostAsync(url, content);
            return response.StatusCode;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string presence = await GetUserPresenceAsync();
            label1.Text = presence;
            if (presence == "Available")
            {
                PortWrite("3");
            }
            else if (presence == "Busy")
            {
                PortWrite("1");
            }
            else if (presence == "Away")
            {
                PortWrite("2");
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            //CalendarResponse calendar = await GetCalendarAsyncMock();
            Calendar = await GetCalendarAsyncMockDateNow();

            label1.Text = "";

            foreach (var item in Calendar.Value)
            {
                label1.Text += $"{item.Subject} {item.Start.DateTime}\n";
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int totalSeconds = (int)(CurrentEvent.End.DateTime - CurrentEvent.Start.DateTime).TotalSeconds;
            int secondsLeft = (int)(CurrentEvent.End.DateTime - DateTime.Now).TotalSeconds;

            int secondsToStart = (int)(CurrentEvent.Start.DateTime - DateTime.Now).TotalSeconds;
            if (secondsToStart > 0)
            {
                label1.Text = $"Event \"{CurrentEvent.Subject}\" will start in {secondsToStart} seconds\nStatus: online";
            }
            else if (secondsLeft < 0)
            {
                timer1.Stop();
                label1.Text = $"Event \"{CurrentEvent.Subject}\" ended \nStatus: online";
                progressBar1.Value = 0;
                eventStarted = false;
                return;
            }
            else
            {
                label1.Text = $"Active event: {CurrentEvent.Subject} {secondsLeft}/{totalSeconds}\nStatus: busy";
                progressBar1.Value = (int)((float)secondsLeft / totalSeconds * 100);

                if (!eventStarted)
                {
                    eventStarted = true;
                    PortWrite($"1;z{totalSeconds};");
                    //PortWrite(totalSeconds.ToString());
                    //PortWrite(";");
                }
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Calendar == null)
            {
                MessageBox.Show("No calendar data");
                return;
            }

            CurrentEvent = Calendar.Value.OrderBy(x => x.Start.DateTime).First();

            //DateTime now = DateTime.Now;
            //foreach (var item in Calendar.Value)
            //{
            //    if (item.Start.DateTime <= now && item.End.DateTime >= now)
            //    {
            //        label1.Text = "Active event: " + item.Subject;
            //        CurrentEvent = item;
            //        break;
            //    }
            //}

            timer1.Start();
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            HttpStatusCode status = await SetPresence();
            label1.Text = status.ToString();
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            string presence = await GetUserPresenceAsyncMock("AvailableResponse.json");
            label1.Text = presence;
            DrawPresence(Color.Green);
            PortWrite("3");

        }

        private void DrawPresence(Color color)
        {
            Bitmap bmp = new Bitmap(150, 150);
            SolidBrush brush = new SolidBrush(color);
            Graphics g = Graphics.FromImage(bmp);
            g.FillEllipse(brush, 0, 0, 100, 100);
            pictureBox1.Image = bmp;
        }

        private async void button6_Click(object sender, EventArgs e)
        {
            string presence = await GetUserPresenceAsyncMock("BusyResponse.json");
            label1.Text = presence;
            DrawPresence(Color.Red);
            PortWrite("1");
        }

        private async void button7_Click(object sender, EventArgs e)
        {
            string presence = await GetUserPresenceAsyncMock("AwayResponse.json");
            label1.Text = presence;
            DrawPresence(Color.Yellow);
            PortWrite("2");
        }

        private void PortWrite(string message)
        {
            if (port != null && port.IsOpen)
            {
                port.Write(message);
            }
        }
    }
}
