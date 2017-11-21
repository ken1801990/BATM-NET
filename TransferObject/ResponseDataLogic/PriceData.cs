using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitcoinATM_Application.TransferObject.ResponseDataLogic
{
    public class PriceData
    {
        public double Price { get; set; }
        public string Price_Base { get; set; }
        public string Exchange { get; set; }
        public long Time { get; set; }
    }
}
