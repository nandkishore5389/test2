using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace OIdataViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Striks = new List<double>();
            Expiries = new List<string>();
            Dates = new List<string>();

            GetStrike();
            GetExpiry();
            GetDates();
            DataContext = this;

            cmbStrick.ItemsSource = Striks;
            cmbExpiry.ItemsSource = Expiries;
            cmbStrick.SelectedValue = Striks[0];
            cmbExpiry.SelectedValue = Expiries[0];

            cmbStrick2.ItemsSource = Striks;
            cmbExpiry2.ItemsSource = Expiries;
            cmbStrick2.SelectedValue = Striks[0];
            cmbExpiry2.SelectedValue = Expiries[0];

            cmbStrick3.ItemsSource = Dates;
            cmbStrick3.SelectedValue = Dates[0];
            cmbExpiry3.ItemsSource = Expiries;
            cmbExpiry3.SelectedValue = Expiries[0];

            cmbDate.ItemsSource = Dates;
            cmbDate.SelectedValue = Dates[0];
            cmbDate2.ItemsSource = Dates;
            cmbDate2.SelectedValue = Dates[0];
            DataContext = this;


        }
        private void GetExpiry()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(@"Data Source=PU8L8PG6QQ2\SQLEXPRESS;Initial Catalog=db_Test;Persist Security Info=True;User ID=saa;Password=saa"))
            {
                conn.Open();

                string query = "select distinct expiry from OptionData where expiry > getdate() order by expiry";


                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, conn);
                dataAdapter.Fill(dt);
                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DateTime exp = Convert.ToDateTime(dt.Rows[i]["expiry"]);
                        Expiries.Add(exp.ToString("ddMMMyyyy"));

                    }
                }

            }
        }

        private void GetDates()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(@"Data Source=PU8L8PG6QQ2\SQLEXPRESS;Initial Catalog=db_Test;Persist Security Info=True;User ID=saa;Password=saa"))
            {
                conn.Open();

                string query = "select distinct CAST(datetime as date ) as datetime from OptionData order by CAST(datetime as date ) desc";


                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, conn);
                dataAdapter.Fill(dt);
                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DateTime exp = Convert.ToDateTime(dt.Rows[i]["datetime"]);
                        Dates.Add(exp.ToString("ddMMMyyyy"));

                    }
                }

            }
        }
        private void GetStrike()
        {
            DataTable dt = new DataTable();
            int nifty = 0;
            using (SqlConnection conn = new SqlConnection(@"Data Source=PU8L8PG6QQ2\SQLEXPRESS;Initial Catalog=db_Test;Persist Security Info=True;User ID=saa;Password=saa"))
            {
                conn.Open();

                string query = "select top(1) nifty  from OptionData  order by datetime desc";


                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, conn);
                dataAdapter.Fill(dt);
                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        nifty = Convert.ToInt32(dt.Rows[i]["nifty"]);
                    }
                }

                int n = 100 * ((int)(nifty / 100));
                // for (int i = 1; i <= 4; i++)
                {
                    Striks.Add(n - (100 * 7));
                    Striks.Add(n - (100 * 6));
                    Striks.Add(n - (100 * 5));
                    Striks.Add(n - (100 * 4));
                    Striks.Add(n - (100 * 3));
                    Striks.Add(n - (100 * 2));
                    Striks.Add(n - (100 * 1));

                    Striks.Add(n + (100 * 0));

                    Striks.Add(n + (100 * 1));
                    Striks.Add(n + (100 * 2));
                    Striks.Add(n + (100 * 3));
                    Striks.Add(n + (100 * 4));
                    Striks.Add(n + (100 * 5));
                    Striks.Add(n + (100 * 6));
                    Striks.Add(n + (100 * 7));
                }
            }
        }
        List<double> Striks { get; set; }
        List<string> Expiries { get; set; }

        List<string> Dates { get; set; }
        public List<Model> GetDailyData(string date, string expiry)
        {
            List<Model> tempOptionDataSource = new List<Model>();

            using (SqlConnection conn = new SqlConnection(@"Data Source=PU8L8PG6QQ2\SQLEXPRESS;Initial Catalog=db_Test;Persist Security Info=True;User ID=saa;Password=saa"))
            {
                DataTable dt = new DataTable();

                conn.Open();

                StringBuilder builder = new StringBuilder();
                builder.Append("select * from (");
                builder.Append("  select *, row_number() over(partition by expiry,[strickPrice]   order by datetime desc) as rn from [OptionData]");
                builder.Append(") t");
                builder.Append(" where t.rn = 1 and CAST(expiry as date ) = CAST('" + expiry + "' as date) and  CAST(datetime as date ) = CAST('" + date + "' as date)");
                string query = builder.ToString();

                SqlDataAdapter dataAdapter = new SqlDataAdapter(builder.ToString(), conn);
                dataAdapter.Fill(dt);

                List<Model> list1 = new List<Model>();
                List<Model> list2 = new List<Model>();

                List<decimal> niftyList = new List<decimal>();
                List<double> niftyTime = new List<double>();
                Model preModel = null, Model = null;
                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Model = new Model()
                        {
                            Date = Convert.ToDateTime(dt.Rows[i]["datetime"]),
                            expiry = Convert.ToDateTime(dt.Rows[i]["expiry"]),
                            nifty = Convert.ToDecimal(dt.Rows[i]["nifty"]),
                            strickPrice = Convert.ToDecimal(dt.Rows[i]["strickPrice"]),

                            call_OI = Convert.ToDecimal(dt.Rows[i]["call_OI"]),
                            call_COI = Convert.ToDecimal(dt.Rows[i]["call_COI"]),
                            call_Volume = Convert.ToDecimal(dt.Rows[i]["call_Volume"]),
                            call_IV = Convert.ToDecimal(dt.Rows[i]["call_IV"]),
                            call_LTP = Convert.ToDecimal(dt.Rows[i]["call_LTP"]),

                            put_OI = Convert.ToDecimal(dt.Rows[i]["put_OI"]),
                            put_COI = Convert.ToDecimal(dt.Rows[i]["put_COI"]),
                            put_Volume = Convert.ToDecimal(dt.Rows[i]["put_Volume"]),
                            put_IV = Convert.ToDecimal(dt.Rows[i]["put_IV"]),
                            put_LTP = Convert.ToDecimal(dt.Rows[i]["put_LTP"])


                        };

                        Model.signal = GenerateSignals(Model, preModel);

                        tempOptionDataSource.Add(Model);
                        preModel = Model;

                        DateTime d = Convert.ToDateTime(dt.Rows[i]["datetime"]);
                        string hr = d.TimeOfDay.Hours.ToString("D2");
                        string min = d.TimeOfDay.Minutes.ToString("D2"); ;



                        niftyList.Add(getNifty(Convert.ToDouble(dt.Rows[i]["nifty"])));
                        niftyTime.Add(Convert.ToDouble(hr + min));
                    }
                }
            }

            return tempOptionDataSource;
        }

        public List<Model> GetIntraDayData(double strike, string expiry, DateTime currentday)
        {
            List<Model> tempOptionDataSource = new List<Model>();

            using (SqlConnection conn = new SqlConnection(@"Data Source=PU8L8PG6QQ2\SQLEXPRESS;Initial Catalog=db_Test;Persist Security Info=True;User ID=saa;Password=saa"))
            {
                DataTable dt = new DataTable();
                conn.Open();
                string query = "select* from OptionData where strickPrice = " + strike + " and  CAST(datetime as date ) = CAST('" + currentday + "' as date) and  CAST('" + expiry + "' as date ) = CAST(expiry as date) order by datetime";
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, conn);
                dataAdapter.Fill(dt);

                List<Model> list1 = new List<Model>();
                List<Model> list2 = new List<Model>();

                List<decimal> niftyList = new List<decimal>();
                List<double> niftyTime = new List<double>();
                Model preModel = null, Model = null;
                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Model = new Model()
                        {
                            Date = Convert.ToDateTime(dt.Rows[i]["datetime"]),
                            expiry = Convert.ToDateTime(dt.Rows[i]["expiry"]),
                            nifty = Convert.ToDecimal(dt.Rows[i]["nifty"]),
                            strickPrice = Convert.ToDecimal(dt.Rows[i]["strickPrice"]),

                            call_OI = Convert.ToDecimal(dt.Rows[i]["call_OI"]),
                            call_COI = Convert.ToDecimal(dt.Rows[i]["call_COI"]),
                            call_Volume = Convert.ToDecimal(dt.Rows[i]["call_Volume"]),
                            call_IV = Convert.ToDecimal(dt.Rows[i]["call_IV"]),
                            call_LTP = Convert.ToDecimal(dt.Rows[i]["call_LTP"]),

                            put_OI = Convert.ToDecimal(dt.Rows[i]["put_OI"]),
                            put_COI = Convert.ToDecimal(dt.Rows[i]["put_COI"]),
                            put_Volume = Convert.ToDecimal(dt.Rows[i]["put_Volume"]),
                            put_IV = Convert.ToDecimal(dt.Rows[i]["put_IV"]),
                            put_LTP = Convert.ToDecimal(dt.Rows[i]["put_LTP"])


                        };

                        Model.signal = GenerateSignals(Model, preModel);

                        tempOptionDataSource.Add(Model);
                        preModel = Model;

                        DateTime d = Convert.ToDateTime(dt.Rows[i]["datetime"]);
                        string hr = d.TimeOfDay.Hours.ToString("D2");
                        string min = d.TimeOfDay.Minutes.ToString("D2"); ;



                        niftyList.Add(getNifty(Convert.ToDouble(dt.Rows[i]["nifty"])));
                        niftyTime.Add(Convert.ToDouble(hr + min));
                    }
                }
            }

            return tempOptionDataSource;
        }

        private string GenerateSignals(Model model, Model preModel)
        {
            string signal = string.Empty;

            if (preModel?.call_OI > 0 && model?.call_OI > 0)
            {
                decimal PCR = model.put_OI / model.call_OI;
                decimal prePCR = preModel.put_OI / preModel.call_OI;


                //call option
                if (PCR < prePCR && model.call_OI > preModel.call_OI && model.call_IV > preModel.call_IV)
                {
                    signal = "BL_long in call"; //bull
                }
                else if (PCR > prePCR && model.call_OI < preModel.call_OI && model.call_IV > preModel.call_IV)
                {
                    signal = "BL_short covering in call"; //bull
                }
                else if (PCR < prePCR && model.call_OI > preModel.call_OI && model.call_IV < preModel.call_IV)
                {
                    signal = "BR_short in call";
                }
                else if (PCR > prePCR && model.call_OI < preModel.call_OI && model.call_IV < preModel.call_IV)
                {
                    signal = "BR_long unwinding in call";
                }
                //put option
                else if (PCR > prePCR && model.put_OI > preModel.put_OI && model.put_IV < preModel.put_IV)
                {
                    signal = "BL_short in put"; //bull
                }
                else if (PCR < prePCR && model.put_OI < preModel.put_OI && model.put_IV < preModel.put_IV)
                {
                    signal = "BL_long unwinding in put"; //bull
                }
                else if (PCR > prePCR && model.put_OI > preModel.put_OI && model.put_IV > preModel.put_IV)
                {
                    signal = "BR_long in put";
                }
                else if (PCR < prePCR && model.put_OI < preModel.put_OI && model.put_IV > preModel.put_IV)
                {
                    signal = "BR_short covering in put";
                }
            }
            return signal;

        }

        private decimal getNifty(double nifty)
        {
            int n = 500 * ((int)(nifty / 500));

            var t = nifty - n;

            return Convert.ToDecimal(t);
        }

        private void CmbStrick_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime dt = DateTime.Now;

            if (cmbDate.SelectedValue != null)
            {
                DateTime.TryParse(Convert.ToString(cmbDate.SelectedValue), out dt);
            }

            double strike = string.IsNullOrEmpty(Convert.ToString(cmbStrick.SelectedValue)) ? Striks[0] : Convert.ToDouble(cmbStrick.SelectedValue);

            string expiry = string.IsNullOrEmpty(Convert.ToString(cmbExpiry.SelectedValue)) ? Expiries[0] : Convert.ToString(cmbExpiry.SelectedValue);

            listview.ItemsSource = GetIntraDayData(strike, expiry, dt);


        }

        private void CmbExpiry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime dt = DateTime.Now;

            if (cmbDate.SelectedValue != null)
            {
                DateTime.TryParse(Convert.ToString(cmbDate.SelectedValue), out dt);
            }
            double strike = string.IsNullOrEmpty(Convert.ToString(cmbStrick.SelectedValue)) ? Striks[0] : Convert.ToDouble(cmbStrick.SelectedValue);

            string expiry = string.IsNullOrEmpty(Convert.ToString(cmbExpiry.SelectedValue)) ? Expiries[0] : Convert.ToString(cmbExpiry.SelectedValue);

            listview.ItemsSource = GetIntraDayData(strike, expiry, dt);
        }



        private void CmbStrick_SelectionChanged2(object sender, SelectionChangedEventArgs e)
        {
            DateTime dt = DateTime.Now;

            if (cmbDate2.SelectedValue != null)
            {
                DateTime.TryParse(Convert.ToString(cmbDate2.SelectedValue), out dt);
            }
            double strike = string.IsNullOrEmpty(Convert.ToString(cmbStrick2.SelectedValue)) ? Striks[0] : Convert.ToDouble(cmbStrick2.SelectedValue);

            string expiry = string.IsNullOrEmpty(Convert.ToString(cmbExpiry2.SelectedValue)) ? Expiries[0] : Convert.ToString(cmbExpiry2.SelectedValue);

            listview2.ItemsSource = GetIntraDayData(strike, expiry, dt);
        }

        private void CmbExpiry_SelectionChanged2(object sender, SelectionChangedEventArgs e)
        {
            DateTime dt = DateTime.Now;

            if (cmbDate2.SelectedValue != null)
            {
                DateTime.TryParse(Convert.ToString(cmbDate2.SelectedValue), out dt);
            }
            double strike = string.IsNullOrEmpty(Convert.ToString(cmbStrick2.SelectedValue)) ? Striks[0] : Convert.ToDouble(cmbStrick2.SelectedValue);

            string expiry = string.IsNullOrEmpty(Convert.ToString(cmbExpiry2.SelectedValue)) ? Expiries[0] : Convert.ToString(cmbExpiry2.SelectedValue);

            listview2.ItemsSource = GetIntraDayData(strike, expiry, dt);
        }

        private void CmbStrick_SelectionChanged3(object sender, SelectionChangedEventArgs e)
        {
            string date = string.IsNullOrEmpty(Convert.ToString(cmbStrick3.SelectedValue)) ? Dates[0] : Convert.ToString(cmbStrick3.SelectedValue);

            string expiry = string.IsNullOrEmpty(Convert.ToString(cmbExpiry3.SelectedValue)) ? Expiries[0] : Convert.ToString(cmbExpiry3.SelectedValue);

            listview3.ItemsSource = GetDailyData(date, expiry);
        }

        private void CmbExpiry_SelectionChanged3(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CmbStrick_SelectionChanged4(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CmbExpiry_SelectionChanged4(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CmbDate_SelectionChanged2(object sender, SelectionChangedEventArgs e)
        {
            DateTime dt = DateTime.Now;

            if (cmbDate2.SelectedValue != null)
            {
                DateTime.TryParse(Convert.ToString(cmbDate2.SelectedValue), out dt);
            }
            double strike = string.IsNullOrEmpty(Convert.ToString(cmbStrick2.SelectedValue)) ? Striks[0] : Convert.ToDouble(cmbStrick2.SelectedValue);

            string expiry = string.IsNullOrEmpty(Convert.ToString(cmbExpiry2.SelectedValue)) ? Expiries[0] : Convert.ToString(cmbExpiry2.SelectedValue);

            listview2.ItemsSource = GetIntraDayData(strike, expiry, dt);


        }

        private void CmbDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime dt = DateTime.Now;

            if (cmbDate.SelectedValue != null)
            {
                DateTime.TryParse(Convert.ToString(cmbDate.SelectedValue), out dt);
            }
            double strike = string.IsNullOrEmpty(Convert.ToString(cmbStrick.SelectedValue)) ? Striks[0] : Convert.ToDouble(cmbStrick.SelectedValue);

            string expiry = string.IsNullOrEmpty(Convert.ToString(cmbExpiry.SelectedValue)) ? Expiries[0] : Convert.ToString(cmbExpiry.SelectedValue);

            listview.ItemsSource = GetIntraDayData(strike, expiry, dt);
        }
    }

}

