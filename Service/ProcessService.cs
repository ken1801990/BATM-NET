using BitcoinATM_Application.TransferObject.Parameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitcoinATM_Application.Service
{
    public class ProcessService
    {
        public ConnectParameter SetParameterAwsService(string actionName, dynamic param, string messageClient)
        {
            var objectConnect = new ConnectParameter()
            {
                Header = new Header()
                {
                    ClientKey = "WebServer Version 1.0",
                    ClientName = "123456789"
                },
                Body = new Body()
                {
                    ActionName = actionName.ToUpper(),
                    Parameter = param,
                    MessageNotedClient = messageClient
                }
            };
            return objectConnect;
        }
    }
}
