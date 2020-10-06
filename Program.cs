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
using System.Data.OleDb;

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
        private static string connectionString = UpdateConnectionString(); //If FAILS: Replace with connectionString from Billing.mdb

        //[Utility Function]: Updates ConnectionString so user doesn't have to. 
        public static string UpdateConnectionString()
        {
            string part1 = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=";
            string path = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            path = Directory.GetParent(Directory.GetParent(path).FullName).FullName + "\\_database\\Billing.mdb";
            string part3 = ";Persist Security Info=True";
            connectionString = part1 + path + part3;
            return connectionString;
        }

        //[Utility Function]: Prints raq XML content to Console Application
        public static void PrintAllXMLToConsole()
        {
            string path = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            path = Directory.GetParent(Directory.GetParent(path).FullName).FullName + "\\_database\\BillFile.xml"; //Change to local files later. 
            XmlDocument Xdoc = new XmlDocument();
            Xdoc.Load(path);
            Xdoc.Save(Console.Out);
        }

        //[Utility Function]: Prints all contents inside of InvoiceDataset List 
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

        //[Helper Function]: Parses only Bill Header children only in XML
        public static void ParseBillHeaderXML()
        {
            //Fetches path to this folder directory
            string path = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            path = Directory.GetParent(Directory.GetParent(path).FullName).FullName + "\\_database\\BillFile.xml";

            //Load XML file to XDocument variable (hence xdoc)
            XDocument xdoc = XDocument.Load(path);

            //Parses XML and store to BillHeader list
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
                //Revereses full name format "LastName, FirstName" to "FirstName LastName"
                string[] firstlastname = p.customername.Split(',');
                Array.Reverse(firstlastname);
                string fullname = string.Join(" ", firstlastname).TrimStart(' ');

                //Bill Header Constructor:  public BillHeader(string invoiceno, string accountno, string customername, string cyclecd, string billdt, string duedt, string accountclass)
                BillHeader newBillHeader = new BillHeader(p.invoiceno, p.accountno, fullname, p.cyclecd, p.billdt, p.duedt, p.accountclass);

                //Add new Bill Header to BillHeader List
                BillHeaderList.Add(newBillHeader);
            });
        }

        //[Helper Function]: Parses only Bill children only in XML
        public static void ParseBillXML()
        {
            //Fetches path to this folder directory
            string path = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            path = Directory.GetParent(Directory.GetParent(path).FullName).FullName + "\\_database\\BillFile.xml";

            //Load XML file to XDocument variable (hence xdoc)
            XDocument xdoc = XDocument.Load(path);

            //Parses XML and store to Bill list
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

        //[Helper Function]: Parses only Address Information children only in XML
        public static void ParseAddressXML()
        {
            //Fetches path to this folder directory
            string path = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            path = Directory.GetParent(Directory.GetParent(path).FullName).FullName + "\\_database\\BillFile.xml";

            //Load XML file to XDocument variable (hence xdoc)
            XDocument xdoc = XDocument.Load(path);

            //Parses XML and store to AddressInforamtion list
            xdoc.Descendants("Address_Information").Select(p => new
            {
                mailingaddress1 = p.Element("Mailing_Address_1").Value,
                mailingaddress2 = p.Element("Mailing_Address_2").Value,
                city = p.Element("City").Value,
                state = p.Element("State").Value,
                zip = p.Element("Zip").Value
            }).ToList().ForEach(p =>
            {
                //Address Information Constructor: public AddressInformation(string mailingaddress1, string mailingaddress2, string city, string state, string zip)
                AddressInformation newAddressInfo = new AddressInformation(p.mailingaddress1, p.mailingaddress2, p.city, p.state, p.zip);

                //Add to AddressInfo List
                AddressInfoList.Add(newAddressInfo);
            });
        }

        //[Required Function]: Parses XML file to 3 parts: BillHeader, Bill, and AddressInforamtion
        public static void ParseXML()
        {
            ParseBillHeaderXML();
            ParseBillXML();
            ParseAddressXML();

            //Merges info to AccountBilling Constructor by combining the 3 classes that made up a bill.  
            for (int i = 0; i < BillHeaderList.Count; i++)
            {
                AccountBilling newAccountBilling = new AccountBilling(BillHeaderList[i], BillList[i], AddressInfoList[i]); //Creating account
                InvoiceDataset.Add(newAccountBilling); //Adding account to list of accounts
            }
        }

        //[Helper Function]: Creates file header for "BillFile-mmddyyyy.rpt"
        public static void BuildFileHeader(TextWriter tw)
        {
            //Calculate BillAmount total for BuildFileHeader
            double sum = 0;
            for (int i = 0; i < InvoiceDataset.Count; i++)
            {
                sum += InvoiceDataset[i].BillInfo.BillAmount;
            }

            //Stores values to Dictionary from InvoiceDataset list.
            Dictionary<int, string> header = new Dictionary<int, string> {
                {1, "FR" }, //FR
                {2, GUID }, //GUID
                {3, "Sample UT file" }, //Sample UT File
                {4, DateTime.Now.ToString("MM/dd/yyyy") }, //Current Date
                {5, InvoiceDataset.Count.ToString() }, //Number of Bills in this File
                {6, sum.ToString("0.00") } //Total Bill Amount
            };

            //Write FieldID~FieldValue pair to BillFile
            foreach (KeyValuePair<int, string> pair in header)
            {
                string kvpformat = $"{pair.Key}~{pair.Value}";
                tw.Write(kvpformat);
                if (pair.Key != 6) tw.Write("|");
            }
            tw.WriteLine();
        }

        //[Helper Function]: Turns XML Parsed information content to KPV format for "BillFile-mmddyyyy.rpt"
        public static void BuildFileBill(TextWriter tw)
        {
            for (int i = 0; i < InvoiceDataset.Count; i++)
            {
                Dictionary<string, string> invoice = new Dictionary<string, string>{
                    {"AA", "CT" },
                    {"BB", InvoiceDataset[i].BillHeaderInfo.AccountNo }, //Account Number
                    {"VV", InvoiceDataset[i].BillHeaderInfo.CustomerName }, //Customer Name
                    {"CC", InvoiceDataset[i].AddressInfo.MailingAddress1 }, //Mailing Address 1
                    {"DD", InvoiceDataset[i].AddressInfo.MailingAddress2 }, //Mailing Address 2
                    {"EE", InvoiceDataset[i].AddressInfo.City }, //City
                    {"FF", InvoiceDataset[i].AddressInfo.State }, //State
                    {"GG", InvoiceDataset[i].AddressInfo.Zip }, //ZIP

                    {"HH", "IH" }, //IH
                    {"II", "R" }, //R
                    {"JJ", InvoiceFormat }, //Invoice Format
                    {"KK", InvoiceDataset[i].BillHeaderInfo.InvoiceNo }, //Invoice Number
                    {"LL", DateTime.Parse(InvoiceDataset[i].BillHeaderInfo.BillDt).ToString("MM/dd/yyyy") }, //Bill Date
                    {"MM", DateTime.Parse(InvoiceDataset[i].BillHeaderInfo.DueDt).ToString("MM/dd/yyyy") }, //Bill Due Date
                    {"NN", InvoiceDataset[i].BillInfo.BillAmount.ToString("0.00") }, //Bill Amount
                    {"OO", DateTime.Now.AddDays(5).ToString("MM/dd/yyyy") }, //Current Date+5 days = First Notification Date
                    {"PP", DateTime.Parse(InvoiceDataset[i].BillHeaderInfo.DueDt).AddDays(-3).ToString("MM/dd/yyyy") }, //Bill Due Date - 3 days = Second Notification Date
                    {"QQ", InvoiceDataset[i].BillInfo.BalanceDue.ToString("0.00") }, //Balance Due
                    {"RR", DateTime.Now.ToString("MM/dd/yyyy") }, //Customer Added
                    {"SS", "SERVICE_ADDRESS" } //Service Address
                };

                //(Math.Truncate(InvoiceDataset[i].BillInfo.BalanceDue * 100) / 100).ToString()
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

        //[Required Function]: Writes to BillFile by using parsed information from BillFile.xml
        public static void BuildBillFile()
        {
            //Name export file "BillFile-mmddyyyy.rpt"
            string date = DateTime.Now.ToString("MM/dd/yyyy");
            string dateformated = Regex.Replace(date, @"[^0-9]", "");
            string filename = "BillFile-" + dateformated + ".rpt";

            //Change to local files later. 
            string path = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            path = Directory.GetParent(Directory.GetParent(path).FullName).FullName + "\\_output\\" + filename;
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

        //[Helper Function]: Given an array of KVP pairs from "BillFile-mmddyyyy.rpt", parse pair and place them inside of MS Access Database.
        public static void ParseKvpPair(string[] kvpPair)
        {
            Hashtable parsedKVP = new Hashtable();
            //split FieldID~FieldValue to hashtable
            foreach (var pair in kvpPair)
            {
                string[] kvpValues = pair.Split('~');
                parsedKVP.Add(kvpValues[0], kvpValues[1]);
            }

            //Open BillFile and place values inside of MS Access tables
            OleDbConnection con = new OleDbConnection(connectionString);
            OleDbCommand cmd = new OleDbCommand();
            con.Open();

            //If any key contains "AA" then the hashtable contains FieldID values AA-GG
            //Add HH-SS values to MS Access DB
            if (parsedKVP.ContainsKey("AA")) //AA-GG 
            {
                cmd.CommandText = "Insert into [Customer](CustomerName, AccountNumber, CustomerAddress, " +
                    "CustomerCity, CustomerState, CustomerZip)Values(a,b,c,d,e,f)";
                cmd.Parameters.Add("@CustomerName", OleDbType.VarChar).Value = parsedKVP["VV"];
                cmd.Parameters.Add("@AccountNumber", OleDbType.VarChar).Value = parsedKVP["BB"];
                cmd.Parameters.Add("@CustomerAddress", OleDbType.VarChar).Value = parsedKVP["CC"];
                cmd.Parameters.Add("@CustomerCity", OleDbType.VarChar).Value = parsedKVP["EE"];
                cmd.Parameters.Add("@CustomerState", OleDbType.VarChar).Value = parsedKVP["FF"];
                cmd.Parameters.Add("@CustomerZip", OleDbType.VarChar).Value = parsedKVP["GG"];
                cmd.Connection = con;
                cmd.ExecuteNonQuery();
                con.Close();

            }
            //If any key contains "HH" then the hashtable contains FieldID values HH-SS
            //Add HH-SS values to MS Access DB
            else if (parsedKVP.ContainsKey("HH")) //HH-SS
            {
                cmd.CommandText = "Insert into [Bills](BillDate, BillNumber, BillAmount, FormatGUID, " +
                    "AccountBalance, DueDate, ServiceAddress, FirstEmailDate, SecondEmailDate)Values(a,b,c,d,e,f,g,h,i)";
                cmd.Parameters.Add("@BillDate", OleDbType.VarChar).Value = parsedKVP["LL"];
                cmd.Parameters.Add("@BillNumber", OleDbType.VarChar).Value = parsedKVP["KK"];
                cmd.Parameters.Add("@BillAmount", OleDbType.VarChar).Value = parsedKVP["NN"];
                cmd.Parameters.Add("@FormatGUID", OleDbType.VarChar).Value = GUID;
                cmd.Parameters.Add("@AccountBalance", OleDbType.VarChar).Value = parsedKVP["QQ"];
                cmd.Parameters.Add("@DueDate", OleDbType.VarChar).Value = parsedKVP["MM"];
                cmd.Parameters.Add("@ServiceAddress", OleDbType.VarChar).Value = parsedKVP["SS"];
                cmd.Parameters.Add("@FirstEmailDate", OleDbType.VarChar).Value = parsedKVP["OO"];
                cmd.Parameters.Add("@SecondEmailDate", OleDbType.VarChar).Value = parsedKVP["PP"];
                cmd.Connection = con;
                cmd.ExecuteNonQuery();
                con.Close();
            }
        }

        //[Required Function]: Reads BillFile-mmddyyyy.rpt file line-by-line and fills the database per row. 
        public static void ImportDataToMDB() //O(n^2) ... Not the best. Can we do better?
        {
            //Name export file "BillFile-mmddyyyy.rpt"
            string date = DateTime.Now.ToString("MM/dd/yyyy");
            string dateformated = Regex.Replace(date, @"[^0-9]", "");
            string filename = "BillFile-" + dateformated + ".rpt";

            //Fetches path to this folder directory
            string path = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            path = Directory.GetParent(Directory.GetParent(path).FullName).FullName + "\\_output\\" + filename;

            try
            {
                //Open BillFile and read lines
                using (var streamReader = File.OpenText(path))
                {
                    //Grabs all lines in file
                    var lines = streamReader.ReadToEnd().Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        ////Parse File Header
                        //if (line.Substring(0, 1) == "1")
                        //{
                        //    //Console.WriteLine("1");
                        //    string[] kvpPair = line.Split('|');
                        //    ParseKvpPair(kvpPair);
                        //}

                        //Parse File Info 1
                        if (line.Substring(0, 2) == "AA")
                        {
                            //Console.WriteLine("AA");
                            string[] kvpPair = line.Split('|');
                            ParseKvpPair(kvpPair);

                        }
                        //Parse File Info 2
                        else if (line.Substring(0, 2) == "HH")
                        {
                            //Console.WriteLine("HH");
                            string[] kvpPair = line.Split('|');
                            ParseKvpPair(kvpPair);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //[Helper Function]: Grabs data from DB and writes it to BillingReport file
        public static void ExtractDataFromDB(TextWriter tw)
        {
            //Connects to DB
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                // Create a command and set its connection to Billing.mdb
                OleDbCommand command = new OleDbCommand("SELECT * FROM Bills", connection);
                OleDbCommand command2 = new OleDbCommand("SELECT * FROM Customer", connection);

                //Create list and vairables to store lines
                List<string> billList = new List<string>();
                List<string> customerList = new List<string>();
                List<string> customerAddedList = new List<string>();
                string billsLine = "";
                string customerLine = "";

                // Open the connection and execute the select command.  
                try
                {
                    connection.Open();
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Read line of Bills Table and store to billsLine
                            billsLine = reader["ID"].ToString()
                                + "," + DateTime.Parse((string)reader["BillDate"].ToString()).ToString("MM/dd/yyyy") //Date showing only MM-dd-yyyy with no timestamp.
                                + "," + reader["BillNumber"].ToString()
                                + "," + reader["AccountBalance"].ToString()
                                + "," + DateTime.Parse((string)reader["DueDate"].ToString()).ToString("MM/dd/yyyy")
                                + "," + reader["BillAmount"].ToString()
                                + "," + reader["FormatGUID"].ToString();

                            //Add line to billList
                            billList.Add(billsLine);
                        }
                    }

                    using (OleDbDataReader reader2 = command2.ExecuteReader())
                    {
                        while (reader2.Read())
                        {
                            //Read line of Customers Table and store to customersline
                            customerLine = reader2["ID"].ToString()
                                + "," + reader2["CustomerName"].ToString()
                                + "," + reader2["AccountNumber"].ToString()
                                + "," + reader2["CustomerAddress"].ToString()
                                + "," + reader2["CustomerCity"].ToString()
                                + "," + reader2["CustomerState"].ToString()
                                + "," + reader2["CustomerZip"].ToString();

                            //Add line to customerList
                            customerList.Add(customerLine);

                            //Add customer added date to another list to get CSV format right
                            customerAddedList.Add(DateTime.Parse((string)reader2["DateAdded"].ToString()).ToString("MM/dd/yyyy"));
                        }
                    }

                    int i = 0;
                    while (i < customerList.Count)
                    {
                        //comebine two lines and write to tw.
                        tw.WriteLine(customerList[i] + billList[i] + "," + customerAddedList[i]);
                        i++;
                    }
                    connection.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        //[Required Function]: Creates CSV file to output folder
        public static void ExportDataDBToCSV()
        {
            //Billing Report header CSV format
            string headerCSV = "Customer.ID,Customer.CustomerName,Customer.AccountNumber," +
                "Customer.CustomerAddress,Customer.CustomerCity,Customer.CustomerState," +
                "Customer.CustomerZip,Bills.ID,Bills.BillDate,Bills.BillNumber," +
                "Bills.AccountBalance,Bills.DueDate,Bills.BillAmount," +
                "Bills.FormatGUID,Customer.DateAdded";

            string path = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            path = Directory.GetParent(Directory.GetParent(path).FullName).FullName + "\\_output\\BillingReport.txt"; //Fetches path to this folder directory
            try
            {
                // Check if file already exists. If yes, delete it. Make a new one
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                //Begin writing BillingReport.txt
                TextWriter tw = new StreamWriter(path, true);
                tw.WriteLine(headerCSV);
                ExtractDataFromDB(tw);
                tw.WriteLine();
                tw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void Main(string[] args)
        {
            //Open, read, and parse XML file
            ParseXML();

            //Create file BillFile-mmddyyyy.rpt
            BuildBillFile();

            //Access Billing.mdb database and import data to tables
            ImportDataToMDB();

            //Export data from Billing.mdb and import data to CSV
            ExportDataDBToCSV();

            //[Helpder function] Displays parsed data to Console App
            //PrintMyParsedData();

            //[Helper function] Displays raw XML text to Console App
            //PrintAllXMLToConsole();

            Console.WriteLine("Completed Assignment...");
        }
    }
}