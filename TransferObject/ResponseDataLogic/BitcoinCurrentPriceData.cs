using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitcoinATM_Application.TransferObject.ResponseDataLogic
{
    public class BitcoinCurrentPriceData
    {
        public string Status { get; set; }
        public DataCurrentPrice Data { get; set; }
    }
    public class DataCurrentPrice {
        public string Network { get; set; }
        public List<PriceData> Prices { get; set; }
    }
}
