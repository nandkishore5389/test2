using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Timers;

namespace ConsoleApp16
{
    class Program
    {
        public static List<string> expiry5min;
        public static List<string> expiry1hour;
        static System.Timers.Timer timer5min;
        static System.Timers.Timer timer1hour;
        public static HttpClient client = new HttpClient();
        static void Main(string[] args)
        {
            string expiry5minString = ConfigurationManager.AppSettings["expiry5min"].ToString();
            expiry5min = expiry5minString.Split(',').ToList();

            string expiry1hourString = ConfigurationManager.AppSettings["expiry1hour"].ToString();
            expiry1hour = expiry1hourString.Split(',').ToList();

            OIData oi = new OIData();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            int expiry5minTime = Convert.ToInt32(ConfigurationManager.AppSettings["expiry5minTime"]);
            timer5min = new System.Timers.Timer
            {
                Interval = 1000 * expiry5minTime * 60
            };
            timer5min.Elapsed += oi.OnTimer5MinEvent;
            timer5min.AutoReset = true;
            timer5min.Enabled = true;
            oi.OnTimer5MinEvent(null, null);

            OIData oi2 = new OIData();
            int expiry1hourTIme = Convert.ToInt32(ConfigurationManager.AppSettings["expiry1hourTIme"]);
            timer1hour = new System.Timers.Timer
            {
                Interval = 1000 * expiry1hourTIme * 60
            };
            timer1hour.Elapsed += oi2.OnTimer1HourEvent;
            timer1hour.AutoReset = true;
            timer1hour.Enabled = true;
            oi2.OnTimer1HourEvent(null, null);

            Console.ReadKey();

        }

        private static void OnTimer5MinEvent(object sender, ElapsedEventArgs e)
        {
            OIData oi = new OIData();
            foreach (var expiry in Program.expiry5min)
            {
                try
                {


                    DateTime expiryDate = DateTime.Parse(expiry);

                    // oi.OnTimer5MinEvent(expiryDate, expiry, new HttpClient());

                    // HttpClient client = new HttpClient();

                    // client.Timeout = TimeSpan.FromSeconds(30);
                    // System.Net.ServicePointManager.DefaultConnectionLimit = 100;

                    //Task<HttpResponseMessage> response = client.GetAsync("https://www1.nseindia.com/live_market/dynaContent/live_watch/option_chain/optionKeys.jsp?segmentLink=17&instrument=OPTIDX&symbol=NIFTY&date=" + expiry.Trim());
                    //if (response.Result != null)
                    //{
                    //    var htmlString = response.Result.Content.ReadAsStringAsync();
                    //    htmlString.Wait();
                    //    var s = htmlString.Result;
                    //    //ParseHTMLdata(htmlString.Result, expiryDate);
                    //}
                    Task<string> vs = client.GetStringAsync("https://www1.nseindia.com/live_market/dynaContent/live_watch/option_chain/optionKeys.jsp?segmentLink=17&instrument=OPTIDX&symbol=NIFTY&date=" + expiry.Trim());
                    if (vs.Result != null)
                    {
                        var htmlString = vs.Result;
                        vs.Wait();
                        var s = htmlString;
                        Console.WriteLine("done: " + DateTime.Now.ToShortTimeString());
                        //ParseHTMLdata(htmlString.Result, expiryDate);
                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine("Error: " + DateTime.Now.ToShortTimeString() + " \n" + ex);
                }
            }
        }

        private static void OnTimer1HourEvent(object sender, ElapsedEventArgs e)
        {

        }
    }
}
