using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitcoinATM_Application.TransferObject.ResponseDataLogic
{
    public class WalletData
    {
        public string Status { get; set; }
        public Data Data { get; set; }
        public List<Balance> Balances { get; set; }

    }
    public class Data {
        public string Network { get; set; }
        public double Available_Balance { get; set; }
        public double Pending_Received_Balance { get; set; }
    }
    public class Balance {
        public string UserId { get; set; }
        public string Label { get; set; }
        public string Address { get; set; }
        public double Available_Balance { get; set; }
        public double Pending_Received_Balance { get; set; }
    }
}
