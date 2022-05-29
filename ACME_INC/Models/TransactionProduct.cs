using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACME_INC.Models
{
    public class TransactionProduct
    {
        private string productID;
        private string productName;
        private string productTotal;
        private string productdate;
        private string productorderID;

        public TransactionProduct(string productID, string productName, string productTotal, string productdate, string productorderID)
        {
            this.productID = productID;
            this.productName = productName;
            this.productTotal = productTotal;
            this.productdate = productdate;
            this.productorderID = productorderID;
        }

        public string ProductID { get => productID; set => productID = value; }
        public string ProductName { get => productName; set => productName = value; }
        public string ProductTotal { get => productTotal; set => productTotal = value; }
        public string Productdate { get => productdate; set => productdate = value; }
        public string ProductorderID { get => productorderID; set => productorderID = value; }
    }
}
