using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Globalization;

namespace ICAssessment_ChristianMartinez
{
    public class Program
    {
        public static List<AccountBilling> InvoiceDataset = new List<AccountBilling>();
        public static List<BillHeader> BillHeaderList = new List<BillHeader>();
        public static List<Bill> BillList = new List<Bill>();
        public static List<AddressInformation> AddressInfoList = new List<AddressInformation>();
        private static string GUID = "8203ACC7-2094-43CC-8F7A-B8F19AA9BDA2";//Globally Unique Identifier 
        private static string InvoiceFormat = "8E2FEA69-5D77-4D0F-898E-DFA25677D19E";

        //[Required Function]: Parses only Bill Header children only
        public static void ParseBillHeader()
        {
            XDocument xdoc = XDocument.Load("C:\\Users\\Christian\\Desktop\\ICAssessment-ChristianMartinez\\database\\BillFile.xml");
            xdoc.Descendants("BILL_HEADER").Select(p => new
            {
                invoiceno = p.Element("Invoice_No").Value,
                accountno = p.Element("Account_No").Value,
                customername = p.Element("Customer_Name").Value,
                cyclecd = p.Element("Cycle_Cd").Value,
                billdt = p.Element("Bill_Dt").Value,
                duedt = p.Element("Due_Dt").Value,
                accountclass = p.Element("Account_Class").Value
            }).ToList().ForEach(p =>
            {
                //Bill Header Constructor:  public BillHeader(string invoiceno, string accountno, string customername, string cyclecd, string billdt, string duedt, string accountclass)
                BillHeader newBillHeader = new BillHeader(p.invoiceno, p.accountno, p.customername, p.cyclecd, p.billdt, p.duedt, p.accountclass);

                //Add new Bill Header to BillHeader List
                BillHeaderList.Add(newBillHeader);
            });
        }

        //[Required Function]: Parses only Bill children only
        public static void ParseBill()
        {
            XDocument xdoc = XDocument.Load("C:\\Users\\Christian\\Desktop\\ICAssessment-ChristianMartinez\\database\\BillFile.xml");
            xdoc.Descendants("Bill").Select(p => new
            {
                billamount = p.Element("Bill_Amount").Value,
                balancedue = p.Element("Balance_Due").Value,
                billrundt = p.Element("Bill_Run_Dt").Value,
                billrunseq = p.Element("Bill_Run_Seq").Value,
                billruntm = p.Element("Bill_Run_Tm").Value,
                billtp = p.Element("Bill_Tp").Value
            }).ToList().ForEach(p =>
            {
                //Bill Constructor: public Bill(double billamount, double balancedue, string billrundt, int billrunseq, int billruntm, string billtype)
                Bill newBill = new Bill(Convert.ToDouble(p.billamount), Convert.ToDouble(p.balancedue), p.billrundt, Convert.ToInt32(p.billrunseq), Convert.ToInt32(p.billruntm), p.billtp);

                //Add new Bill to Bill List
                BillList.Add(newBill);
            });
        }

        //[Required Function]: Parses only Address Information children only
        public static void ParseAddress()
        {
            XDocument xdoc = XDocument.Load("C:\\Users\\Christian\\Desktop\\ICAssessment-ChristianMartinez\\database\\BillFile.xml");
            xdoc.Descendants("Address_Information").Select(p => new
            {
                mailingaddress1 = p.Element("Mailing_Address_1").Value,
                mailingaddress2 = p.Element("Mailing_Address_2").Value,
                city = p.Element("City").Value,
                state = p.Element("State").Value,
                zip = p.Element("Zip").Value
            }).ToList().ForEach(p =>
            {
                //Constructor: public AddressInformation(string mailingaddress1, string mailingaddress2, string city, string state, string zip)
                AddressInformation newAddressInfo = new AddressInformation(p.mailingaddress1, p.mailingaddress2, p.city, p.state, p.zip);

                //Add to AddressInfo List
                AddressInfoList.Add(newAddressInfo);
            });
        }

        //[Required Function]: Parses XML file to 3 parts: BillHeader, Bill, and AddressInforamtion
        public static void ParseXML()
        {
            ParseBillHeader();
            ParseBill();
            ParseAddress();

            //Merges info to AccountBilling Constructor by combining the 3 classes that made up a bill.  
            for (int i = 0; i < BillHeaderList.Count; i++)
            {
                AccountBilling newAccountBilling = new AccountBilling(BillHeaderList[i], BillList[i], AddressInfoList[i]); //Creating account
                InvoiceDataset.Add(newAccountBilling); //Adding account to list of accounts
            }
        }

        //[Required Function]: Creates file header for "BillFile-mmddyyyy.rpt"
        public static void BuildFileHeader(TextWriter tw)
        {
            double sum = 0;
            for (int i = 0; i < InvoiceDataset.Count; i++)
            {
                sum += InvoiceDataset[i].BillInfo.BillAmount;
            }

            Dictionary<int, string> header = new Dictionary<int, string> {
                {1, "FR" }, //?
                {2, "["+ GUID +"]" }, //Globally Unique Identifier
                {3, "Sample UT file" }, //?
                {4, "[" + DateTime.Now.ToString("MM-dd-yyyy") + "]" }, //DateTime.Now.ToString("MM-dd-yyyy");
                {5, "["+ InvoiceDataset.Count+"]" }, //InvoiceDataset.Count;
                {6, "[" + sum + "]" } //bill_amount_sum = bill.values[i]
            };


            foreach (KeyValuePair<int, string> pair in header)
            {
                string kvpformat = $"{pair.Key}~{pair.Value}";
                tw.Write(kvpformat);
                if (pair.Key != 6) tw.Write("|");
                //Console.Write(kvpformat);
            }
            tw.WriteLine();
        }

        //[Required Function]: Turns XML Parsed information content to KPV format for "BillFile-mmddyyyy.rpt"
        public static void BuildFileBill(TextWriter tw)
        {
            DateTime.Now.ToString("MM-dd-yyyy");
            for (int i = 0; i < InvoiceDataset.Count; i++)
            {
                Dictionary<string, string> invoice = new Dictionary<string, string>{
                    {"AA", "CT" }, //?
                    {"BB", "[" +InvoiceDataset[i].BillHeaderInfo.AccountNo+ "]" }, //InvoiceDataset[i].BillHeader.Account_No;
                    {"VV", "["+InvoiceDataset[i].BillHeaderInfo.CustomerName+"]" }, //InvoiceDataset[i].BillHeader.Customer_Name;
                    {"CC", "["+InvoiceDataset[i].AddressInfo.MailingAddress1+"]" }, //InvoiceDataset[i].Address_Information.Mailing_Address_1
                    {"DD", "["+InvoiceDataset[i].AddressInfo.MailingAddress2+"]" }, //InvoiceDataset[i].BillHeader.Account_No
                    {"EE", "["+InvoiceDataset[i].AddressInfo.City+"]" },
                    {"FF", "["+InvoiceDataset[i].AddressInfo.State+"]" },
                    {"GG", "["+InvoiceDataset[i].AddressInfo.Zip+"]" },

                    {"HH", "IH" }, //?
                    {"II", "R" }, //?
                    {"JJ", "["+InvoiceFormat+"]" },
                    {"KK", "["+InvoiceDataset[i].BillHeaderInfo.InvoiceNo+"]" },
                    {"LL", "["+InvoiceDataset[i].BillHeaderInfo.BillDt+"]" },
                    {"MM", "["+InvoiceDataset[i].BillHeaderInfo.DueDt+"]" },
                    {"NN", "["+InvoiceDataset[i].BillInfo.BillAmount+"]" },
                    {"OO", "["+ DateTime.Now.AddDays(5).ToString("MM-dd-yyyy") +"]" },
                    {"PP", "["+ ConvertDateMonth(InvoiceDataset[i].BillHeaderInfo.DueDt).AddDays(-3).ToString("MM-dd-yyyy") +"]" },
                    {"QQ", "["+InvoiceDataset[i].BillInfo.BalanceDue+"]" },
                    {"RR", "["+DateTime.Now.ToString("MM-dd-yyyy")+"]" },
                    {"SS", "[SERVICE_ADDRESS]" } //?
                };

                foreach (KeyValuePair<string, string> pair in invoice)
                {
                    string kvpformat = $"{pair.Key}~{pair.Value}";
                    tw.Write(kvpformat);
                    if (pair.Key == "GG") tw.WriteLine("");
                    if (pair.Key != "SS" && pair.Key != "GG") tw.Write("|");
                }
                tw.WriteLine();
            }


        }

        //[Required Function] Writes to BillFile by using parsed information from BillFile.xml
        public static void CreateBillFile()
        {
            //Name export file "BillFile-mmddyyyy.rpt"
            string date = DateTime.Now.ToString("MM-dd-yyyy");
            string dateformated = Regex.Replace(date, @"[^0-9]", "");
            string filename = "BillFile-" + dateformated + ".txt"; //CHANGE THIS TO RPT LATER!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            string path = "C:\\Users\\Christian\\Desktop\\ICAssessment-ChristianMartinez\\output\\" + filename;

            try
            {
                // Check if file already exists. If yes, delete it.     
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                //Begin writing billfile
                TextWriter tw = new StreamWriter(path, true);
                BuildFileHeader(tw);
                BuildFileBill(tw);
                tw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //[Helper Function]: Prints raq XML content to Console Application
        public static void PrintAllXMLToConsole()
        {
            XmlDocument Xdoc = new XmlDocument();
            Xdoc.Load("C:\\Users\\Christian\\Desktop\\ICAssessment-ChristianMartinez\\database\\BillFile.xml");
            Xdoc.Save(Console.Out);
        }

        //[Helper Function]: Prints all contents inside of InvoiceDataset List 
        public static void PrintMyParsedData()
        {
            for (int i = 0; i < InvoiceDataset.Count; i++)
            {
                Console.WriteLine("Bill #: " + i);
                InvoiceDataset[i].BillHeaderInfo.Print();
                InvoiceDataset[i].BillInfo.Print();
                InvoiceDataset[i].AddressInfo.Print();
                Console.WriteLine("============");
            }
        }

        //[Helper Function]: Turns a date formated {May 4, 2020) and returns a DateTime date formated {5-04-2020 12:00:00}
        public static DateTime ConvertDateMonth(string date)
        {
            string month = Regex.Replace(date, @"[\d-]", string.Empty); //gives me month name only
            DateTime newDate = DateTime.Parse(date);
            return newDate;
        }

        public static void Main(string[] args)
        {
            //PrintAllXMLToConsole();

            //Open, read, and parse XML file
            ParseXML();

            //Test Parsed Date Print
            //PrintMyParsedData();

            //Create file BillFile-mmddyyyy.rpt
            CreateBillFile();

            Console.WriteLine("Completed Assignment...");
        }
    }
}