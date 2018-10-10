using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Device.Location;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace WindowsServiceTrackingLocation
{
    public partial class WindowServiceTrackingLocation : ServiceBase
    {
        private Timer timerSendingMail = null;
        private Timer timerCheckingInternetConnection = null;
        private Utilities utils = new Utilities();
        private bool isCallingAPI = false;
        private bool lastStateConnection = false;
        private const int INTERVAL_TIMER_INTERNET_CONNECTION = 2 * 60 * 1000;
        private const int INTERVAL_TIMER_SENDING_MAIL = 15 * 60 * 1000;

        public WindowServiceTrackingLocation()
        {
            InitializeComponent();
            //OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                utils.Log("----------------Start------------------");
                GetLocation();
                SetUpTimerMailProcess();
                SetUpTimerCheckInternetConnection();
            }
            catch (Exception e)
            {
                utils.Log("Exception occurs: " + e.ToString());
            }
        }

        protected override void OnStop()
        {
            try
            {
                utils.Log("-------------------Stop---------------");
                utils.SendingMailUsingMailKit();
                ReleaseResource();
            }
            catch (Exception e)
            {
                utils.Log("Exception occurs when STOP: " + e.ToString());
            }

        }

        void ReleaseResource()
        {
            timerSendingMail.Stop();
            timerSendingMail.Dispose();
            timerCheckingInternetConnection.Stop();
            timerCheckingInternetConnection.Dispose();
            this.Dispose();
            this.Stop();
        }

        void SetUpTimerCheckInternetConnection()
        {
            timerCheckingInternetConnection = new Timer(INTERVAL_TIMER_INTERNET_CONNECTION);
            timerCheckingInternetConnection.Elapsed += OnTimerElapsedCheckingInternetConnection;
            timerCheckingInternetConnection.Enabled = true;
        }

        void OnTimerElapsedCheckingInternetConnection(object sender, ElapsedEventArgs e)
        {
            if (!lastStateConnection)
            {
                bool result = CheckInternetConnection();
                if (result)
                {
                    lastStateConnection = result;
                    utils.SendingMailUsingMailKit();
                }
            }
            else
            {
                lastStateConnection = CheckInternetConnection();
            }
        }

        void SetUpTimerMailProcess()
        {
            timerSendingMail = new Timer()
            {
                Interval = INTERVAL_TIMER_SENDING_MAIL,
                Enabled = true
            };
            timerSendingMail.Elapsed += OnTimerElapsedSendingMail;
        }

        void OnTimerElapsedSendingMail(object sender, ElapsedEventArgs e)
        {
            isCallingAPI = true;
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
            ParseLocationAsync(e.Position.Location);
        }

        void ParseLocationAsync(GeoCoordinate coordinate)
        {
            if (coordinate.IsUnknown == false)
            {
                utils.Log($"Latitude, Longtitude: {coordinate.Latitude}, {coordinate.Longitude}");
                if (isCallingAPI)
                {
                    isCallingAPI = false;
                    GetAllEventDataAsync(coordinate.Latitude, coordinate.Longitude);
                }
            }
        }

        async void GetAllEventDataAsync(double Lati, double Longti) //Get All Events Records  
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

                var result = JObject.Parse(responseStr);
                utils.LogAddress(result["address"].ToString(), result["address-components"].ToString());
            }
        }

        public bool CheckInternetConnection()
        {
            try
            {
                Ping myPing = new Ping();
                string host = "google.com";
                byte[] buffer = new byte[32];
                int timeout = 1000;
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                if (reply.Status == IPStatus.Success)
                    return true;
            }
            catch (Exception e)
            {
                utils.Log("Error when connect internet. " + e.ToString());
                return false;
            }
            return false;
        }
    }
}
