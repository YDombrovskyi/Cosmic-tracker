﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CosmicTracker.Models;
using Microsoft.CodeAnalysis;


namespace CosmicTracker.Controllers
{
    public class HomeController : Controller
    {
       private double lat1;
       private double lat2;
       private double lon1;
       private double lon2;
        private DateTime time1;
        private DateTime time2;
        List<string> datas = new List<string>();

        public IActionResult Index()
        {
            Location();
           var dis = Distance();
            var Speedresult = Math.Round(dis/(TimeConverter()/3600),1); 
            var information = new List<string> {
                Convert.ToString(lat1),
                Convert.ToString(lon1),
                Convert.ToString(lat2),
                Convert.ToString(lon2),
                Math.Round(dis,3).ToString(),
                Speedresult.ToString()
               // datas[6]//crew1
               // datas[7],//crew2
               // datas[8]//crew3
            };
          
            return View(information);
        }
        //  get Location 2 times

        public async void Location()
        {
            HttpClient http = new HttpClient();
           
            try
            {
                http.BaseAddress = new Uri("http://api.open-notify.org/iss-now.json");
                http.Timeout = TimeSpan.FromSeconds(15);
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
           // datas.Add(dat2.crew.ToString());
            
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
           var timeForMeasurm = (point2 - point1).TotalSeconds;
           return timeForMeasurm;
        }
        //Converter unix Timestamp to Data
        public static DateTime UnixTimeStampToDateTime(string timeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
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
