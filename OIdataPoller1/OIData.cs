using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ConsoleApp16
{
    public class OIData
    {
        public void OnTimer5MinEvent(object sender, ElapsedEventArgs e)
        {
            try
            {
                foreach (string expiry in Program.expiry5min)
                {
                    DateTime expiryDate = DateTime.Parse(expiry);
                    Console.WriteLine("OnTimer5MinEvent " + "https://www1.nseindia.com/live_market/dynaContent/live_watch/option_chain/optionKeys.jsp?segmentLink=17&instrument=OPTIDX&symbol=NIFTY&date=" + expiry.Trim());

                    var response = Program.client.GetAsync("https://www1.nseindia.com/live_market/dynaContent/live_watch/option_chain/optionKeys.jsp?segmentLink=17&instrument=OPTIDX&symbol=NIFTY&date=" + expiry.Trim());

                    if (response.Result != null)
                    {
                        var htmlString = response.Result.Content.ReadAsStringAsync();
                        htmlString.Wait();
                        ParseHTMLdata(htmlString.Result, expiryDate);
                    }
                    else
                    {
                        Console.WriteLine("OnTimer5MinEvent Internal server Error");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("OnTimer5MinEvent Error: " + ex);
            }
        }

        public void OnTimer1HourEvent(object sender, EventArgs e)
        {
            try
            {
                foreach (string expiry in Program.expiry1hour)
                {
                    DateTime expiryDate = DateTime.Parse(expiry);
                    Console.WriteLine("OnTimer1HourEvent " + "https://www1.nseindia.com/live_market/dynaContent/live_watch/option_chain/optionKeys.jsp?segmentLink=17&instrument=OPTIDX&symbol=NIFTY&date=" + expiry.Trim());

                    var response = Program.client.GetAsync("https://www1.nseindia.com/live_market/dynaContent/live_watch/option_chain/optionKeys.jsp?segmentLink=17&instrument=OPTIDX&symbol=NIFTY&date=" + expiry.Trim());

                    if (response.Result != null)
                    {
                        var htmlString = response.Result.Content.ReadAsStringAsync();
                        htmlString.Wait();
                        ParseHTMLdata(htmlString.Result, expiryDate);
                    }
                    else
                    {
                        Console.WriteLine("OnTimer1HourEvent Internal server Error");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("OnTimer1HourEvent Error: " + ex);
            }
        }

        void ParseHTMLdata(string htmlString, DateTime expiry)
        {
            try
            {

                Dictionary<int, string> dictCols = new Dictionary<int, string>();
                dictCols.Add(11, "strickPrice");
                dictCols.Add(1, "call_OI");
                dictCols.Add(2, "call_COI");
                dictCols.Add(3, "call_Volume");
                dictCols.Add(4, "call_IV");
                dictCols.Add(5, "call_LTP");

                dictCols.Add(21, "put_OI");
                dictCols.Add(20, "put_COI");
                dictCols.Add(19, "put_Volume");
                dictCols.Add(18, "put_IV");
                dictCols.Add(17, "put_LTP");


                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(htmlString);

                var nodes = doc.DocumentNode.SelectNodes("//table/tr");
                DataTable OIdata = new DataTable("OIData");

                var headers = nodes[0]
                    .Elements("th")
                    .Select(th => th.InnerHtml.Trim());

                DataColumn currentDatatime = new DataColumn();
                currentDatatime.DataType = Type.GetType("System.DateTime");
                currentDatatime.ColumnName = "datetime";
                OIdata.Columns.Add(currentDatatime);

                DataColumn expiryDatatime = new DataColumn();
                expiryDatatime.DataType = Type.GetType("System.DateTime");
                expiryDatatime.ColumnName = "expiry";
                OIdata.Columns.Add(expiryDatatime);

                DataColumn nifty = new DataColumn();
                nifty.DataType = Type.GetType("System.Double");
                nifty.ColumnName = "nifty";
                OIdata.Columns.Add(nifty);

                foreach (var item in dictCols)
                {
                    DataColumn col = new DataColumn();
                    col.DataType = Type.GetType("System.Decimal");
                    col.ColumnName = item.Value;
                    OIdata.Columns.Add(col);
                }



                var header1 = nodes[0].InnerText.Trim();
                var niftyString = header1.Substring(header1.IndexOf("NIFTY") + 6, 5).Trim();
                decimal niftyValue = 0;
                decimal.TryParse(niftyString, out niftyValue);

                var rows = nodes.Skip(1).Select(tr => tr
                .Elements("td")
                .Select(td => td.InnerText.Trim())
                .ToArray());


                foreach (var row in rows)
                {
                    if (row != null && row.Count() == 23)
                    {
                        if (Math.Abs(GetRowData(row[11])) % 100 == 0 && Math.Abs(GetRowData(row[11]) - niftyValue) < 700) // is multiple of 100 && strick price is 500 point away from nifty.
                        {
                            try
                            {
                                DataRow OIDataRow = OIdata.NewRow();
                                OIDataRow["datetime"] = DateTime.Now;
                                OIDataRow["expiry"] = expiry;
                                OIDataRow["nifty"] = niftyValue;
                                foreach (var item in dictCols)
                                {
                                    OIDataRow[item.Value] = GetRowData(row[item.Key]);
                                }

                                OIdata.Rows.Add(OIDataRow);
                            }
                            catch (Exception)
                            {


                            }

                        }

                    }

                }

                SaveDataInDB(OIdata);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void SaveDataInDB(DataTable oIdata)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(@"Data Source=PU8L8PG6QQ2\SQLEXPRESS;Initial Catalog=db_Test;Persist Security Info=True;User ID=saa;Password=saa"))
                {
                    conn.Open();

                    SqlCommand sqlCommandCount1 = new SqlCommand("select count(*) from [db_Test].[dbo].[OptionData]", conn);
                    long startCount = Convert.ToInt64(sqlCommandCount1.ExecuteScalar());

                    using (SqlBulkCopy bulk = new SqlBulkCopy(conn))
                    {
                        bulk.DestinationTableName = "[db_Test].[dbo].[OptionData]";

                        try
                        {
                            bulk.WriteToServer(oIdata);
                            SqlCommand sqlCommandCount2 = new SqlCommand("select count(*) from [db_Test].[dbo].[OptionData]", conn);
                            long endtCount = Convert.ToInt64(sqlCommandCount2.ExecuteScalar());

                            Console.WriteLine(DateTime.Now.ToShortTimeString() + ": original row count:" + startCount + ", update count:" + endtCount);

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("SaveDataInDB Error: " + ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public decimal GetRowData(string v)
        {
            decimal d = 0;
            try
            {
                decimal.TryParse(v, out d);
            }
            catch (Exception)
            {
                return 0;
            }

            return d;
        }
    }
}
