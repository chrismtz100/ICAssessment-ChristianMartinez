using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICAssessment_ChristianMartinez
{
    public class BillHeader //<BillHeader> info </BillHeader>
    {
        public string InvoiceNo { get; set; } //Invoice Number
        public string AccountNo { get; set; } //Account Number
        public string CustomerName { get; set; } //Customer Name
        public string CycleCd { get; set; } //Cycle Cd
        public string BillDt { get; set; } //Bill Date
        public string DueDt { get; set; } //Due Date
        public string AccountClass { get; set; } //Account Class

        public BillHeader(string invoiceno, string accountno, string customername, 
            string cyclecd, string billdt, string duedt, string accountclass)
        {
            InvoiceNo = invoiceno;
            AccountNo = accountno;
            CustomerName = customername;
            CycleCd = cyclecd;
            BillDt = billdt;
            DueDt = duedt;
            AccountClass = accountclass;
        }

        public void Print()
        {
            Console.WriteLine("<--Bill Header-->");
            Console.WriteLine(this.InvoiceNo);
            Console.WriteLine(this.AccountNo);
            Console.WriteLine(this.CustomerName);
            Console.WriteLine(this.CycleCd);
            Console.WriteLine(this.BillDt);
            Console.WriteLine(this.DueDt);
            Console.WriteLine(this.AccountClass);
        }
    }
}
