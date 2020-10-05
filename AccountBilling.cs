using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICAssessment_ChristianMartinez
{
    public class AccountBilling
    {
        public int Id { get; set; }
        public BillHeader BillHeaderInfo { get; set; }
        public Bill BillInfo { get; set; }
        public AddressInformation AddressInfo { get; set; }

        public AccountBilling(BillHeader newbillheader, Bill newbill, AddressInformation newaddressinfo)
        {
            BillHeaderInfo = newbillheader;
            BillInfo = newbill;
            AddressInfo = newaddressinfo;
        }
    }
}
