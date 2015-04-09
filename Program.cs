#undef STEINMART
#define NORMAL
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Reflection;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels; 

using System.Configuration;
using SalesOrdEntry.Epicor.SessionModSvc;
using SalesOrdEntry.Epicor.SalesOrderSvc;

using FastLoad;

namespace SalesOrdEntry
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        /*
        private static string epiUser = ConfigurationManager.AppSettings.Get("username");
        private static string epiPassword = ConfigurationManager.AppSettings.Get("password");
        private static string epiServer = ConfigurationManager.AppSettings.Get("servername");
        private static string epiSite = ConfigurationManager.AppSettings.Get("sitename");
        */
        private static string epiUser = "manager";
        private static string epiPassword = "manager";
        private static string epiServer = "ITEP10";
        // private static string epiSite = "Epicor10ProductionUser";
        private static string epiSite = "Epicor10Production";

        private enum EndpointBindingType
        {
            SOAPHttp,
            BasicHttp
        }

        private static WSHttpBinding GetWsHttpBinding()
        {
            var binding = new WSHttpBinding();
            const int maxBindingSize = Int32.MaxValue;
            binding.MaxReceivedMessageSize = maxBindingSize;
            binding.ReaderQuotas.MaxDepth = maxBindingSize;
            binding.ReaderQuotas.MaxStringContentLength = maxBindingSize;
            binding.ReaderQuotas.MaxArrayLength = maxBindingSize;
            binding.ReaderQuotas.MaxBytesPerRead = maxBindingSize;
            binding.ReaderQuotas.MaxNameTableCharCount = maxBindingSize;
            binding.Security.Mode = SecurityMode.Message;
            binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
            return binding;
        }

        public static BasicHttpBinding GetBasicHttpBinding()
        {
            var binding = new BasicHttpBinding();
            const int maxBindingSize = Int32.MaxValue;
            binding.MaxReceivedMessageSize = maxBindingSize;
            binding.ReaderQuotas.MaxDepth = maxBindingSize;
            binding.ReaderQuotas.MaxStringContentLength = maxBindingSize;
            binding.ReaderQuotas.MaxArrayLength = maxBindingSize;
            binding.ReaderQuotas.MaxBytesPerRead = maxBindingSize;
            binding.ReaderQuotas.MaxNameTableCharCount = maxBindingSize;
            binding.Security.Mode = BasicHttpSecurityMode.TransportWithMessageCredential;
            binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
            return binding;
        }

        private static TClient GetClient<TClient, TInterface>(string url, string username, string password, EndpointBindingType bindingType)
            where TClient : ClientBase<TInterface>
            where TInterface : class
        {
            System.ServiceModel.Channels.Binding binding = null;
            TClient client;
            var endpointAddress = new EndpointAddress(url);
            switch (bindingType)
            {
                case EndpointBindingType.BasicHttp:
                    binding = GetBasicHttpBinding();
                    break;
                case EndpointBindingType.SOAPHttp:
                    binding = GetWsHttpBinding();
                    break;
            }
            TimeSpan operationTimeout = new TimeSpan(0, 12, 0);
            binding.CloseTimeout = operationTimeout;
            binding.ReceiveTimeout = operationTimeout;
            binding.SendTimeout = operationTimeout;
            binding.OpenTimeout = operationTimeout;

            client = (TClient)Activator.CreateInstance(typeof(TClient), binding, endpointAddress);
            if (!string.IsNullOrEmpty(username) && (client.ClientCredentials != null))
            {
                client.ClientCredentials.UserName.UserName = username;
                client.ClientCredentials.UserName.Password = password;
            }
            return client;
        }
        static void LoadSalesOrder(SalesOrderSvcContractClient salesOrderClient, SalesOrder so, out string result)
        {
            var ts = new SalesOrderTableset();
            result = "p_";
            salesOrderClient.GetNewOrderHed(ref ts);

            var newRow = ts.OrderHed.Where(n => n.RowMod.Equals("A", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            // Guid rowID = newRow.SysRowID;

            // FastLoad.SalesOrder so = new FastLoad.SalesOrder();
            E10Lookup look = new E10Lookup();
            int custNum = look.GetCustomerNum(so.CustomerID);

            if (newRow != null)
            {
                newRow.Company = so.Company;

                Epicor.SalesOrderSvc.UserDefinedColumns columns = newRow.UserDefinedColumns;

                /*
                foreach (KeyValuePair<string, object> kvp in columns)
                {
                    Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                }
                */
                columns["OrderType_c"] = "EDI";
                columns["EDIHeadChar11_c"] = "";
                // columns["EDIHeadChar11_c"] = "hello";

                // newRow.BTCustID = so.CustomerID;
                newRow.CustNum = custNum;
                newRow.BTCustNum = custNum;
                if (so.ShipVia.Equals("UGND"))
                {
                    newRow.ShipViaCode = "UPGD";
                }
                else
                {
                    newRow.ShipViaCode = so.ShipVia;
                }

                newRow.PONum = so.PoNo;
                newRow.TermsCode = so.TermsCode;
                newRow.ShipToNum = so.ShipToNum;
                newRow.ShipToCustNum = custNum;
                newRow.NeedByDate = so.NeedByDate;
                newRow.OrderDate = so.OrderDate;
                newRow.RequestDate = so.RequestDate;
                newRow.ReadyToCalc = true;
                
#if STEINMART
                newRow.PickListComment = so.ediMarkingNotes;
                newRow.RefNotes = so.ediMarkingNotes;
                newRow.ShipComment = so.ediMarkingNotes;

#endif
                newRow.RowMod = "A";
                try
                {
                    salesOrderClient.Update(ref ts);
                }
                catch (Exception e)
                {
                    string message = e.Message;
                    result = message.Substring(0, 3);
                    bool AllOk = false;
                }
            }

            int orderNum = ts.OrderHed[0].OrderNum;

            ts = salesOrderClient.GetByID(orderNum);

            if (ts != null)
            {
                result = orderNum.ToString();
                foreach (OrderLine line in so.lines)
                {
                    salesOrderClient.GetNewOrderDtl(ref ts, orderNum);
                    string PartDescription = look.GetPartDescr(line.Upc);
                    //  ts = salesOrderClient.GetByID(orderNum);
                    string dtlrow = ts.OrderDtl[0].RowMod;
                    var newDtlRow = ts.OrderDtl.Where(n => n.RowMod.Equals("A", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    newDtlRow.PartNum = line.Upc;
                    newDtlRow.PartNumPartDescription = PartDescription;
                    newDtlRow.LineDesc = PartDescription;
                    newDtlRow.XPartNum = line.CustomerPart;
                    newDtlRow.OrderLine = line.LineNum;
                    newDtlRow.OrderQty = line.OrderQty;
                    newDtlRow.PricingQty = line.OrderQty;
                    newDtlRow.SellingQuantity = line.OrderQty;
                    newDtlRow.SalesUM = so.Get_UOM_FromSellingFactor(line.SellingFactor);

                    // newDtlRow.DocInUnitPrice = line.UnitPrice;
                    newDtlRow.DocUnitPrice = line.UnitPrice;
                    // newDtlRow.Reference = "123456";
                    newDtlRow.UnitPrice = line.UnitPrice;
                    newDtlRow.SellingFactor = line.SellingFactor;

                    newDtlRow.RowMod = "A";
                    // newRow.RowMod = "U";
                    try
                    {
                        salesOrderClient.Update(ref ts);
                        ts = salesOrderClient.GetByID(orderNum);
                    }
                    catch (Exception ex2)
                    {
                        string mess2 = ex2.Message;
                        // result = ex2.Message;
                    }

                }
                try
                {
                    ts = salesOrderClient.GetByID(orderNum);
                    TaxConnectStatusRow taxrow = ts.TaxConnectStatus.First();
                    taxrow.ETCOffline = true;
                    salesOrderClient.Update(ref ts);
                }
                catch (Exception ex2)
                {
                    string messtaxOffline = ex2.Message;
                    // result = ex2.Message;
                }
            }
        }

        [STAThread]
        static void Main()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => { return true; };

            EndpointBindingType bindingType = EndpointBindingType.BasicHttp;

            string epicorUserID = epiUser;
            string epiorUserPassword = epiPassword;

            string scheme = "http";
            if (bindingType == EndpointBindingType.BasicHttp)
            {
                scheme = "https";
            }
            UriBuilder builder = new UriBuilder(scheme, epiServer);

            builder.Path = epiSite + "/Ice/Lib/SessionMod.svc";

            SessionModSvcContractClient sessionModClient = GetClient<SessionModSvcContractClient, SessionModSvcContract>(builder.Uri.ToString(), epicorUserID, epiorUserPassword, bindingType);

            builder.Path = epiSite + "/Erp/BO/SalesOrder.svc";
            SalesOrderSvcContractClient salesOrderClient = GetClient<SalesOrderSvcContractClient, SalesOrderSvcContract>(builder.Uri.ToString(), epiorUserPassword, epicorUserID, bindingType);

            Guid sessionId = Guid.Empty;
#if NORMAL
            try
            {
                sessionId = sessionModClient.Login();
                sessionModClient.Endpoint.Behaviors.Add(new HookServiceBehavior(sessionId, epicorUserID));
                salesOrderClient.Endpoint.Behaviors.Add(new HookServiceBehavior(sessionId, epicorUserID));


                string dirName = @"Z:\e10\EDI_Data\IN_XML";
                string[] filePaths = Directory.GetFiles(dirName);
                string message = "";
                bool AllOk = true;
                TestData testData = new TestData();
                string result = "";
                foreach (string fileName in filePaths)
                {
                    try
                    {
                        XmlReader reader = new XmlReader(fileName);
                        SalesOrder salesOrder = reader.GetSalesOrder();
                        result = "";
                        LoadSalesOrder(salesOrderClient, salesOrder, out result);
                    }
                    catch (Exception e)
                    {
                        message = e.Message;
                        AllOk = false;
                    }
                    if (AllOk)
                    {
                        testData.MoveFile(fileName, result);
                    }
                    else
                    {
                        testData.MoveFile(fileName, result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ex" + ex.Message);
                sessionModClient.Logout();
            }
            if (sessionId != Guid.Empty)
            {
                sessionModClient.Logout();
            }
        }

#endif
#if STEINMART
            try
            {
                sessionId = sessionModClient.Login();
                sessionModClient.Endpoint.Behaviors.Add(new HookServiceBehavior(sessionId, epicorUserID));
                salesOrderClient.Endpoint.Behaviors.Add(new HookServiceBehavior(sessionId, epicorUserID));
                SteinmartDataReader reader = new SteinmartDataReader();
                ArrayList so_list = reader.GetSOList();
                foreach (SalesOrder so in so_list)
                {
                    string result;
                    try
                    {
                        LoadSalesOrder(salesOrderClient, so, out result);
                    }
                    catch (Exception e)
                    {
                        string message = e.Message;
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ex" + ex.Message);
                sessionModClient.Logout();
            }
            if (sessionId != Guid.Empty)
            {
                sessionModClient.Logout();
            }
        }
#endif
    }
}