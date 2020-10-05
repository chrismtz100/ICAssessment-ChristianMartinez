using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICAssessment_ChristianMartinez
{
    public class Bill //<Bill> info </Bill>
    {
        public double BillAmount { get; set; } //Bill Amount
        public double BalanceDue { get; set; } //Balance Due
        public string BillRunDt { get; set; } //Bill Run Date
        public int BillRunSeq { get; set; } //Bill Run Sequence
        public int BillRunTm { get; set; } //Bill Run Tm
        public string BillType { get; set; } //Bill Type

        public Bill(double billamount, double balancedue, string billrundt, int billrunseq, int billruntm, string billtype)
        {
            BillAmount = billamount;
            BalanceDue = balancedue;
            BillRunDt = billrundt;
            BillRunSeq = billrunseq;
            BillRunTm = billruntm;
            BillType = billtype;
        }

        public void Print()
        {
            Console.WriteLine("<--Bill-->");
            Console.WriteLine(this.BillAmount);
            Console.WriteLine(this.BalanceDue);
            Console.WriteLine(this.BillRunDt);
            Console.WriteLine(this.BillRunSeq);
            Console.WriteLine(this.BillRunTm);
            Console.WriteLine(this.BillType);
        }
    }
}
