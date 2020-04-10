using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OIdataViewer
{
   public class Model
    {
        public DateTime Date { get; set; }
        public DateTime expiry { get; set; }

        public decimal nifty { get; set; }
        public decimal strickPrice { get; set; }
        public decimal call_OI { get; set; }
        public decimal call_COI { get; set; }
        public decimal call_Volume { get; set; }
        public decimal call_IV { get; set; }
        public decimal call_LTP { get; set; }

        public decimal put_OI { get; set; }
        public decimal put_COI { get; set; }
        public decimal put_Volume { get; set; }
        public decimal put_IV { get; set; }
        public decimal put_LTP { get; set; }

        public string signal { get; set; }
    }

}

