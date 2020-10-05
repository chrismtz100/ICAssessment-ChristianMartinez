using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICAssessment_ChristianMartinez
{
    public class AddressInformation //<Address_Information> info </Address_Information>
    {
        public string MailingAddress1 { get; set; } //Mailing Address 1
        public string MailingAddress2 { get; set; } //Mailing Address 2
        public string City { get; set; } //City
        public string State { get; set; } //State
        public string Zip { get; set; } //Zip

        public AddressInformation(string mailingaddress1, string mailingaddress2, string city, string state, string zip)
        {
            MailingAddress1 = mailingaddress1;
            MailingAddress2 = mailingaddress2;
            City = city;
            State = state;
            Zip = zip;
        }

        public void Print()
        {
            Console.WriteLine("<--Address Information-->");
            Console.WriteLine(this.MailingAddress1);
            Console.WriteLine(this.MailingAddress2);
            Console.WriteLine(this.City);
            Console.WriteLine(this.State);
            Console.WriteLine(this.Zip);
        }
    }
}
