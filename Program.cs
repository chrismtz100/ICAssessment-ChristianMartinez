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
        private static string connectionString = UpdateConnectionString();

        public static string UpdateConnectionString()
        {
            string part1 = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=";
            string path = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            path = Directory.GetParent(Directory.GetParent(path).FullName).FullName + "\\_database\\Billing.mdb";
            string part3 = ";Persist Security Info=True";
            connectionString = part1 + path + part3;
            return connectionString;
        }

        //[Helper Function]: Parses only Bill Header children only
        public static void ParseBillHeaderXML()
        {
            string path = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            path = Directory.GetParent(Directory.GetParent(path).FullName).FullName + "\\_database\\BillFile.xml"; //Change to local files later. 

            XDocument xdoc = XDocument.Load(path);
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

        //[Helper Function]: Parses only Bill children only
        public static void ParseBillXML()
        {
            string path = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            path = Directory.GetParent(Directory.GetParent(path).FullName).FullName + "\\_database\\BillFile.xml"; //Change to local files later. 
            XDocument xdoc = XDocument.Load(path);
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

        //[Helper Function]: Parses only Address Information children only
        public static void ParseAddressXML()
        {
            string path = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            path = Directory.GetParent(Directory.GetParent(path).FullName).FullName + "\\_database\\BillFile.xml"; //Change to local files later. 
            XDocument xdoc = XDocument.Load(path);
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
            double sum = 0;
            for (int i = 0; i < InvoiceDataset.Count; i++)
            {
                sum += InvoiceDataset[i].BillInfo.BillAmount;
            }

            Dictionary<int, string> header = new Dictionary<int, string> {
                {1, "FR" }, //?
                {2, GUID }, //Globally Unique Identifier
                {3, "Sample UT file" }, //?
                {4, DateTime.Now.ToString("MM-dd-yyyy") }, //DateTime.Now.ToString("MM-dd-yyyy");
                {5, InvoiceDataset.Count.ToString() }, //InvoiceDataset.Count;
                {6, sum.ToString("0.00") } //bill_amount_sum = bill.values[i]
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

        //[Helper Function]: Turns XML Parsed information content to KPV format for "BillFile-mmddyyyy.rpt"
        public static void BuildFileBill(TextWriter tw)
        {
            for (int i = 0; i < InvoiceDataset.Count; i++)
            {
                Dictionary<string, string> invoice = new Dictionary<string, string>{
                    {"AA", "CT" }, //?
                    {"BB", InvoiceDataset[i].BillHeaderInfo.AccountNo }, //InvoiceDataset[i].BillHeader.Account_No;
                    {"VV", InvoiceDataset[i].BillHeaderInfo.CustomerName }, //InvoiceDataset[i].BillHeader.Customer_Name;
                    {"CC", InvoiceDataset[i].AddressInfo.MailingAddress1 }, //InvoiceDataset[i].Address_Information.Mailing_Address_1
                    {"DD", InvoiceDataset[i].AddressInfo.MailingAddress2 }, //InvoiceDataset[i].BillHeader.Account_No
                    {"EE", InvoiceDataset[i].AddressInfo.City },
                    {"FF", InvoiceDataset[i].AddressInfo.State },
                    {"GG", InvoiceDataset[i].AddressInfo.Zip },

                    {"HH", "IH" }, //?
                    {"II", "R" }, //?
                    {"JJ", InvoiceFormat },
                    {"KK", InvoiceDataset[i].BillHeaderInfo.InvoiceNo },
                    {"LL", InvoiceDataset[i].BillHeaderInfo.BillDt },
                    {"MM", InvoiceDataset[i].BillHeaderInfo.DueDt },
                    {"NN", InvoiceDataset[i].BillInfo.BillAmount.ToString("0.00") },
                    {"OO", DateTime.Now.AddDays(5).ToString("MM-dd-yyyy") },
                    {"PP", DateTime.Parse(InvoiceDataset[i].BillHeaderInfo.DueDt).AddDays(-3).ToString("MM-dd-yyyy") },
                    {"QQ", InvoiceDataset[i].BillInfo.BalanceDue.ToString("0.00") },
                    {"RR", DateTime.Now.ToString("MM-dd-yyyy") },
                    {"SS", "SERVICE_ADDRESS" } //?
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
            string date = DateTime.Now.ToString("MM-dd-yyyy");
            string dateformated = Regex.Replace(date, @"[^0-9]", "");
            string filename = "BillFile-" + dateformated + ".rpt";

            string path = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            path = Directory.GetParent(Directory.GetParent(path).FullName).FullName + "\\_output\\" + filename; //Change to local files later. 
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

        //[Helper Function]: Given an array of KVP pairs from "BillFile-mmddyyyy.rpt", parse pair and place them inside of MS Access Database.
        public static void ParseKvpPair(string[] kvpPair)
        {
            Hashtable parsedKVP = new Hashtable();
            foreach (var pair in kvpPair)
            {
                string[] kvpValues = pair.Split('~');
                parsedKVP.Add(kvpValues[0], kvpValues[1]);
            }

            OleDbConnection con = new OleDbConnection(connectionString);
            OleDbCommand cmd = new OleDbCommand();
            con.Open();
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
            string date = DateTime.Now.ToString("MM-dd-yyyy");
            string dateformated = Regex.Replace(date, @"[^0-9]", "");
            string filename = "BillFile-" + dateformated + ".rpt";

            string path = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            path = Directory.GetParent(Directory.GetParent(path).FullName).FullName + "\\_output\\" + filename; //Change to local files later. 
            try
            {
                using (var streamReader = File.OpenText(path))
                {
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

        public static void ExtractDataFromDB(TextWriter tw)
        {
            //OleDbDataReader reader = null;

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                // Create a command and set its connection  
                OleDbCommand command = new OleDbCommand("SELECT * FROM Bills", connection);
                OleDbCommand command2 = new OleDbCommand("SELECT * FROM Customer", connection);
                List<string> billstable = new List<string>();
                List<string> customerstable = new List<string>();
                List<string> customeraddedtable = new List<string>();
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
                            //Read first line of Bills Table and store to billsLine
                            billsLine = reader["ID"].ToString()
                                + "," + DateTime.Parse((string)reader["BillDate"].ToString()).ToString("MM-dd-yyyy") //Date showing only MM-dd-yyyy with no timestamp.
                                + "," + reader["BillNumber"].ToString()
                                + "," + reader["AccountBalance"].ToString()
                                + "," + DateTime.Parse((string)reader["DueDate"].ToString()).ToString("MM-dd-yyyy")
                                + "," + reader["BillAmount"].ToString()
                                + "," + reader["FormatGUID"].ToString();

                            //Add line to billstable
                            billstable.Add(billsLine);
                        }
                    }

                    using (OleDbDataReader reader2 = command2.ExecuteReader())
                    {
                        while (reader2.Read())
                        {
                            //Read first line of Customers Table and store to customersline
                            customerLine = reader2["ID"].ToString()
                                + "," + reader2["CustomerName"].ToString()
                                + "," + reader2["AccountNumber"].ToString()
                                + "," + reader2["CustomerAddress"].ToString()
                                + "," + reader2["CustomerCity"].ToString()
                                + "," + reader2["CustomerState"].ToString()
                                + "," + reader2["CustomerZip"].ToString();

                            //Add line to billstable
                            customerstable.Add(customerLine);

                            //Add customer added date to another list to get CSV format right
                            customeraddedtable.Add(DateTime.Parse((string)reader2["DateAdded"].ToString()).ToString("MM-dd-yyyy"));
                        }
                    }


                    int i = 0;
                    while (i < customerstable.Count)
                    {
                        //comebine two lines and write to tw.
                        tw.WriteLine(customerstable[i] + billstable[i] + "," + customeraddedtable[i]);
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
            string headerCSV = "Customer.ID,Customer.CustomerName,Customer.AccountNumber," +
                "Customer.CustomerAddress,Customer.CustomerCity,Customer.CustomerState," +
                "Customer.CustomerZip,Bills.ID,Bills.BillDate,Bills.BillNumber," +
                "Bills.AccountBalance,Bills.DueDate,Bills.BillAmount," +
                "Bills.FormatGUID,Customer.DateAdded";
            //string path = "C:\\Users\\Christian\\Desktop\\ICAssessment-ChristianMartinez\\_output\\BillingReport.txt"; //Change to local files later. 

            string path = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            path = Directory.GetParent(Directory.GetParent(path).FullName).FullName + "\\_output\\BillingReport.txt"; //Change to local files later. 
            try
            {
                // Check if file already exists. If yes, delete it.     
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                //Begin writing billfile
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
            //ExportDataDBToCSV();

            //[Helpder function] Displays parsed data to Console App
            //PrintMyParsedData();

            //[Helper function] Displays raw XML text to Console App
            //PrintAllXMLToConsole();

            Console.WriteLine("Completed Assignment...");
        }
    }
}