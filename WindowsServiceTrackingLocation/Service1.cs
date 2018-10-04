using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Device.Location;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace WindowsServiceTrackingLocation
{
    public partial class WindowServiceTrackingLocation : ServiceBase
    {
        private Timer timer = null;
        private Utilities utils = new Utilities();

        public WindowServiceTrackingLocation()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            utils.Log("----------------Start------------------");
            GetLocation();
            SetUpTimerMailProcess();
        }

        protected override void OnStop()
        {
            utils.Log("-------------------Stop---------------");
            utils.SendingMailUsingMailKit();
            timer.Stop();
            timer.Dispose();
        }

        void SetUpTimerMailProcess()
        {
            timer = new Timer()
            {
                Interval = 5 * 60 * 1000,
                Enabled = true
            };
            timer.Elapsed += OnTimerElapsed;
        }

        void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            utils.SendingMailUsingMailKit();
        }

        void GetLocation()
        {
            GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();
            watcher.PositionChanged += PositionChangedMethod;
            watcher.TryStart(false, TimeSpan.FromMilliseconds(1000));
        }

        void PositionChangedMethod(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            ParseLocationAsync(e.Position.Location, false);
        }

        void ParseLocationAsync(GeoCoordinate coordinate, bool isCallingAPI)
        {
            if (coordinate.IsUnknown == false)
            {
                utils.Log($"Latitude {coordinate.Latitude}, Longtitude {coordinate.Longitude}");
                if (isCallingAPI)
                {
                    GetAllEventDataAsync(coordinate.Latitude, coordinate.Longitude);
                }
            }
        }

        async Task GetAllEventDataAsync(double Lati, double Longti) //Get All Events Records  
        {
            using (var client = new HttpClient())
            {
                Console.OutputEncoding = Encoding.UTF8;
                var req = new List<KeyValuePair<string, string>>();
                req.Add(new KeyValuePair<string, string>("user-id", "dinhphu951998"));
                req.Add(new KeyValuePair<string, string>("api-key", "YqBLQSnxcQiiNKrflBbAIbp9ROa7VlmgpCvxHedZJyesRj2v"));
                req.Add(new KeyValuePair<string, string>("latitude", Lati.ToString()));
                req.Add(new KeyValuePair<string, string>("longitude", Longti.ToString()));
                req.Add(new KeyValuePair<string, string>("language-code", "en"));

                var content = new FormUrlEncodedContent(req);
                var response = await client.PostAsync("https://neutrinoapi.com/geocode-reverse", content);
                var responseStr = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(Lati.ToString() + " " + Longti);
                //Console.WriteLine(responseStr);

                var result = JsonConvert.DeserializeObject<dynamic>(responseStr);
                utils.LogAddress(result["address"], result["address-components"]);
                //Console.WriteLine();
                //Console.WriteLine("{0}", result["country"]);
                //Console.WriteLine("{0}", result["country-code"]);
                //Console.WriteLine("{0}", result["address"]);
                //Console.WriteLine("{0}", result["city"]);
            }
        }

    }
}
