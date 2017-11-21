using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BitcoinATM_Application.TransferObject.Parameter
{
    public class ConnectParameter
    {
        public Header Header { get; set; }
        public Body Body { get; set; }
    }
    public class Header
    {
        public string ClientName { get; set; }
        public string ClientKey { get; set; }
    }
    public class Body
    {
        public string ActionName { get; set; }
        public dynamic Parameter { get; set; }
        public string MessageNotedClient { get; set; }
    }
}