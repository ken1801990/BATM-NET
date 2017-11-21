using BitcoinATM_Application.TransferObject.Parameter;
using BitcoinATM_Application.TransferObject.ResponseDataLogic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BitcoinATM_Application.Service
{
    public class BitcoinService: ProcessService
    {
        public WalletData GetWallet(BitcoinParameter param)
        {
            var data = CallWallet_BlockIOApi(param.WalletAddress);
            if (string.IsNullOrWhiteSpace(data)) {
                return new WalletData();
            }
            var dataReturn = JsonConvert.DeserializeObject<WalletData>(data) as WalletData;
            if (dataReturn != null) {
                return dataReturn;
            }
            return new WalletData();
        }

        public string CallWallet_BlockIOApi(string walletId) {
            var param = new BitcoinParameter() {
                WalletAddress = walletId
            };
            var objectConnect = this.SetParameterAwsService("BITCOIN_GETWALLETADDRESS", param, string.Empty);
            var data = ServiceCommunicate.RequestPost(JsonConvert.SerializeObject(objectConnect));
            return data.Result;
        }

        public BitcoinCurrentPriceData GetListCurrentPrice() {
            var data = CallCurrentPrice_BlockIOApi();
            if (string.IsNullOrWhiteSpace(data)) {
                return new BitcoinCurrentPriceData();
            }
            var dataReturn = JsonConvert.DeserializeObject<BitcoinCurrentPriceData>(data);
            if (dataReturn != null) {
                return dataReturn;
            }
            return new BitcoinCurrentPriceData();
        }

        public PriceData GetLastedPriceOfUsd() {
            var dataReturn = GetListCurrentPrice();
            if(dataReturn!=null && dataReturn.Data.Prices.Count > 0)
            {
                var bcUsdPrice = dataReturn.Data.Prices.OrderByDescending(k => k.Time).FirstOrDefault(k => k.Price_Base == "USD");
                return bcUsdPrice;
            }
            return new PriceData();

        }

        public string CallCurrentPrice_BlockIOApi()
        {
            var objectConnect = this.SetParameterAwsService("BITCOIN_GETLISTCURRENTPRICE", new BitcoinParameter(), string.Empty);
            var data = ServiceCommunicate.RequestPost(JsonConvert.SerializeObject(objectConnect));
            return data.Result;
        }
    }
}
