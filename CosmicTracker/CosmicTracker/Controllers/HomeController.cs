using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using CosmicTracker.Models;


namespace CosmicTracker.Controllers
{
    public class HomeController : Controller
    {
        private double lat1;
        private double lat2;
        private double lon1;
        private double lon2;
        List<string> datas = new List<string>();
        private string[] Peoples;
        public List<string> Team = new List<string>();
        public IActionResult Index()
        {
            Location();
            var dis = Distance();
            var Speedresult = Math.Round(dis / (TimeConverter() / 3600), 1);
            var information = new List<string> {
                Convert.ToString(datas[0]),
                Convert.ToString(datas[1]),
                Convert.ToString(datas[3]),
                Convert.ToString(datas[4]),
                Math.Round(dis,3).ToString(),
                Speedresult.ToString(),
                datas[6]

            };

            return View(information);
        }
        //  get Location 2 times
        public async void SpaceTeam()
        {
            HttpClient http = new HttpClient();
            try
            {
                http.BaseAddress = new Uri("http://api.open-notify.org/astros.json");
                http.Timeout = TimeSpan.FromSeconds(10);
            }
            catch (Exception)
            {
                RedirectToAction("Error", "Home");
            }
            var data = http.GetAsync("http://api.open-notify.org/astros.json").Result.Content.ReadAsStringAsync().Result;
            SiteResponse SpaceTm = Newtonsoft.Json.JsonConvert.DeserializeObject<SiteResponse>(data);
            if (SpaceTm.message != "success")
            {
                RedirectToAction("Error", "Home");
            }
            else
            {
                datas.Add(SpaceTm.crew.number);

                foreach (var crewMember in SpaceTm.crew.People)
                {
                    Team.Add(crewMember);
                }
            }
        }
        public void Location()
        {
            HttpClient http = new HttpClient();

            try
            {
                http.BaseAddress = new Uri("http://api.open-notify.org/iss-now.json");
                http.Timeout = TimeSpan.FromSeconds(10);
            }
            catch (Exception)
            {
                RedirectToAction("Error", "Home");
            }
            var data = http.GetAsync("http://api.open-notify.org/iss-now.json").Result.Content.ReadAsStringAsync().Result;
            SiteResponse dat = Newtonsoft.Json.JsonConvert.DeserializeObject<SiteResponse>(data);
            datas.Add(dat.iss_position.latitude);
            datas.Add(dat.iss_position.longitude);
            datas.Add(dat.timestamp.ToString());
            Thread.Sleep(10000);

            var data2 = http.GetAsync("http://api.open-notify.org/iss-now.json").Result.Content.ReadAsStringAsync().Result;
            SiteResponse dat2 = Newtonsoft.Json.JsonConvert.DeserializeObject<SiteResponse>(data2);
            datas.Add(dat2.iss_position.latitude);
            datas.Add(dat2.iss_position.longitude);
            datas.Add(dat2.timestamp.ToString());


        }

        //Main function to calculate distance   
        public double Distance()
        {
            lat1 = double.Parse(datas[0], CultureInfo.InvariantCulture);
            lon1 = double.Parse(datas[1], CultureInfo.InvariantCulture);
            lat2 = double.Parse(datas[3], CultureInfo.InvariantCulture);
            lon2 = double.Parse(datas[4], CultureInfo.InvariantCulture);

            var d1 = lat1 * (Math.PI / 180.0);
            var num1 = lon1 * (Math.PI / 180.0);
            var d2 = lat2 * (Math.PI / 180.0);
            var num2 = lon2 * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
            var dist = 6371 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
            return dist;
        }

        public double TimeConverter()
        {

            var point1 = UnixTimeStampToDateTime(datas[2]);
            var point2 = UnixTimeStampToDateTime(datas[5]);
            string format = "HH:mm  ddd d MMM yyyy";
            datas.Add(point2.ToString(format));
            var timeForMeasurm = (point2 - point1).TotalSeconds;
            return timeForMeasurm;
        }
        //Converter unix Timestamp to Data
        public static DateTime UnixTimeStampToDateTime(string timeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(Convert.ToDouble(timeStamp)).ToLocalTime();
            return dtDateTime;
        }

        //if errors appear

        [ResponseCache(Duration = 15, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

}
